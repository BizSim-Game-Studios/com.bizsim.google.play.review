using System;
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
            if (AssetDatabase.LoadAssetAtPath<ReviewSettings>(ReviewSettings.AssetDatabasePath) == null)
            {
                Debug.LogWarning(BizSimLogger.Prefix +
                    "ReviewSettings.asset not found at " + ReviewSettings.AssetDatabasePath + ". " +
                    "ReviewController will fall back to compile-time defaults at runtime, and the logger " +
                    "will print a one-shot fallback warning on first call. Open " +
                    "BizSim → Google Play → Review → Configuration to create the asset.");
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
