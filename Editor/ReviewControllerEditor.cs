using UnityEditor;
using UnityEngine;

namespace BizSim.Google.Play.Review.Editor
{
    [CustomEditor(typeof(ReviewController))]
    internal sealed class ReviewControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("QA / Testing", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Clear Local Cooldown removes the PlayerPrefs entry so a fresh review can be " +
                "requested immediately. Intended for QA builds only — the cooldown protects " +
                "consumers from Google's quota-invisible throttling.",
                MessageType.Info);

            if (!Application.isPlaying)
            {
                using (new EditorGUI.DisabledScope(true))
                    GUILayout.Button("Clear Local Cooldown (play mode only)");
                return;
            }

            if (GUILayout.Button("Clear Local Cooldown"))
            {
                ReviewController.Instance?.ClearLocalCooldownForTesting();
            }
        }
    }
}
