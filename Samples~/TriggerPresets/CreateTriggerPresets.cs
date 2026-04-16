using UnityEditor;
using UnityEngine;
using BizSim.Google.Play.Review;

namespace BizSim.Google.Play.Review.Samples.TriggerPresets
{
    /// <summary>
    /// Editor menu action that creates three <see cref="ReviewSettings"/> preset assets
    /// tuned for different app categories. Each preset configures the trigger engine's
    /// first-run grace period (sessions and days) to match the engagement curve of the
    /// target category.
    /// <para>
    /// <b>CasualGameTrigger</b> — short grace: 5 sessions / 3 days. Casual games have
    /// high early engagement; prompt while the player is still active.
    /// </para>
    /// <para>
    /// <b>HardcoreGameTrigger</b> — long grace: 10 sessions / 14 days. Hardcore games
    /// need more play time before the user has a well-formed opinion.
    /// </para>
    /// <para>
    /// <b>UtilityAppTrigger</b> — minimal grace: 3 sessions / 7 days. Utility apps have
    /// short sessions; the user knows the value quickly but may use the app infrequently.
    /// </para>
    /// </summary>
    public static class CreateTriggerPresets
    {
        private const string OUTPUT_DIR = "Assets/BizSim/Review/TriggerPresets";

        [MenuItem("BizSim/Google Play/Review/Create Trigger Presets")]
        public static void CreateAll()
        {
            EnsureDirectoryExists("Assets/BizSim");
            EnsureDirectoryExists("Assets/BizSim/Review");
            EnsureDirectoryExists(OUTPUT_DIR);

            CreatePreset("CasualGameTrigger",   graceSessions: 5,  graceDays: 3,  minSessions: 10);
            CreatePreset("HardcoreGameTrigger",  graceSessions: 10, graceDays: 14, minSessions: 25);
            CreatePreset("UtilityAppTrigger",    graceSessions: 3,  graceDays: 7,  minSessions: 5);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("BizSim Review",
                "3 trigger presets created in " + OUTPUT_DIR, "OK");
        }

        private static void CreatePreset(string name, int graceSessions, int graceDays, int minSessions)
        {
            var asset = ScriptableObject.CreateInstance<ReviewSettings>();

            // Trigger engine tuning
            asset.FirstRunGraceSessions = graceSessions;
            asset.FirstRunGraceDays = graceDays;

            // Logging defaults — verbose for presets so consumers can see the
            // trigger engine decision tree during integration testing.
            asset.LogsEnabled = true;
            asset.LogLevel = BizSimLogger.LogLevel.Verbose;

            AssetDatabase.CreateAsset(asset, $"{OUTPUT_DIR}/{name}.asset");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            var folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
