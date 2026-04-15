using UnityEngine;

namespace BizSim.Google.Play.Review
{
    public static class BizSimLogger
    {
        // Per-package PREFIX — never changes, never read from asset.
        // Required convention: "[BizSim.Review] " with trailing space. Enforced by BizSimLoggerPrefixTest.
        public const string Prefix = "[BizSim.Review] ";

        public enum LogLevel { Verbose = 0, Info = 1, Warning = 2, Error = 3, Silent = 4 }

        private static ReviewSettings _cachedSettings;
        private static bool _loggedFallbackWarning;

        private static ReviewSettings Settings
        {
            get
            {
                if (_cachedSettings != null) return _cachedSettings;
                _cachedSettings = Resources.Load<ReviewSettings>(ReviewSettings.ResourcesLoadKey);
                if (_cachedSettings != null) return _cachedSettings;

                // Asset missing. Warn once per session, then fall back to compile-time defaults.
                if (!_loggedFallbackWarning)
                {
                    Debug.LogWarning(Prefix +
                        "Settings asset not found at Resources/BizSim/GooglePlay/ReviewSettings.asset — " +
                        "falling back to compile-time defaults. Open BizSim → Google Play → Review → " +
                        "Configuration to create the asset.");
                    _loggedFallbackWarning = true;
                }
                _cachedSettings = ScriptableObject.CreateInstance<ReviewSettings>();
                return _cachedSettings;
            }
        }

        public static void Verbose(string msg) { if (Should(LogLevel.Verbose)) Debug.Log(Prefix + msg); }
        public static void Info   (string msg) { if (Should(LogLevel.Info))    Debug.Log(Prefix + msg); }
        public static void Warning(string msg) { if (Should(LogLevel.Warning)) Debug.LogWarning(Prefix + msg); }
        public static void Error  (string msg) { if (Should(LogLevel.Error))   Debug.LogError(Prefix + msg); }

        // Master-switch (LogsEnabled) is checked FIRST — it overrides LogLevel.
        // When LogsEnabled == false, every call is a no-op regardless of severity.
        private static bool Should(LogLevel level)
        {
            var s = Settings;
            if (!s.LogsEnabled) return false;
            return (int)level >= (int)s.LogLevel;
        }

#if UNITY_EDITOR
        public static void InvalidateCache()
        {
            _cachedSettings = null;
            _loggedFallbackWarning = false;
        }
#endif
    }
}
