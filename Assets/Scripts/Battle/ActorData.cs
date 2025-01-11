using Assets;
using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.Utility;

[CreateAssetMenu(fileName = "Actor", menuName = "ScriptableObjects/Actor", order = 1)]
public class ActorData : ScriptableObject
{
    public string Name;
    public float maxHp;
    public float maxBuildup;
    public float maxPosture;
    public float maxStamina;

    public bool Controllable;
    [HideInInspector] public Color mainColor;
    [HideInInspector] public Color secondaryColor;

    public List<BaseAction> baseActions;
    public List<BaseReaction> baseReactions;
    public List<BasePassive> basePassives;
    public List<HpBar> baseHpBars;

     public List<BaseAction> actions;
     public List<BaseReaction> reactions;
     public List<HpBar> hpBars;


    public float currentBuildup;
    public float currentPosture;
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

    private void HealAllBars()
    {
        foreach (var hpbar in hpBars)
        {
            hpbar.currentHp = hpbar.maxHp;
            hpbar.alive = true;
        }
    }

    private void HealAllAliveBars()
    {
        foreach (var hpBar in hpBars)
            if (hpBar.alive) hpBar.currentHp = hpBar.maxHp;
    }

    public void Refresh()
    {
        currentBuildup = 0;
        currentPosture = maxPosture;
        currentStamina = maxStamina;
        HealAllAliveBars();
        foreach (var action in actions)
            action.Refresh();
        foreach (var reaction in reactions)
            reaction.Refresh();
    }

    public void Initialize()
    {
        actions.Clear();
        reactions.Clear();
        hpBars.Clear();
        foreach (var skill in baseActions)
        {
            var clone = Instantiate(skill);
            actions.Add(clone);
        }
        foreach (var reaction in baseReactions)
        {
            if(reaction)
            {
                var clone = Instantiate(reaction);
                reactions.Add(clone);
            }

        }
        foreach (var bar in baseHpBars)
        {
            var clone = Instantiate(bar);
            hpBars.Add(clone);
        }
    }

}



