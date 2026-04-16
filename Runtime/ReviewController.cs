using System;
using System.Collections.Generic;
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

        private IReviewTriggerEngine _triggerEngine;
        private IReviewConfigSource _configSource;
        private IConsentGate _consentGate;
        private ReviewSessionTracker _sessionTracker;
        private readonly List<string> _recordedEvents = new();
        private readonly List<string> _recordedMilestones = new();
        private ReviewError? _lastError;

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

            // Enterprise Wave 1: initialize trigger engine defaults
            _configSource ??= new StaticReviewConfigSource();
            _consentGate ??= new AlwaysAllowConsentGate();
            _sessionTracker = new ReviewSessionTracker();
            _sessionTracker.RecordLaunch();
            _triggerEngine ??= new ReviewTriggerEngine(
                _configSource,
                _consentGate,
                defaultMinSessions: _settings != null ? _settings.FirstRunGraceSessions : 3,
                defaultMinDays: _settings != null ? _settings.FirstRunGraceDays : 7,
                offlineGuardEnabled: _settings != null && _settings.OfflineGuardEnabled);

            // S4 security: warn when kill switch and consent gate are defaults in dev builds
            if (Debug.isDebugBuild)
            {
                if (_configSource is StaticReviewConfigSource)
                    BizSimLogger.Warning(
                        "No custom IReviewConfigSource set — remote kill switch not wired. " +
                        "Call SetConfigSource() to enable field incident control.");

                if (_consentGate is AlwaysAllowConsentGate)
                    BizSimLogger.Warning(
                        "No custom IConsentGate set — review prompts will fire without " +
                        "consent verification. For GDPR/DMA regions, call SetConsentGate() " +
                        "with your CMP integration. See Documentation~/GDPR-INTEGRATION.md");
            }
        }

        public void RequestReview()
        {
            EnsureMainThread();
            if (_inFlight) throw new InvalidOperationException("Review request already in progress");

            // Enterprise Wave 1: trigger engine gate
            var decision = EvaluateTriggerEngine();
            if (decision.IsBlock)
            {
                BizSimLogger.Info($"Review request blocked by trigger engine: {decision}");
                try { _analytics?.OnReviewError(new ReviewError(ReviewErrorCode.QuotaCooldownActive, $"trigger_block: {decision.Reason}", false, DateTime.UtcNow)); }
                catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on trigger block: {ex.Message}"); }
                return;
            }

            // Enterprise Wave 1: dry-run mode
            if (IsDryRunActive())
            {
                BizSimLogger.Info("[DRY-RUN] Review request would proceed. Decision: " + decision);
                return;
            }

            _inFlight = true;
            try { _analytics?.OnReviewRequested(); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnReviewRequested: {ex.Message}"); }
            try
            {
                _provider.RequestReview();
            }
            catch
            {
                // Sync throw: provider rejected the call before starting. Release the in-flight slot
                // so the controller isn't stuck. The exception propagates to the caller.
                _inFlight = false;
                throw;
            }
        }

        // Sentinel-value pattern per CROSS-INVARIANTS §12.2.1: a non-positive timeoutSeconds
        // resolves to _settings.DefaultTimeoutSeconds; consumers who pass a positive value override.
        public async Task<ReviewResult> RequestReviewAsync(CancellationToken ct = default, float timeoutSeconds = -1f)
        {
            EnsureMainThread();
            if (_inFlight) throw new InvalidOperationException("Review request already in progress");

            // Enterprise Wave 1: trigger engine gate
            var decision = EvaluateTriggerEngine();
            if (decision.IsBlock)
            {
                BizSimLogger.Info($"Review request blocked by trigger engine: {decision}");
                try { _analytics?.OnReviewError(new ReviewError(ReviewErrorCode.QuotaCooldownActive, $"trigger_block: {decision.Reason}", false, DateTime.UtcNow)); }
                catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on trigger block: {ex.Message}"); }
                throw new InvalidOperationException($"Review blocked by trigger engine: {decision.Reason}");
            }

            // Enterprise Wave 1: dry-run mode
            if (IsDryRunActive())
            {
                BizSimLogger.Info("[DRY-RUN] Review request would proceed. Decision: " + decision);
                return new ReviewResult(false, DateTime.UtcNow, TimeSpan.Zero, ReviewFlowSource.Mock);
            }

            _inFlight = true;
            if (timeoutSeconds <= 0f)
                timeoutSeconds = _settings != null ? _settings.DefaultTimeoutSeconds : 30f;
            try { _analytics?.OnReviewRequested(); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnReviewRequested: {ex.Message}"); }

            // Enterprise Wave 1: watchdog timeout
            int watchdogMs = (_settings != null ? _settings.WatchdogTimeoutSeconds : 8) * 1000;
            using var watchdogCts = new CancellationTokenSource(watchdogMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, watchdogCts.Token);

            try
            {
                return await _provider.RequestReviewAsync(linkedCts.Token, timeoutSeconds);
            }
            catch (OperationCanceledException) when (watchdogCts.IsCancellationRequested && !ct.IsCancellationRequested)
            {
                // Watchdog fired, not the caller's token
                _inFlight = false;
                var error = new ReviewError(ReviewErrorCode.Timeout, "Watchdog timeout expired", true, DateTime.UtcNow);
                _lastError = error;
                try { _analytics?.OnReviewError(error); }
                catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on watchdog timeout: {ex.Message}"); }
                BizSimLogger.Warning($"Watchdog timeout ({watchdogMs}ms) expired during review flow");
                OnError?.Invoke(error);
                throw new TimeoutException($"Review flow watchdog timeout ({watchdogMs}ms) expired");
            }
            catch
            {
                _inFlight = false;
                throw;
            }
        }

        // Explicit interface implementation to satisfy IReviewProvider's positive-timeout contract.
        Task<ReviewResult> IReviewProvider.RequestReviewAsync(CancellationToken ct, float timeoutSeconds)
            => RequestReviewAsync(ct, timeoutSeconds);

        public void SetAnalyticsAdapter(IReviewAnalyticsAdapter adapter)
        {
            EnsureMainThread();
            _analytics = adapter;
        }

        // --- Enterprise Wave 1: Setter methods for trigger engine components ---

        public void SetTriggerEngine(IReviewTriggerEngine engine)
        {
            EnsureMainThread();
            _triggerEngine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public void SetConfigSource(IReviewConfigSource source)
        {
            EnsureMainThread();
            _configSource = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void SetConsentGate(IConsentGate gate)
        {
            EnsureMainThread();
            _consentGate = gate ?? throw new ArgumentNullException(nameof(gate));
        }

        // --- Enterprise Wave 1: Event and milestone recording ---

        public void RecordEvent(string eventName)
        {
            EnsureMainThread();
            if (eventName == null) throw new ArgumentNullException(nameof(eventName));
            _recordedEvents.Add(eventName);
            BizSimLogger.Verbose($"Event recorded: {eventName}");
        }

        public void RecordMilestone(string milestoneName)
        {
            EnsureMainThread();
            if (milestoneName == null) throw new ArgumentNullException(nameof(milestoneName));
            _recordedMilestones.Add(milestoneName);
            BizSimLogger.Verbose($"Milestone recorded: {milestoneName}");
        }

        public void RecordSession()
        {
            EnsureMainThread();
            _sessionTracker?.RecordSession();
        }

        // --- Enterprise Wave 1: Diagnostics ---

        public ReviewDiagnosticSnapshot GetDiagnosticSnapshot()
        {
            EnsureMainThread();
            return new ReviewDiagnosticSnapshot
            {
                SessionCount = _sessionTracker?.SessionCount ?? 0,
                LaunchCount = _sessionTracker?.LaunchCount ?? 0,
                DaysSinceInstall = _sessionTracker?.DaysSinceInstall ?? 0,
                CooldownActive = !(_coolDown?.CanRequestReview() ?? true),
                CooldownRemainingSeconds = (_coolDown?.Remaining() ?? TimeSpan.Zero).TotalSeconds.ToString("F0"),
                LastFlowTimestamp = _coolDown?.LastPromptTimeUtc?.ToString("O") ?? "",
                LastErrorCode = _lastError?.Code.ToString() ?? "",
                RemoteEnabled = _configSource?.RemoteEnabled ?? true,
                ConsentGranted = _consentGate?.IsConsented(BuildTriggerContext()) ?? true,
                OfflineGuardTriggered = _settings != null && _settings.OfflineGuardEnabled
                    && Application.internetReachability == NetworkReachability.NotReachable,
                ConfigSourceType = _configSource?.GetType().Name ?? "null",
                TriggerEngineType = _triggerEngine?.GetType().Name ?? "null",
                AppVersion = Application.version,
                PackageVersion = Review.PackageVersion.Current,
                SnapshotTimestamp = DateTime.UtcNow.ToString("O")
            };
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
            _lastError = e;
            try { _analytics?.OnReviewError(e); }
            catch (Exception ex) { BizSimLogger.Warning($"analytics adapter threw on OnReviewError: {ex.Message}"); }

            // Enterprise Wave 1: Play Store fallback on hard errors
            if (e.Code == ReviewErrorCode.PlayStoreNotFound || e.Code == ReviewErrorCode.BridgeNotInitialized)
            {
                BizSimLogger.Info($"Hard error ({e.Code}) — attempting Play Store fallback");
                try
                {
                    ReviewPlayStoreFallback.OpenPlayStoreListing(Application.identifier);
                }
                catch (Exception fallbackEx)
                {
                    BizSimLogger.Warning($"Play Store fallback failed: {fallbackEx.Message}");
                }
            }

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

        private ReviewTriggerContext BuildTriggerContext()
        {
            return new ReviewTriggerContext(
                sessionCount: _sessionTracker?.SessionCount ?? 0,
                launchCount: _sessionTracker?.LaunchCount ?? 0,
                daysSinceInstall: _sessionTracker?.DaysSinceInstall ?? 0,
                lastFlowTimestamp: _coolDown?.LastPromptTimeUtc ?? DateTime.MinValue,
                eventsRecorded: _recordedEvents.ToArray(),
                milestonesReached: _recordedMilestones.ToArray(),
                appVersion: Application.version);
        }

        private TriggerDecision EvaluateTriggerEngine()
        {
            if (_triggerEngine == null) return TriggerDecision.Allow;
            var context = BuildTriggerContext();
            return _triggerEngine.Evaluate(context);
        }

        private bool IsDryRunActive()
        {
            return _settings != null && _settings.DryRunMode && Debug.isDebugBuild;
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
