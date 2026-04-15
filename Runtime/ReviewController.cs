using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Top-level singleton for the review flow. Consumer code calls
    /// <c>ReviewController.Instance.RequestReviewAsync()</c> or subscribes to events.
    /// All public methods are main-thread only; calling from a background thread throws
    /// <see cref="InvalidOperationException"/>.
    /// </summary>
    public sealed class ReviewController : MonoBehaviour, IReviewProvider
    {
        private static ReviewController _instance;
        public static ReviewController Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;
                var go = new GameObject("[ReviewController]");
                _instance = go.AddComponent<ReviewController>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        [SerializeField] private ReviewMockConfig _mockConfig;
        // NOTE (post-freeze r5, 2026-04-15): duplicate [SerializeField] fields
        // _minPromptIntervalDays / _timeoutSeconds / _logLevel / _useMockInDevelopmentBuild
        // have been REMOVED per CROSS-INVARIANTS §12. The ReviewSettings asset is the
        // SOLE source of truth for those values.

        private ReviewSettings _settings;
        private IReviewProvider _provider;
        private ReviewCoolDownLogic _coolDown;
        private IReviewAnalyticsAdapter _analytics;
        private int _mainThreadId;
        private bool _inFlight;

        public event Action<ReviewResult> OnReviewFlowCompleted;
        public event Action<ReviewError>  OnError;

        public bool IsRequestInFlight => _inFlight;
        public DateTime? LastPromptTimeUtc => _coolDown?.LastPromptTimeUtc;
        public bool CanRequestReview() { EnsureMainThread(); return _coolDown?.CanRequestReview() ?? false; }
        public TimeSpan CooldownRemaining() { EnsureMainThread(); return _coolDown?.Remaining() ?? TimeSpan.Zero; }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

            // Post-r5 per CROSS-INVARIANTS §12: load project-wide Settings asset.
            // BizSimLogger reads the same asset via its own cache.
            _settings = Resources.Load<ReviewSettings>(ReviewSettings.ResourcesLoadKey);
            int  intervalDays    = _settings != null ? _settings.MinPromptIntervalDays : 90;
            float defaultTimeout = _settings != null ? _settings.DefaultTimeoutSeconds : 30f;
            _coolDown = new ReviewCoolDownLogic(intervalDays);

#if UNITY_ANDROID && !UNITY_EDITOR
  #if DEVELOPMENT_BUILD
            if (_settings != null && _settings.UseMockInDevelopmentBuild)
            {
                _provider = new MockReviewProvider(_mockConfig, _coolDown);
                BizSimLogger.Info("Development build: using MockReviewProvider per ReviewSettings.UseMockInDevelopmentBuild");
            }
            else
  #endif
            {
                _provider = new AndroidReviewProvider(_coolDown, defaultTimeout);
            }
#else
            _provider = new MockReviewProvider(_mockConfig, _coolDown);
#endif

            _provider.OnReviewFlowCompleted += HandleCompleted;
            _provider.OnError += HandleError;
        }

        public void RequestReview()
        {
            EnsureMainThread();
            if (_inFlight) throw new InvalidOperationException("Review request already in progress");
            _inFlight = true;
            try { _analytics?.OnReviewRequested(); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnReviewRequested: {ex.Message}"); }
            _provider.RequestReview();
        }

        // Sentinel-value pattern per CROSS-INVARIANTS §12.2.1: a non-positive timeoutSeconds
        // resolves to _settings.DefaultTimeoutSeconds; consumers who pass a positive value override.
        public Task<ReviewResult> RequestReviewAsync(CancellationToken ct = default, float timeoutSeconds = -1f)
        {
            EnsureMainThread();
            if (_inFlight) throw new InvalidOperationException("Review request already in progress");
            _inFlight = true;
            if (timeoutSeconds <= 0f)
                timeoutSeconds = _settings != null ? _settings.DefaultTimeoutSeconds : 30f;
            try { _analytics?.OnReviewRequested(); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnReviewRequested: {ex.Message}"); }
            return _provider.RequestReviewAsync(ct, timeoutSeconds);
        }

        // Explicit interface implementation to satisfy IReviewProvider's positive-timeout contract.
        Task<ReviewResult> IReviewProvider.RequestReviewAsync(CancellationToken ct, float timeoutSeconds)
            => RequestReviewAsync(ct, timeoutSeconds);

        public void SetAnalyticsAdapter(IReviewAnalyticsAdapter adapter)
        {
            EnsureMainThread();
            _analytics = adapter;
        }

        // QA escape hatch — NOT guarded by EnsureMainThread per Thread-safety contract.
        public void ClearLocalCooldownForTesting()
        {
            _coolDown?.ClearForTesting();
            try { _analytics?.OnLocalCooldownCleared(); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnLocalCooldownCleared: {ex.Message}"); }
        }

        private void HandleCompleted(ReviewResult r)
        {
            _inFlight = false;
            _coolDown.StampNow();
            try { _analytics?.OnReviewFlowCompleted(r); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnReviewFlowCompleted: {ex.Message}"); }
            OnReviewFlowCompleted?.Invoke(r);
        }

        private void HandleError(ReviewError e)
        {
            _inFlight = false;
            try { _analytics?.OnReviewError(e); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnReviewError: {ex.Message}"); }
            OnError?.Invoke(e);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            if (_provider is IDisposable d)
            {
                try { d.Dispose(); }
                catch (Exception ex) { BizSimLogger.Warning($"provider dispose threw: {ex.Message}"); }
            }
        }

        private void EnsureMainThread([CallerMemberName] string caller = null)
        {
            if (Thread.CurrentThread.ManagedThreadId != _mainThreadId)
            {
                throw new InvalidOperationException(
                    $"ReviewController.{caller} must be called from the Unity main thread " +
                    $"(was called from thread id {Thread.CurrentThread.ManagedThreadId}).");
            }
        }
    }
}
