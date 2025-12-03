using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Items;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Assets.Scripts.Save
{
    [Serializable]
    public class SaveData
    {
        public int currentFloor;
        public int trainingsDone;
        public int trainingsDoneThisFloor;
        public ActorSave player;
        public List<TimeLock> timelocks = new List<TimeLock>();

        public void LoadActor(Actor actorToOverwrite)
        {
            ActorSave.LoadActorFromSave(player, actorToOverwrite);
        }
    }


    [System.Serializable]
    public class ActorSave
    {
        public string actorId;
        public string Name;
        public List<HpBar> hpBars;
        public float maxBuildup;
        public float maxPosture;
        public float maxStamina;
        public List<string> actionsIds;
        public List<BaseItem> items;

        public float currentStamina;
        public float currentPosture;
        public float currentBuildup;

        public float postureRegenRate = 1f;
        public float staminaRegenRate = 1f;
        public int STR;
        public int CON;
        public int AGI;
        public int Spirit;

        public static ActorSave CreateSaveFromActor(Actor actor)
        {
            var save = new ActorSave
            {
                actionsIds = new List<string>(),
                hpBars = new List<HpBar>()
            };


            ActorData ad = actor.ActorData;

            save.actorId = ad.guid;
            save.Name = ad.Name;
            save.items = actor.inventory.Items;

            save.currentStamina = ad.currentStamina;
            save.currentBuildup = ad.currentBuildup;
            save.currentPosture = ad.currentPosture;
            save.maxStamina = ad.maxStamina;
            save.maxPosture = ad.maxPosture;
            save.maxBuildup = ad.maxBuildup;

            save.STR = ad.Strength;
            save.CON = ad.Agility;
            save.AGI = ad.Mind;
            save.Spirit = ad.Spirit;

            // Copy HP Bars
            save.hpBars = new List<HpBar>();
            foreach (var h in ad.hpBars)
            {
                save.hpBars.Add(new HpBar
                {
                    maxHp = h.maxHp,
                    currentHp = h.currentHp,
                    alive = h.alive
                });
            }

            // Save action GUIDs
            foreach (var a in ad.actions)
            {
                save.actionsIds.Add(a.guid);
            }

            return save;
        }
        public static void LoadActorFromSave(ActorSave save, Actor actorToOverwrite)
        {
            var actionDb = SaveManager.instance.actionDatabase;
            // You can rewrite this to your own spawning system
            Actor actor = actorToOverwrite;

            ActorData ad = actor.ActorData;

            ad.Name = save.Name;

            ad.items = save.items;

            ad.currentStamina = save.currentStamina;
            ad.currentBuildup = save.currentBuildup;
            ad.currentPosture = save.currentPosture;
            ad.maxStamina = save.maxStamina;
            ad.maxBuildup = save.maxBuildup;
            ad.maxPosture = save.maxPosture;


            ad.Strength = save.STR;
            ad.Agility = save.CON;
            ad.Mind = save.AGI;
            ad.Spirit = save.Spirit;

            // Restore HP bars
            ad.hpBars.Clear();
            foreach (var h in save.hpBars)
            {
                ad.hpBars.Add(new HpBar
                {
                    maxHp = h.maxHp,
                    currentHp = h.currentHp,
                    alive = h.alive
                });
            }

            // Restore actions
            ad.actions.Clear();
            foreach (var guid in save.actionsIds)
            {
                BaseAction template = actionDb.Get(guid);
                BaseAction clone = UnityEngine.Object.Instantiate(template);
                ad.actions.Add(clone);
            }
        }

    }


}
