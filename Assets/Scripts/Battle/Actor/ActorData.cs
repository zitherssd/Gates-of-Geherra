using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor.AI;
using Assets.Scripts.Battle.Items;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Actor", menuName = "ScriptableObjects/Actor", order = 1)]
    public class ActorData : ScriptableObject
    {
        public string guid;
        public string Name;
        public List<HpBar> hpBars;
        public float maxBuildup;
        public float maxPosture;
        public float maxStamina;

        public bool Controllable;
        public AiRuleset AIRuleset;

        public Color mainColor;
        public Color secondaryColor;


        public List<BaseItem> startingItems;
        public List<BaseAction> baseActions;
        [HideInInspector]
        public List<BaseAction> actions;
        [HideInInspector]
        public List<BaseItem> items;

        [HideInInspector]
        public float currentBuildup;
        [HideInInspector]
        public float currentPosture;
        [HideInInspector]
        public float currentStamina;


        public float postureRegenRate = 1f;
        public float staminaRegenRate = 1f;


        public int ATK;
        public int DEF;
        public int AGI;
        public int Spirit;

        public event Action OnDeath;
        public event Action OnHpBarLost;


        private void Awake()
        {
            currentBuildup = 0;
            currentPosture = maxPosture;
        }

        public void DealDamage(float damage)
        {
            //Find last bar which is alive
            var lastBar = hpBars.Where<HpBar>(item => item.alive).LastOrDefault();

            //Deal damage
            if (lastBar == null) return;
            lastBar.currentHp -= damage;

            //If no longer alive
            if (lastBar.alive == false)
            {
                OnHpBarLost?.Invoke();
            }

            //If no more alive bars then die
            if(hpBars.Where<HpBar>(item => item.alive).Count() == 0)
            {
                OnDeath?.Invoke();
            }

            //If no more bars are alive invoke onDeath
            if (!(hpBars.Where<HpBar>(item => item.alive).Count() > 0))
            {
                if (lastBar.alive == false)
                {
                    OnHpBarLost?.Invoke();
                }
                OnDeath?.Invoke();
                currentPosture = 0;
            }
        }

        public void DealPostureDamage(float damage)
        {
            currentPosture -= damage;
            currentPosture = Mathf.Clamp(currentPosture, 0, maxPosture);
        }

        public void DealStaminaDamage(float damage)
        {
            currentStamina = Mathf.Clamp(currentStamina - damage, 0, maxStamina);
        }

        public void ChangeBuildup(float value)
        {
            currentBuildup += value;
            Mathf.Clamp(currentBuildup, 0, maxBuildup);
        }

        public bool isDead()
        {
            if (GetCurrentHP() == 0)
                return true;
            else
                return false;
        }

        public float GetCurrentHP()
        {
            //Find last alive bar
            var lastBar = hpBars.Where<HpBar>(item => item.alive).LastOrDefault();

            if (lastBar != null)
                return lastBar.currentHp;
            else
                return 0;
        }

        internal float GetCurrentPosture()
        {
            return currentPosture;
        }

        public void Reset()
        {
            Initialize();
            HealAllBars();
            Refresh();
        }

        public void HealAllBars()
        {
            foreach (var hpbar in hpBars)
            {
                hpbar.currentHp = hpbar.maxHp;
                hpbar.alive = true;
            }
        }

        public void HealAllAliveBars()
        {
            foreach (var hpBar in hpBars)
                if (hpBar.alive) hpBar.currentHp = hpBar.maxHp;
        }

        public void Refresh()
        {
            currentBuildup = 0;
            currentPosture = maxPosture;
            currentStamina = maxStamina;
            foreach (var action in actions)
                action.Refresh();
        }

        public void Initialize()
        {
            actions.Clear();
            foreach (var skill in baseActions)
            {
                var clone = Instantiate(skill);
                actions.Add(clone);
            }
            if(items == null || items.Count == 0)
            {
                items = new List<BaseItem>();
                foreach (var item in startingItems)
                {
                    // Items do NOT need Instantiate unless they hold state
                    // So we add the SO directly
                    items.Add(item);
                }
            }
        }
    }
}



