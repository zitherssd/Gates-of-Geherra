using UnityEditor;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(SlowdownVisualEffect))]
    public class SlowdownVisualEffectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Setup Instructions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Configure the post-processing effects in the Volume profile\n" +
                "2. Adjust 'Smooth Speed' to control how fast the effect fades in/out\n" +
                "3. The volume weight will smoothly transition based on TimeScale",
                MessageType.Info);
            
            EditorGUILayout.LabelField("Smooth Speed Reference", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "2-3: Slow smooth transition\n" +
                "5-6: Default (noticeable but responsive)\n" +
                "10+: Fast snappy transition",
                MessageType.None);
        }
    }
}
