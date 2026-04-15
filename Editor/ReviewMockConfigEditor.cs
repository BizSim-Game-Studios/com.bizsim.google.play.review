using UnityEditor;
using UnityEngine;

namespace BizSim.Google.Play.Review.Editor
{
    [CustomEditor(typeof(ReviewMockConfig))]
    internal sealed class ReviewMockConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _delay;
        private SerializedProperty _error;
        private SerializedProperty _shown;

        private void OnEnable()
        {
            _delay = serializedObject.FindProperty("SimulatedDelaySeconds");
            _error = serializedObject.FindProperty("SimulatedError");
            _shown = serializedObject.FindProperty("SimulateFlowShown");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Mock scenario", EditorStyles.boldLabel);
            _delay.floatValue = EditorGUILayout.Slider("Simulated Delay (s)", _delay.floatValue, 0f, 10f);
            EditorGUILayout.PropertyField(_error);
            EditorGUILayout.PropertyField(_shown);
            EditorGUILayout.HelpBox(
                "SimulateFlowShown is logging-only. The Google Play In-App Review API is " +
                "quota-invisible — the package never exposes 'was the dialog shown?' to callers.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
