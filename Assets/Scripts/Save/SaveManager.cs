using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Crawler;
using Assets.Scripts.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Save
{
    public class SaveManager : MonoBehaviour
    {
        public ActionDatabase actionDatabase;
        public int currentSaveSlot;
        public static SaveManager instance;
        void Awake()
        { DontDestroyOnLoad(this);
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
                //silver = BattleManager.instance.Silver,
                player = ActorSave.CreateSaveFromActor(actor)
            };
            SaveGame(slot, saveData);
        }

        public SaveData LoadFromSlot(int slot)
        {
            var save = LoadGame(slot);
            //ActorSave.LoadActorFromSave(save.player, actionDatabase);
            //FloorManager.instance.currentFloor = save.currentFloor;
            return save;

        }

        public void SaveGame(int slot, SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            Debug.Log($"Saved slot {slot} to {GetSlotPath(slot)}");
            File.WriteAllText(GetSlotPath(slot), json);
        }

        public void DeleteSave(int slot)
        {
            string path = GetSlotPath(slot);

            if (File.Exists(path))
            {
                File.Delete(GetSlotPath(slot));
            }
        }

        public SaveData LoadGame(int slot)
        {
            currentSaveSlot = slot;
            string path = GetSlotPath(slot);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Save slot {slot} does not exist!");
                return null;
            }

            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"Loaded slot {slot}.");
            return data;
        }

        public bool SlotExists(int slot)
        {
            return File.Exists(GetSlotPath(slot));
        }
    }
}
