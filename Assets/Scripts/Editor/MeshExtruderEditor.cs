using System.Collections;
using Assets.Scripts.Utility;
using UnityEngine;
using UnityEditor;


namespace Assets.Scripts
{
    [CustomEditor(typeof(MeshExtruder))]
    public class MeshExtruderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Check if we're dealing with multiple objects
            if (targets.Length > 1)
            {
                EditorGUILayout.HelpBox("Multi-object editing not supported.", MessageType.Info);
                return;
            }

            DrawDefaultInspector();

            MeshExtruder meshExtruder = (MeshExtruder)target;
            if (GUILayout.Button("Generate Mesh"))
            {
                meshExtruder.BuildGeometry();
            }
        }
    }
}
