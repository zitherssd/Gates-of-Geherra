using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Save;
using Assets.Scripts.Game;

public static class TimeLockManager
{

    public static bool IsLocked(string id)
    {
        Cleanup();
        return GameFlowManager.instance.timelocks.Any(t => t.id == id);
    }


    public static TimeLock Get(string id)
    {
        //Cleanup();
        return GameFlowManager.instance.timelocks.FirstOrDefault(t => t.id == id);
    }

    public static void Add(string id, TimeSpan duration)
    {
        var tl = new TimeLock(id, DateTime.UtcNow.Add(duration));
        GameFlowManager.instance.timelocks.Add(tl);

        SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
    }

    public static void Remove(string id)
    {
        GameFlowManager.instance.timelocks.RemoveAll(t => t.id == id);
        SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
    }

    public static void Cleanup()
    {
        bool changed = false;

        for (int i = GameFlowManager.instance.timelocks.Count - 1; i >= 0; i--)
        {
            if (GameFlowManager.instance.timelocks[i].IsDone)
            {
                GameFlowManager.instance.timelocks.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
            SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
    }
}
