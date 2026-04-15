using UnityEngine;

namespace BizSim.Google.Play.Review
{
    // Stub — full content + decorations land in Phase 5 Step 13.0a per CROSS-INVARIANTS §12.
    public sealed class ReviewSettings : ScriptableObject
    {
        // Path constants per CROSS-INVARIANTS §12.5 — keep the two in sync.
        public const string ResourcesLoadKey  = "BizSim/GooglePlay/ReviewSettings";
        public const string AssetDatabasePath = "Assets/Resources/" + ResourcesLoadKey + ".asset";

        public bool LogsEnabled = true;
        public BizSimLogger.LogLevel LogLevel = BizSimLogger.LogLevel.Info;
        public bool UseMockInDevelopmentBuild = false;
        public bool EnableAnalyticsByDefault = false;
        public int MinPromptIntervalDays = 90;
        public float DefaultTimeoutSeconds = 30f;
    }
}
