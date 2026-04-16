using System;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    [Serializable]
    public class ReviewDiagnosticSnapshot
    {
        public int SchemaVersion = 1;
        public int SessionCount;
        public int LaunchCount;
        public int DaysSinceInstall;
        public bool CooldownActive;
        public string CooldownRemainingSeconds;
        public string LastFlowTimestamp;
        public string LastErrorCode;
        public bool RemoteEnabled;
        public bool ConsentGranted;
        public bool OfflineGuardTriggered;
        public string ConfigSourceType;
        public string TriggerEngineType;
        public string AppVersion;
        public string PackageVersion;
        public string SnapshotTimestamp;

        public string ToJson() => JsonUtility.ToJson(this, true);
    }
}
