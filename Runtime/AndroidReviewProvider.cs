#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Android JNI provider. Wraps the Java <c>ReviewBridge</c> class via
    /// <see cref="AndroidJavaClass"/> and <see cref="AndroidJavaObject"/>. Compiled only under
    /// <c>UNITY_ANDROID &amp;&amp; !UNITY_EDITOR</c>. The editor and non-Android build paths use
    /// <see cref="MockReviewProvider"/>.
    /// </summary>
    internal sealed class AndroidReviewProvider : IReviewProvider, IDisposable
    {
        private readonly ReviewCoolDownLogic _coolDown;
        private readonly float _defaultTimeoutSeconds;

        private AndroidJavaObject _bridge;
        private ReviewCallbackProxy _proxy;
        private TaskCompletionSource<ReviewResult> _pendingTcs;
        private DateTime _pendingStartedAt;
        private bool _disposed;

        public event Action<ReviewResult> OnReviewFlowCompleted;
        public event Action<ReviewError>  OnError;

        public DateTime? LastPromptTimeUtc => _coolDown.LastPromptTimeUtc;
        public bool CanRequestReview() => _coolDown.CanRequestReview();
        public TimeSpan CooldownRemaining() => _coolDown.Remaining();

        public AndroidReviewProvider(ReviewCoolDownLogic coolDown, float defaultTimeoutSeconds)
        {
            _coolDown = coolDown ?? throw new ArgumentNullException(nameof(coolDown));
            _defaultTimeoutSeconds = defaultTimeoutSeconds > 0f ? defaultTimeoutSeconds : 30f;
            EnsureBridge();
        }

        private void EnsureBridge()
        {
            if (_bridge != null) return;
            try
            {
                using (var bridgeClass = new AndroidJavaClass("com.bizsim.google.play.review.ReviewBridge"))
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (currentActivity == null)
                    {
                        BizSimLogger.Error("AndroidReviewProvider: UnityPlayer.currentActivity is null — cannot create bridge");
                        return;
                    }
                    _bridge = bridgeClass.CallStatic<AndroidJavaObject>("create", currentActivity);
                }
                _proxy = new ReviewCallbackProxy(HandleFlowCompletedFromJni, HandleErrorFromJni);
                _bridge.Call("setCallback", _proxy);
            }
            catch (Exception ex)
            {
                BizSimLogger.Error($"AndroidReviewProvider: bridge init failed: {ex.Message}");
                _bridge?.Dispose();
                _bridge = null;
                _proxy = null;
            }
        }

        public void RequestReview()
        {
            if (!_coolDown.CanRequestReview())
            {
                EmitCooldownError();
                return;
            }
            if (_bridge == null)
            {
                EmitError(ReviewErrorCode.BridgeNotInitialized, "ReviewBridge not initialized");
                return;
            }
            _pendingStartedAt = DateTime.UtcNow;
            try
            {
                _bridge.Call("requestReviewFlow");
            }
            catch (Exception ex)
            {
                EmitError(ReviewErrorCode.InternalError, ex.Message);
            }
        }

        public async Task<ReviewResult> RequestReviewAsync(CancellationToken ct, float timeoutSeconds)
        {
            if (!_coolDown.CanRequestReview())
            {
                var err = MakeCooldownError();
                OnError?.Invoke(err);
                throw new InvalidOperationException(err.Message);
            }
            if (_bridge == null)
            {
                var err = new ReviewError(ReviewErrorCode.BridgeNotInitialized,
                    "ReviewBridge not initialized",
                    retryable: true,
                    occurredAtUtc: DateTime.UtcNow);
                OnError?.Invoke(err);
                throw new InvalidOperationException(err.Message);
            }

            if (timeoutSeconds <= 0f) timeoutSeconds = _defaultTimeoutSeconds;
            _pendingTcs = new TaskCompletionSource<ReviewResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingStartedAt = DateTime.UtcNow;

            try
            {
                _bridge.Call("requestReviewFlow");
            }
            catch (Exception ex)
            {
                _pendingTcs.TrySetException(ex);
                var err = new ReviewError(ReviewErrorCode.InternalError, ex.Message,
                    retryable: true, occurredAtUtc: DateTime.UtcNow);
                OnError?.Invoke(err);
                throw;
            }

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), ct);
            var completed   = await Task.WhenAny(_pendingTcs.Task, timeoutTask);
            if (completed == timeoutTask)
            {
                ct.ThrowIfCancellationRequested();
                // Timeout: drop the pending TCS reference so a late JNI callback does NOT fire the
                // consumer event (per §4 cancellation contract). Log at Info.
                var stalePending = _pendingTcs;
                _pendingTcs = null;
                var err = new ReviewError(ReviewErrorCode.Timeout,
                    $"RequestReviewAsync timed out after {timeoutSeconds:F1}s",
                    retryable: true,
                    occurredAtUtc: DateTime.UtcNow);
                BizSimLogger.Info($"Review flow timed out after {timeoutSeconds:F1}s — late callbacks will be logged but not surfaced");
                stalePending.TrySetException(new TimeoutException(err.Message));
                OnError?.Invoke(err);
                throw new TimeoutException(err.Message);
            }
            return await _pendingTcs.Task;
        }

        public void ClearLocalCooldownForTesting() => _coolDown.ClearForTesting();

        // ----- JNI callback handlers (called on main thread via UnityMainThreadDispatcher) -----

        private void HandleFlowCompletedFromJni()
        {
            var elapsed = DateTime.UtcNow - _pendingStartedAt;
            var result = new ReviewResult(
                flowCompleted: true,
                completedAtUtc: DateTime.UtcNow,
                elapsed: elapsed,
                source: ReviewFlowSource.Android);

            var tcs = _pendingTcs;
            if (tcs == null)
            {
                BizSimLogger.Info("Late review flow completion received after timeout — ignored");
                return;
            }
            _pendingTcs = null;
            tcs.TrySetResult(result);
            OnReviewFlowCompleted?.Invoke(result);
        }

        private void HandleErrorFromJni(int code, string message)
        {
            var mapped = (ReviewErrorCode)code;
            var err = new ReviewError(mapped, message ?? "",
                retryable: ReviewError.IsRetryable(mapped),
                occurredAtUtc: DateTime.UtcNow);

            var tcs = _pendingTcs;
            if (tcs == null)
            {
                BizSimLogger.Info($"Late review error received after timeout (code={code}) — ignored");
                return;
            }
            _pendingTcs = null;
            tcs.TrySetException(new InvalidOperationException(err.Message));
            OnError?.Invoke(err);
        }

        // ----- Error helpers -----

        private void EmitCooldownError() => OnError?.Invoke(MakeCooldownError());

        private ReviewError MakeCooldownError() => new ReviewError(
            ReviewErrorCode.QuotaCooldownActive,
            "Local cooldown active; cannot request a review yet.",
            retryable: false,
            occurredAtUtc: DateTime.UtcNow);

        private void EmitError(ReviewErrorCode code, string message)
            => OnError?.Invoke(new ReviewError(code, message ?? "",
                retryable: ReviewError.IsRetryable(code),
                occurredAtUtc: DateTime.UtcNow));

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _bridge?.Dispose(); } catch { /* ignore */ }
            _bridge = null;
            _proxy = null;
        }
    }
}
#endif
