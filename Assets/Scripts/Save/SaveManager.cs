using Assets.Scripts.Crawler;
using Assets.Scripts.Game;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Save
{
    public class SaveManager : MonoBehaviour
    {
        public ActionDatabase actionDatabase;
        public int currentSaveSlot;
        public static SaveManager instance;
        public string newGamePlayerName;
        void Awake()
        {
            DontDestroyOnLoad(this);
            instance = this;
        }

        public void Start()
        {
            actionDatabase.Initialize();
        }

        private string GetSlotPath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
        }

        public void SaveToSlot(int slot)
        {
            var actor = GameFlowManager.instance.playerActor;
            var saveData = new SaveData
            {
                currentFloor = FloorManager.instance.currentFloor,
                timelocks = GameFlowManager.instance.timelocks ?? new List<TimeLock>(),
                player = ActorSaveData.FromActor(actor),
                trainingsDone = GameFlowManager.instance.trainingsDone,
                trainingsDoneThisFloor = GameFlowManager.instance.trainingsDoneThisFloor

            };

            // Capture the current UI loadout
            if (UIManager.instance != null)
            {
                saveData.player.actions = UIManager.instance.GetCurrentLoadout();
            }

            SaveGame(slot, saveData);
        }

        public SaveData LoadFromSlot(int slot)
        {
            var save = LoadGame(slot);
            FloorManager.instance.currentFloor = save.currentFloor;
            GameFlowManager.instance.trainingsDone = save.trainingsDone;
            GameFlowManager.instance.trainingsDoneThisFloor = save.trainingsDoneThisFloor;
            GameFlowManager.instance.timelocks = save.timelocks ?? new List<TimeLock>();

            return save;

        }

        public void SaveGame(int slot, SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);

#if UNITY_WEBGL && !UNITY_EDITOR
    PlayerPrefs.SetString("save_slot_" + slot, json);
    PlayerPrefs.Save();
    Debug.Log("Saved WebGL slot " + slot);
#else
            File.WriteAllText(GetSlotPath(slot), json);
            Debug.Log($"Saved slot {slot} to {GetSlotPath(slot)}");
#endif
        }

        public void DeleteSave(int slot)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
    PlayerPrefs.DeleteKey("save_slot_" + slot);
#else
            string path = GetSlotPath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
#endif
        }

        public SaveData LoadGame(int slot)
        {
            currentSaveSlot = slot;
            string json;

#if UNITY_WEBGL && !UNITY_EDITOR
    if (!PlayerPrefs.HasKey("save_slot_" + slot))
    {
        Debug.LogWarning("WebGL save slot " + slot + " does not exist!");
        return null;
    }
    json = PlayerPrefs.GetString("save_slot_" + slot);
#else
            string path = GetSlotPath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Save slot {slot} does not exist!");
                return null;
            }
            json = File.ReadAllText(path);
#endif

            return JsonUtility.FromJson<SaveData>(json);
        }

        public bool SlotExists(int slot)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
    return PlayerPrefs.HasKey("save_slot_" + slot);
#else
            return File.Exists(GetSlotPath(slot));
#endif
        }
    }
}
