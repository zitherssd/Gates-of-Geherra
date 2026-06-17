using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Items;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor
{
    [Serializable]
    public class ActorRuntime
    {
        public ActorDefinition Definition;

        public string Name;
        public List<HpBar> hpBars = new List<HpBar>();

        public float baseMaxBuildup;
        public float baseMaxPosture;
        public float baseMaxStamina;

        public float postureRegenRate = 1f;
        public float staminaRegenRate = 1f;

        public int Strength;
        public int Agility;
        public int Mind;
        public int Spirit;

        public List<BaseAction> actions = new List<BaseAction>();
        public List<BaseItem> items = new List<BaseItem>();

        public float currentBuildup;
        public float currentPosture;
        public float currentStamina;

        public event Action OnDeath;

        public float maxBuildup => baseMaxBuildup + Mind;
        public float maxPosture => baseMaxPosture + Strength;
        public float maxStamina => baseMaxStamina + Agility * 2;

        public ActorRuntime(ActorDefinition definition)
        {
            Definition = definition;
            ResetToDefinition();
        }

        public void ResetToDefinition()
        {
            if (Definition == null)
            {
                return;
            }

            Name = Definition.Name;
            baseMaxBuildup = Definition.baseMaxBuildup;
            baseMaxPosture = Definition.baseMaxPosture;
            baseMaxStamina = Definition.baseMaxStamina;
            postureRegenRate = Definition.postureRegenRate;
            staminaRegenRate = Definition.staminaRegenRate;
            Strength = Definition.Strength;
            Agility = Definition.Agility;
            Mind = Definition.Mind;
            Spirit = Definition.Spirit;

            if (hpBars == null)
            {
                hpBars = new List<HpBar>();
            }
            hpBars.Clear();
            if (Definition.hpBars != null)
            {
                foreach (var hpBar in Definition.hpBars)
                {
                    hpBars.Add(new HpBar
                    {
                        maxHp = hpBar.maxHp,
                        currentHp = hpBar.maxHp,
                        alive = true
                    });
                }
            }

            if (actions == null)
            {
                actions = new List<BaseAction>();
            }
            actions.Clear();
            if (Definition.baseActions != null)
            {
                foreach (var skill in Definition.baseActions)
                {
                    actions.Add(UnityEngine.Object.Instantiate(skill));
                }
            }

            if (items == null)
            {
                items = new List<BaseItem>();
            }
            items.Clear();
            if (Definition.startingItems != null)
            {
                foreach (var item in Definition.startingItems)
                {
                    items.Add(item);
                }
            }

            Refresh();
        }

        public void Refresh()
        {
            currentBuildup = 0;
            currentPosture = maxPosture;
            currentStamina = maxStamina;

            if (actions == null)
            {
                return;
            }

            foreach (var action in actions)
            {
                action.Refresh();
            }
        }

        public void DealDamage(float damage)
        {
            var lastBar = hpBars.Where(item => item.alive).LastOrDefault();
            if (lastBar == null)
            {
                return;
            }

            lastBar.currentHp -= damage;

            if (hpBars.Where(item => item.alive).Count() == 0)
            {
                OnDeath?.Invoke();
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
            return GetCurrentHP() == 0;
        }

        public float GetCurrentHP()
        {
            var lastBar = hpBars.Where(item => item.alive).LastOrDefault();
            if (lastBar != null)
            {
                return lastBar.currentHp;
            }

            return 0;
        }

        public void HealAllBars()
        {
            foreach (var hpBar in hpBars)
            {
                hpBar.currentHp = hpBar.maxHp;
                hpBar.alive = true;
            }
        }

        public void HealAllAliveBars()
        {
            foreach (var hpBar in hpBars)
            {
                if (hpBar.alive)
                {
                    hpBar.currentHp = hpBar.maxHp;
                }
            }
        }
    }
}
