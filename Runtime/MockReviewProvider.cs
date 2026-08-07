using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Editor + non-Android provider. Replays a <see cref="ReviewMockConfig"/> scenario
    /// by firing <see cref="OnReviewFlowCompleted"/> or <see cref="OnError"/> after the
    /// configured delay. Honors the shared <see cref="ReviewCoolDownLogic"/> so cooldown
    /// behavior matches the Android path.
    /// The config is optional. <see cref="ReviewController"/> only ever exists via its own
    /// Instance getter, which builds it with AddComponent, so its Inspector-only
    /// <c>_mockConfig</c> field is null on that path and no assignment is possible. With no
    /// config supplied the default scenario is used, as Documentation~/troubleshooting.md
    /// already documents.
    /// </summary>
    public sealed class MockReviewProvider : IReviewProvider, IDisposable
    {
        private readonly ReviewMockConfig _cfg;
        private readonly ReviewCoolDownLogic _coolDown;
        private bool _disposed;

        public event Action<ReviewResult> OnReviewFlowCompleted;
        public event Action<ReviewError>  OnError;

        public DateTime? LastPromptTimeUtc => _coolDown.LastPromptTimeUtc;
        public bool CanRequestReview() => _coolDown.CanRequestReview();
        public TimeSpan CooldownRemaining() => _coolDown.Remaining();

        // `cfg != null`, not `cfg ??`: ReviewMockConfig is a UnityEngine.Object, and `??`
        // bypasses Unity's overloaded equality, so a destroyed asset would pass as non-null
        // and fail later with MissingReferenceException. coolDown keeps its throw — a null
        // cooldown is a wiring error, while a null cfg means "no scenario chosen" and has a
        // documented default. Matches MockAppUpdateProvider.
        public MockReviewProvider(ReviewMockConfig cfg, ReviewCoolDownLogic coolDown)
        {
            _cfg      = cfg != null ? cfg : CreateDefaultConfig();
            _coolDown = coolDown ?? throw new ArgumentNullException(nameof(coolDown));
        }

        private static ReviewMockConfig CreateDefaultConfig()
        {
            var cfg = ScriptableObject.CreateInstance<ReviewMockConfig>();
            cfg.hideFlags = HideFlags.HideAndDontSave;
            BizSimLogger.Info(
                "No ReviewMockConfig assigned — using the default mock scenario " +
                "(SimulatedDelaySeconds=0.5, SimulatedError=NoError). Assign a preset from " +
                "Samples~/MockPresets to ReviewController's Mock Config field to simulate " +
                "other scenarios.");
            return cfg;
        }

        public void RequestReview()
        {
            if (!_coolDown.CanRequestReview())
            {
                OnError?.Invoke(new ReviewError(
                    ReviewErrorCode.QuotaCooldownActive,
                    "Local cooldown active; cannot request a review yet.",
                    retryable: false,
                    occurredAtUtc: DateTime.UtcNow));
                return;
            }
            _ = RunAsync(CancellationToken.None);
        }

        public async Task<ReviewResult> RequestReviewAsync(CancellationToken ct, float timeoutSeconds)
        {
            if (!_coolDown.CanRequestReview())
            {
                var err = new ReviewError(
                    ReviewErrorCode.QuotaCooldownActive,
                    "Local cooldown active; cannot request a review yet.",
                    retryable: false,
                    occurredAtUtc: DateTime.UtcNow);
                OnError?.Invoke(err);
                throw new InvalidOperationException(err.Message);
            }
            return await RunAsync(ct);
        }

        public void ClearLocalCooldownForTesting() => _coolDown.ClearForTesting();

        private async Task<ReviewResult> RunAsync(CancellationToken ct)
        {
            var startedAt = DateTime.UtcNow;
            var delay = Math.Max(0f, _cfg.SimulatedDelaySeconds);
            if (delay > 0f)
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), ct);
            }
            else
            {
                // Yield once so callbacks fire asynchronously even at zero delay.
                await Task.Yield();
            }

            if (_cfg.SimulatedError != ReviewErrorCode.NoError)
            {
                var error = new ReviewError(
                    _cfg.SimulatedError,
                    $"MockReviewProvider simulated error: {_cfg.SimulatedError}",
                    retryable: ReviewError.IsRetryable(_cfg.SimulatedError),
                    occurredAtUtc: DateTime.UtcNow);
                OnError?.Invoke(error);
                throw new InvalidOperationException(error.Message);
            }

            // NOTE (audit round-3 Phase 3 nit-2): Controller is the sole cooldown stamper.
            // `ReviewController.HandleCompleted` calls _coolDown.StampNow() on success. Providers
            // MUST NOT stamp internally — prevents double-stamp on shared ReviewCoolDownLogic
            // instance. Same rule will apply to AndroidReviewProvider in Phase 4.

            var result = new ReviewResult(
                flowCompleted: true,
                completedAtUtc: DateTime.UtcNow,
                elapsed: DateTime.UtcNow - startedAt,
                source: ReviewFlowSource.Mock);

            BizSimLogger.Info(
                $"MockReviewProvider flow complete (SimulateFlowShown={_cfg.SimulateFlowShown}, elapsed={result.Elapsed.TotalMilliseconds:F0} ms)");

            OnReviewFlowCompleted?.Invoke(result);
            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // No native handles to release — pure C#.
        }
    }
}
