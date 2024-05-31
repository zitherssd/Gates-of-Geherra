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

    [HideInInspector] public List<Assets.BaseAction> actions;
    [HideInInspector] public List<BaseReaction> reactions;
    [HideInInspector] public List<HpBar> hpBars;


    public float currentHp;
    public float currentBuildup;
    public float currentPosture;
    public float currentStamina;


    public int ATK;
    public int DEF;
    public int AGI;
    public int Spirit;

    public event Action<ActorData> onDeath;
    public event Action onHpBarLost;


    private void Awake()
    {
        currentHp = maxHp;
        currentBuildup = 0;
        currentPosture = maxPosture;
    }

    public void DealDamage(float damage)
    {
        //Find last bar which is alive
        var lastBar = hpBars.Where<HpBar>(item => item.alive).Last();

        //Deal damage
        if (lastBar == null) return;
        lastBar.currentHp -= damage;


        //If no more bars are alive invoke onDeath
        if (!(hpBars.Where<HpBar>(item => item.alive).Count() > 0))
        {
            if (lastBar.alive == false)
            {
                onHpBarLost?.Invoke();
            }
            onDeath?.Invoke(this);
            currentPosture = 0;
            currentHp = 0;
        }
    }

    public void DealPostureDamage(float damage)
    {
        currentPosture -= damage;
        currentPosture = Mathf.Clamp(currentPosture, 0, maxPosture);
    }

    public void DealStaminaDamage(float damage)
    {
        currentStamina -= damage;
        Mathf.Clamp(currentStamina, 0, maxStamina);
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
        HealAllBars();
        Initialize();
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
            var clone = Instantiate(reaction);
            reactions.Add(clone);
        }
        foreach (var passive in basePassives)
        {
            //passive.Perform();
        }
        foreach (var bar in baseHpBars)
        {
            var clone = Instantiate(bar);
            hpBars.Add(bar);
        }
    }

}



