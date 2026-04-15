using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BizSim.Google.Play.Review;
using BizSim.Google.Play.Review.Editor;

namespace BizSim.Google.Play.Review.EditorTests
{
    public class ReviewSettingsAssetTests
    {
        private const string PATH = "Assets/Resources/BizSim/GooglePlay/ReviewSettings.asset";

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.LoadAssetAtPath<ReviewSettings>(PATH) != null)
                AssetDatabase.DeleteAsset(PATH);
        }

        [Test]
        public void LoadOrCreate_AutoCreatesAssetAndFolders()
        {
            // Ensure clean state
            if (AssetDatabase.LoadAssetAtPath<ReviewSettings>(PATH) != null)
                AssetDatabase.DeleteAsset(PATH);

            var asset = ReviewSettingsAsset.LoadOrCreate();
            Assert.NotNull(asset);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<ReviewSettings>(PATH));
            Assert.IsTrue(AssetDatabase.IsValidFolder("Assets/Resources/BizSim/GooglePlay"));
        }

        [Test]
        public void Save_RoundTripsLogLevel()
        {
            var asset = ReviewSettingsAsset.LoadOrCreate();
            asset.LogLevel = BizSimLogger.LogLevel.Error;
            ReviewSettingsAsset.Save();

            AssetDatabase.Refresh();
            var reloaded = AssetDatabase.LoadAssetAtPath<ReviewSettings>(PATH);
            Assert.AreEqual(BizSimLogger.LogLevel.Error, reloaded.LogLevel);
        }

        [Test]
        public void ResetToDefaults_RestoresOriginalValues()
        {
            var asset = ReviewSettingsAsset.LoadOrCreate();
            asset.LogsEnabled = false;
            asset.LogLevel = BizSimLogger.LogLevel.Error;
            asset.UseMockInDevelopmentBuild = true;
            asset.EnableAnalyticsByDefault = true;
            asset.MinPromptIntervalDays = 1;
            asset.DefaultTimeoutSeconds = 120f;
            ReviewSettingsAsset.Save();

            ReviewSettingsAsset.ResetToDefaults();

            var reloaded = AssetDatabase.LoadAssetAtPath<ReviewSettings>(PATH);
            var fresh = ScriptableObject.CreateInstance<ReviewSettings>();
            Assert.AreEqual(fresh.LogsEnabled, reloaded.LogsEnabled);
            Assert.AreEqual(fresh.LogLevel, reloaded.LogLevel);
            Assert.AreEqual(fresh.UseMockInDevelopmentBuild, reloaded.UseMockInDevelopmentBuild);
            Assert.AreEqual(fresh.EnableAnalyticsByDefault, reloaded.EnableAnalyticsByDefault);
            Assert.AreEqual(fresh.MinPromptIntervalDays, reloaded.MinPromptIntervalDays);
            Assert.AreEqual(fresh.DefaultTimeoutSeconds, reloaded.DefaultTimeoutSeconds);
            Object.DestroyImmediate(fresh);
        }
    }
}
