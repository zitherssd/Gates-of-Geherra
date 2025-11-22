using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Battle.Actions;

[CreateAssetMenu(menuName = "Database/ActionDatabase")]
public class ActionDatabase : ScriptableObject
{
    private Dictionary<string, BaseAction> lookup;


    public void Initialize()
    {
        lookup = new Dictionary<string, BaseAction>();

        // Automatically loads ALL BaseAction assets inside Resources/Actions AND SUBFOLDERS
        BaseAction[] actions = Resources.LoadAll<BaseAction>("Actions");

        foreach (var action in actions)
        {
            if (string.IsNullOrEmpty(action.guid))
            {
                action.guid = System.Guid.NewGuid().ToString();
                Debug.LogError($"Action '{action.name}' has no ID! Assign one.");
                continue;
            }

            lookup[action.guid] = action;
        }

        Debug.Log($"ActionDatabase initialized with {lookup.Count} actions.");
    }

    public BaseAction Get(string id)
    {
        if (lookup.TryGetValue(id, out var action))
            return action;

        Debug.LogError($"Action with ID '{id}' not found.");
        return null;
    }
}
