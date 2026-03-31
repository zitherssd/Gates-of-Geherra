using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActorData), true)]
public class ActorDataEditor : Editor
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        ActorData action = target as ActorData;

        if (string.IsNullOrEmpty(action.guid))
        {
            string path = AssetDatabase.GetAssetPath(action);
            action.guid = AssetDatabase.AssetPathToGUID(path);
            EditorUtility.SetDirty(action);
            AssetDatabase.SaveAssets();
        }
    }
}
