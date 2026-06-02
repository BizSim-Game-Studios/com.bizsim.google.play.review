using System.Reflection;
using UnityEditor;
using UnityEngine;
using BizSim.Google.Play.Editor.Core;

namespace BizSim.Google.Play.Review.Editor
{
    public sealed class ReviewConfiguration : EditorWindow
    {
        private const string REPO_URL = "https://github.com/BizSim-Game-Studios/com.bizsim.google.play.review";
        private const string DOCS_URL = REPO_URL + "/blob/main/Documentation~/DATA_SAFETY.md";
        private Vector2 _scroll;
        private SerializedObject _settingsSO;
        private int _selectedTab;

        private static readonly string[] TAB_LABELS = new[]
        {
            "Settings", "Trigger Engine", "Remote Config", "Consent Gate", "Firebase", "Telemetry", "Diagnostics", "Links"
        };

        [MenuItem("BizSim/Google Play/Review/Configuration", false, 100)]
        public static void ShowWindow()
        {
            var w = GetWindow<ReviewConfiguration>("BizSim Review");
            w.minSize = new Vector2(460, 480);
            w.Show();
        }

        private void OnEnable()
        {
            var settings = ReviewSettingsAsset.LoadOrCreate();
            _settingsSO = new SerializedObject(settings);
        }

