using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Project-wide settings asset for the Review package. Editable via
    /// <c>BizSim → Google Play → Review → Configuration</c> editor window per
    /// <c>CROSS-PACKAGE-INVARIANTS.md §12</c>. The asset lives at
    /// <c>Assets/Resources/BizSim/GooglePlay/ReviewSettings.asset</c>.
    /// </summary>
    [CreateAssetMenu(
        menuName = "BizSim/Google Play/Review Settings",
        fileName = "ReviewSettings",
        order = 0)]
    public sealed class ReviewSettings : ScriptableObject
    {
        // Path constants per CROSS-INVARIANTS §12.5 — keep the two in sync.
        public const string ResourcesLoadKey  = "BizSim/GooglePlay/ReviewSettings";
        public const string AssetDatabasePath = "Assets/Resources/" + ResourcesLoadKey + ".asset";

        [Header("Logging")]
        [Tooltip("Master switch — when false, BizSimLogger is a no-op for this package regardless of LogLevel.")]
        public bool LogsEnabled = true;

        [Tooltip("Minimum severity printed when LogsEnabled is true.")]
        public BizSimLogger.LogLevel LogLevel = BizSimLogger.LogLevel.Info;

        [Header("Editor / Development")]
        [Tooltip("If true, builds with DEVELOPMENT_BUILD use MockProvider instead of the real JNI provider. Release builds always use real provider regardless.")]
        public bool UseMockInDevelopmentBuild = false;

        [Header("Analytics")]
        [Tooltip("If true, the controller registers a default Firebase analytics adapter at Awake (when BIZSIM_FIREBASE is defined).")]
        public bool EnableAnalyticsByDefault = false;

        [Header("Review-specific")]
        [Tooltip("Minimum days between in-game review prompts (local cooldown).")]
        [Range(1, 365)]
        public int MinPromptIntervalDays = 90;

        [Tooltip("Async review-flow timeout (seconds). Caller can override per-call by passing a positive value to *Async methods.")]
        [Range(5f, 120f)]
        public float DefaultTimeoutSeconds = 30f;

        [Header("Trigger Engine (Wave 1)")]
        [Tooltip("Minimum sessions before first review prompt")]
        public int FirstRunGraceSessions = 3;

        [Tooltip("Minimum days since install before first review prompt")]
        public int FirstRunGraceDays = 7;

        [Tooltip("Internal watchdog timeout in seconds (3-60)")]
        [Range(3, 60)]
        public int WatchdogTimeoutSeconds = 8;

        [Tooltip("Skip review flow when device is offline")]
        public bool OfflineGuardEnabled = true;

        [Tooltip("Log full decision tree without invoking provider (dev builds only)")]
        public bool DryRunMode;
    }
}
