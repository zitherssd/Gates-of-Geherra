using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActorDefinition), true)]
public class ActorDefinitionEditor : Editor
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        ActorDefinition action = target as ActorDefinition;

        if (string.IsNullOrEmpty(action.guid))
        {
            string path = AssetDatabase.GetAssetPath(action);
            action.guid = AssetDatabase.AssetPathToGUID(path);
            EditorUtility.SetDirty(action);
            AssetDatabase.SaveAssets();
        }
    }
}