        private void OnGUI()
        {
            if (_settingsSO == null) OnEnable();
            if (_settingsSO == null) return;

            // The SerializedObject is built once (in OnEnable) over the disk-loaded asset.
            // Edits accumulate as PENDING in the SO across frames AND sections. We deliberately
            // do NOT call Update() here (it would re-read the target each frame and discard
            // multi-frame pending edits) nor ApplyModifiedProperties() (it would mutate the live
            // asset every frame, which breaks the Revert button — verified in Unity). Pending
            // edits are flushed to the asset ONLY by Apply; Revert rebuilds the SO over the
            // un-mutated asset to discard them.

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();
            EditorGUILayout.Space(8);

            _selectedTab = GUILayout.Toolbar(_selectedTab, TAB_LABELS);
            EditorGUILayout.Space(8);

            switch (_selectedTab)
            {
                case 0: DrawSettingsSection(); break;
                case 1: DrawTriggerEngineSection(); break;
                case 2: DrawRemoteConfigSection(); break;
                case 3: DrawConsentGateSection(); break;
                case 4: DrawFirebaseSection(); break;
                case 5: DrawTelemetrySection(); break;
                case 6: DrawDiagnosticsSection(); break;
                case 7: DrawLinksSection(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private static string ResolveNativeSdkVersion()
        {
            const BindingFlags F = BindingFlags.Public | BindingFlags.Static;
            var t = typeof(PackageVersion);
            var f = t.GetField("NativeSdkVersion", F)
                 ?? t.GetField("PlayCoreVersion", F);
            return f?.GetRawConstantValue() as string ?? "unknown";
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("BizSim Google Play In-App Review", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Package version: {PackageVersion.Current}");
            EditorGUILayout.LabelField($"Play Core: {ResolveNativeSdkVersion()}");
            EditorGUILayout.LabelField($"Released: {PackageVersion.ReleaseDate}");
        }

        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("Settings (project-wide defaults)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These values are stored in Assets/Resources/BizSim/GooglePlay/ReviewSettings.asset " +
                "and become the controller's defaults at runtime. Edit them here to change the " +
                "project-wide cooldown interval, log level, or mock toggle.",
                MessageType.Info);

            // _settingsSO already Update()'d once at the top of OnGUI — do NOT call again here.
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("LogsEnabled"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("LogLevel"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("UseMockInDevelopmentBuild"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("EnableAnalyticsByDefault"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("MinPromptIntervalDays"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("DefaultTimeoutSeconds"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wave 2 Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("ResetCooldownOnVersionChange"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("ThankYouToastEnabled"));

            EditorGUILayout.Space();
            DrawApplyRevertResetButtons();
        }

        private void DrawTriggerEngineSection()
        {
            EditorGUILayout.LabelField("Trigger Engine Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure the built-in trigger engine thresholds. These values control when " +
                "the review prompt is eligible to fire. The trigger engine evaluates these " +
                "criteria before every RequestReview call.",
                MessageType.Info);

            // _settingsSO already Update()'d once at the top of OnGUI — do NOT call again here.
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("FirstRunGraceSessions"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("FirstRunGraceDays"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("WatchdogTimeoutSeconds"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("OfflineGuardEnabled"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("DryRunMode"));

            EditorGUILayout.Space();
            DrawApplyRevertResetButtons();
        }

        private void DrawRemoteConfigSection()
        {
            EditorGUILayout.LabelField("Remote Config Integration", EditorStyles.boldLabel);

            string configSourceType = Application.isPlaying
                ? GetRuntimeConfigSourceType()
                : "StaticReviewConfigSource (default at edit time)";

            EditorGUILayout.LabelField("Current IReviewConfigSource:", configSourceType);

            bool remoteEnabled = true;
            if (Application.isPlaying)
            {
                var controller = ReviewController.Instance;
                if (controller != null)
                {
                    var snap = controller.GetDiagnosticSnapshot();
                    remoteEnabled = snap.RemoteEnabled;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var prev = GUI.color;
                GUI.color = remoteEnabled ? Color.green : Color.red;
                EditorGUILayout.LabelField(
                    remoteEnabled ? "RemoteEnabled: YES" : "RemoteEnabled: NO (kill switch active)",
                    EditorStyles.boldLabel);
                GUI.color = prev;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Call ReviewController.Instance.SetConfigSource() at runtime to wire a Firebase Remote Config " +
                "or Unity Gaming Services backend. The default StaticReviewConfigSource always returns " +
                "RemoteEnabled=true with no thresholds. See Documentation~/REMOTE-CONFIG-INTEGRATION.md.",
                MessageType.Info);
        }

        private void DrawConsentGateSection()
        {
            EditorGUILayout.LabelField("Consent Gate Integration", EditorStyles.boldLabel);

            string gateType = Application.isPlaying
                ? GetRuntimeConsentGateType()
                : "AlwaysAllowConsentGate (default at edit time)";

            EditorGUILayout.LabelField("Current IConsentGate:", gateType);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Call ReviewController.Instance.SetConsentGate() at runtime to wire your CMP (Consent " +
                "Management Platform) integration. The default AlwaysAllowConsentGate skips consent " +
                "verification entirely. For GDPR/DMA regions, you MUST wire a real consent gate. " +
                "See Documentation~/GDPR-INTEGRATION.md.",
                MessageType.Warning);
        }

        private void DrawDiagnosticsSection()
        {
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to view live diagnostic data from the ReviewController.",
                    MessageType.Info);
                return;
            }

            var controller = ReviewController.Instance;
            if (controller == null)
            {
                EditorGUILayout.HelpBox(
                    "ReviewController.Instance is null. The controller has not been accessed yet.",
                    MessageType.Warning);
                return;
            }

            var snapshot = controller.GetDiagnosticSnapshot();

            EditorGUILayout.LabelField("Session Count:", snapshot.SessionCount.ToString());
            EditorGUILayout.LabelField("Launch Count:", snapshot.LaunchCount.ToString());
            EditorGUILayout.LabelField("Days Since Install:", snapshot.DaysSinceInstall.ToString());
            EditorGUILayout.LabelField("Cooldown Active:", snapshot.CooldownActive.ToString());
            EditorGUILayout.LabelField("Cooldown Remaining (s):", snapshot.CooldownRemainingSeconds);
            EditorGUILayout.LabelField("Last Flow Timestamp:", snapshot.LastFlowTimestamp);
            EditorGUILayout.LabelField("Last Error Code:", snapshot.LastErrorCode);
            EditorGUILayout.LabelField("Remote Enabled:", snapshot.RemoteEnabled.ToString());
            EditorGUILayout.LabelField("Consent Granted:", snapshot.ConsentGranted.ToString());
            EditorGUILayout.LabelField("Offline Guard Triggered:", snapshot.OfflineGuardTriggered.ToString());
            EditorGUILayout.LabelField("Config Source Type:", snapshot.ConfigSourceType);
            EditorGUILayout.LabelField("Trigger Engine Type:", snapshot.TriggerEngineType);
            EditorGUILayout.LabelField("App Version:", snapshot.AppVersion);
            EditorGUILayout.LabelField("Package Version:", snapshot.PackageVersion);

            EditorGUILayout.Space();
            if (GUILayout.Button("Copy JSON to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = snapshot.ToJson();
                Debug.Log(BizSimLogger.Prefix + "Diagnostic snapshot copied to clipboard.");
            }

            if (GUILayout.Button("Refresh"))
            {
                Repaint();
            }
        }

        private void DrawFirebaseSection()
        {
            EditorGUILayout.LabelField("Firebase Analytics Integration", EditorStyles.boldLabel);
            bool packageInstalled = BizSimDefineManager.IsFirebaseAnalyticsInstalled();
            string version = packageInstalled ? BizSimDefineManager.GetFirebaseAnalyticsVersion() : null;

            using (new EditorGUILayout.HorizontalScope())
            {
                var prev = GUI.color;
                GUI.color = packageInstalled ? Color.green : new Color(1f, 0.5f, 0f);
                EditorGUILayout.LabelField(
                    packageInstalled ? $"\u2713 Installed (v{version})" : "\u2717 Not installed",
                    EditorStyles.boldLabel);
                GUI.color = prev;
            }

            bool definePresent = BizSimDefineManager.IsFirebaseDefinePresentAnywhere();
            EditorGUILayout.LabelField($"BIZSIM_FIREBASE define: {(definePresent ? "active" : "inactive")}");

            string msg = BizSimDefineManager.GetFirebaseStatusMessage(out var msgType);
            EditorGUILayout.HelpBox(msg, msgType);

            using (new EditorGUI.DisabledScope(!packageInstalled || definePresent))
                if (GUILayout.Button("Add BIZSIM_FIREBASE to all platforms"))
                    BizSimDefineManager.AddFirebaseDefineAllPlatforms();
            using (new EditorGUI.DisabledScope(!definePresent))
                if (GUILayout.Button("Remove BIZSIM_FIREBASE from all platforms"))
                    BizSimDefineManager.RemoveFirebaseDefineAllPlatforms();
        }

        private void DrawTelemetrySection()
        {
            EditorGUILayout.LabelField("Samples & Telemetry (Wave 2)", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Wave 2 adds IReviewAnalyticsAdapterV2 with 15 event methods. The controller " +
                "dispatches V2 events automatically when the adapter implements the V2 interface.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("V2 Event Names", EditorStyles.boldLabel);

            string[] v2Events = new[]
            {
                "OnReviewRequested(ctx)",
                "OnReviewFlowCompleted(ctx)",
                "OnReviewError(error, ctx)",
                "OnTriggerEvaluated(decision, ctx)",
                "OnPreloadStarted(ctx)",
                "OnPreloadSucceeded(ctx)",
                "OnPreloadFailed(error, ctx)",
                "OnKillSwitchBlocked(ctx)",
                "OnConsentBlocked(ctx)",
                "OnOfflineBlocked(ctx)",
                "OnCooldownBlocked(ctx)",
            };

            foreach (var e in v2Events)
                EditorGUILayout.LabelField("  " + e);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Adapter Wiring Status", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Enter Play Mode to see adapter status.");
                return;
            }

            var controller = ReviewController.Instance;
            if (controller == null)
            {
                EditorGUILayout.LabelField("ReviewController not initialized.");
                return;
            }

            // Use diagnostics to infer adapter wiring (no direct accessor for _analytics)
            EditorGUILayout.LabelField("Preload Cached:", controller.IsPreloadCached.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Available Samples", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  FirebaseAdapter — Samples~/FirebaseAdapter/");
            EditorGUILayout.LabelField("  FeedbackSink — Samples~/FeedbackSink/");
            EditorGUILayout.LabelField("  BasicIntegration — Samples~/BasicIntegration/");
            EditorGUILayout.LabelField("  MockPresets — Samples~/MockPresets/");
        }

        private void DrawLinksSection()
        {
            EditorGUILayout.LabelField("Documentation & Support", EditorStyles.boldLabel);
            if (GUILayout.Button("Open GitHub Repository")) Application.OpenURL(REPO_URL);
            if (GUILayout.Button("Open Data Safety Documentation")) Application.OpenURL(DOCS_URL);
        }

        private void DrawApplyRevertResetButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply"))
                {
                    // Flush pending edits, then write the asset to disk.
                    _settingsSO.ApplyModifiedPropertiesWithoutUndo();
                    ReviewSettingsAsset.Save();
                    BizSimLogger.InvalidateCache();
                    _settingsSO.Update();
                }
                if (GUILayout.Button("Revert"))
                {
                    // Discard unsaved in-memory edits by reloading the asset from disk.
                    // ApplyModifiedProperties() runs every frame (mutating the live asset),
                    // so Update() alone cannot undo edits — rebuild the SerializedObject over
                    // the disk-loaded asset, mirroring the Reset branch. (bridge-pattern §8)
                    _settingsSO = new SerializedObject(ReviewSettingsAsset.LoadOrCreate());
                }
                if (GUILayout.Button("Reset to defaults"))
                {
                    ReviewSettingsAsset.ResetToDefaults();
                    _settingsSO = new SerializedObject(ReviewSettingsAsset.LoadOrCreate());
                    BizSimLogger.InvalidateCache();
                }
            }
        }

        private static string GetRuntimeConfigSourceType()
        {
            var controller = ReviewController.Instance;
            if (controller == null) return "N/A (controller not initialized)";
            var snap = controller.GetDiagnosticSnapshot();
            return snap.ConfigSourceType;
        }

        private static string GetRuntimeConsentGateType()
        {
            var controller = ReviewController.Instance;
            if (controller == null) return "N/A (controller not initialized)";
            var snap = controller.GetDiagnosticSnapshot();
            return snap.ConsentGranted ? "Consented" : "Not consented";
        }
    }
}
