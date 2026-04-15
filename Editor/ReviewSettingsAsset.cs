using System.IO;
using UnityEditor;
using UnityEngine;

namespace BizSim.Google.Play.Review.Editor
{
    internal static class ReviewSettingsAsset
    {
        private const string FOLDER = "Assets/Resources/BizSim/GooglePlay";
        private const string PATH   = FOLDER + "/ReviewSettings.asset";

        public static ReviewSettings LoadOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ReviewSettings>(PATH);
            if (existing != null) return existing;

            EnsureFolder(FOLDER);
            var inst = ScriptableObject.CreateInstance<ReviewSettings>();
            AssetDatabase.CreateAsset(inst, PATH);
            AssetDatabase.SaveAssets();
            return inst;
        }

        public static void Save()
        {
            var asset = AssetDatabase.LoadAssetAtPath<ReviewSettings>(PATH);
            if (asset == null) return;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        public static void ResetToDefaults()
        {
            var asset = LoadOrCreate();
            var fresh = ScriptableObject.CreateInstance<ReviewSettings>();
            EditorUtility.CopySerialized(fresh, asset);
            Object.DestroyImmediate(fresh);
            Save();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
