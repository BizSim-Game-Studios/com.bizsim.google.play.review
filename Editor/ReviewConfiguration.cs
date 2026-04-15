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

        [MenuItem("BizSim/Google Play/Review/Configuration", false, 100)]
        public static void ShowWindow()
        {
            var w = GetWindow<ReviewConfiguration>("BizSim Review");
            w.minSize = new Vector2(460, 380);
            w.Show();
        }

        private void OnEnable()
        {
            var settings = ReviewSettingsAsset.LoadOrCreate();
            _settingsSO = new SerializedObject(settings);
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();
            EditorGUILayout.Space(8);
            DrawSettingsSection();
            EditorGUILayout.Space(8);
            DrawFirebaseSection();
            EditorGUILayout.Space(8);
            DrawLinksSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("BizSim Google Play In-App Review", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Package version: {PackageVersion.Current}");
            EditorGUILayout.LabelField($"Play Core: {PackageVersion.PlayCoreVersion}");
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

            if (_settingsSO == null) OnEnable();
            _settingsSO.Update();

            EditorGUILayout.PropertyField(_settingsSO.FindProperty("LogsEnabled"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("LogLevel"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("UseMockInDevelopmentBuild"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("EnableAnalyticsByDefault"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("MinPromptIntervalDays"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("DefaultTimeoutSeconds"));

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply"))
                {
                    _settingsSO.ApplyModifiedProperties();
                    ReviewSettingsAsset.Save();
                    BizSimLogger.InvalidateCache();
                }
                if (GUILayout.Button("Revert"))
                {
                    _settingsSO.Update();
                }
                if (GUILayout.Button("Reset to defaults"))
                {
                    ReviewSettingsAsset.ResetToDefaults();
                    _settingsSO = new SerializedObject(ReviewSettingsAsset.LoadOrCreate());
                    BizSimLogger.InvalidateCache();
                }
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

        private void DrawLinksSection()
        {
            EditorGUILayout.LabelField("Documentation & Support", EditorStyles.boldLabel);
            if (GUILayout.Button("Open GitHub Repository")) Application.OpenURL(REPO_URL);
            if (GUILayout.Button("Open Data Safety Documentation")) Application.OpenURL(DOCS_URL);
        }
    }
}
