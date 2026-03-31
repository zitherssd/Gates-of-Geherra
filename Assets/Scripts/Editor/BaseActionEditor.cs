using Assets.Scripts.Battle.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseAction), true)]
public class BaseActionEditor : Editor
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        BaseAction action = target as BaseAction;

        if (string.IsNullOrEmpty(action.guid))
        {
            string path = AssetDatabase.GetAssetPath(action);
            action.guid = AssetDatabase.AssetPathToGUID(path);
            EditorUtility.SetDirty(action);
            AssetDatabase.SaveAssets();
        }
    }
}
