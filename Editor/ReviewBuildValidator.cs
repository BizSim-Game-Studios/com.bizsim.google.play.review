using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BizSim.Google.Play.Review.Editor
{
    internal sealed class ReviewBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android) return;

            if (!HasEdm4U())
            {
                Debug.LogWarning(BizSimLogger.Prefix +
                    "EDM4U not detected at build time even though this package declares " +
                    "'com.google.external-dependency-manager:1.2.187' as a UPM dependency. " +
                    "This usually means the host project is missing the OpenUPM scoped registry entry. " +
                    "Add the registry to Packages/manifest.json (see this package's README.md Installation section) " +
                    "and re-import the package, then run Android Resolver → Force Resolve to pull " +
                    "'com.google.android.play:review:2.0.2'.");
            }

            // Settings asset existence check — CROSS-INVARIANTS §13 risk #5 mitigation.
            // Uses Debug.LogWarning directly because the Settings asset may be missing (LogsEnabled=false).
            var settings = AssetDatabase.LoadAssetAtPath<ReviewSettings>(ReviewSettings.AssetDatabasePath);
            if (settings == null)
            {
                Debug.LogWarning(BizSimLogger.Prefix +
                    "ReviewSettings.asset not found at " + ReviewSettings.AssetDatabasePath + ". " +
                    "ReviewController will fall back to compile-time defaults at runtime, and the logger " +
                    "will print a one-shot fallback warning on first call. Open " +
                    "BizSim → Google Play → Review → Configuration to create the asset.");
            }
            else
            {
                // Enterprise Wave 1: watchdog invariant check
                if (settings.WatchdogTimeoutSeconds < settings.DefaultTimeoutSeconds)
                {
                    Debug.LogWarning(BizSimLogger.Prefix +
                        $"WatchdogTimeoutSeconds ({settings.WatchdogTimeoutSeconds}s) is shorter than " +
                        $"DefaultTimeoutSeconds ({settings.DefaultTimeoutSeconds}s) — the watchdog will " +
                        "usually pre-empt the consumer timeout. Consider raising the watchdog or " +
                        "lowering the default timeout in ReviewSettings.");
                }

                // Enterprise Wave 1: dry-run mode in release build
                if (settings.DryRunMode && !EditorUserBuildSettings.development)
                {
                    Debug.LogWarning(BizSimLogger.Prefix +
                        "DryRunMode is enabled in ReviewSettings but this is a release build. " +
                        "Dry-run is gated on Debug.isDebugBuild and will be ignored at runtime. " +
                        "Disable DryRunMode in BizSim > Google Play > Review > Configuration " +
                        "to suppress this warning.");
                }
            }

            // Wave 3: ProGuard rule validation
            ValidateProguardRules();
        }

        private static void ValidateProguardRules()
        {
            // Unity's standard consumer ProGuard rules path (set via Player Settings).
            var proguardUserPath = Path.Combine(Application.dataPath, "Plugins", "Android", "proguard-user.txt");
            if (!File.Exists(proguardUserPath)) return; // No custom rules — the .androidlib consumer-rules.pro covers it.

            try
            {
                var content = File.ReadAllText(proguardUserPath);
                var warnings = ReviewProguardValidator.Validate(content);
                foreach (var warning in warnings)
                {
                    Debug.LogWarning(BizSimLogger.Prefix + "ProGuard validation: " + warning);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(BizSimLogger.Prefix +
                    $"Could not read proguard-user.txt for validation: {ex.Message}");
            }
        }

        private static bool HasEdm4U()
        {
            // Reflect on GooglePlayServices.PlayServicesResolver to avoid a hard dep.
            var type = Type.GetType("GooglePlayServices.PlayServicesResolver, Google.JarResolver");
            return type != null;
        }
    }
}
