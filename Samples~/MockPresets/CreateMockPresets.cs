using UnityEditor;
using UnityEngine;
using BizSim.Google.Play.Review;

namespace BizSim.Google.Play.Review.Samples.MockPresets
{
    public static class CreateMockPresets
    {
        private const string OUTPUT_DIR = "Assets/BizSim/Review/MockPresets";

        [MenuItem("Assets/Create/BizSim/Google Play/Review/Mock Presets", false, 200)]
        public static void CreateAll()
        {
            if (!AssetDatabase.IsValidFolder("Assets/BizSim"))
                AssetDatabase.CreateFolder("Assets", "BizSim");
            if (!AssetDatabase.IsValidFolder("Assets/BizSim/Review"))
                AssetDatabase.CreateFolder("Assets/BizSim", "Review");
            if (!AssetDatabase.IsValidFolder(OUTPUT_DIR))
                AssetDatabase.CreateFolder("Assets/BizSim/Review", "MockPresets");

            Create("MockPreset_Success_Fast",           0.1f, ReviewErrorCode.NoError,           true);
            Create("MockPreset_Success_SlowNetwork",    3.0f, ReviewErrorCode.NoError,           true);
            Create("MockPreset_PlayStoreNotFound",      0.1f, ReviewErrorCode.PlayStoreNotFound, false);
            Create("MockPreset_InvalidRequest",         0.1f, ReviewErrorCode.InvalidRequest,    false);
            Create("MockPreset_InternalError",          0.5f, ReviewErrorCode.InternalError,     false);
            Create("MockPreset_Success_DialogNotShown", 0.2f, ReviewErrorCode.NoError,           false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("BizSim Review",
                "6 mock presets created in " + OUTPUT_DIR, "OK");
        }

        private static void Create(string name, float delay, ReviewErrorCode error, bool shown)
        {
            var asset = ScriptableObject.CreateInstance<ReviewMockConfig>();
            asset.SimulatedDelaySeconds = delay;
            asset.SimulatedError = error;
            asset.SimulateFlowShown = shown;
            AssetDatabase.CreateAsset(asset, $"{OUTPUT_DIR}/{name}.asset");
        }
    }
}
