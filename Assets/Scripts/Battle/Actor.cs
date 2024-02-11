using Assets;
using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Actor", menuName = "ScriptableObjects/Actor", order = 1)]
public class Actor : ScriptableObject
{



    public string Name;
    public float maxHp;
    public float maxBuildup;
    public float maxPosture;
    public bool Controllable;
    [HideInInspector] public Color mainColor;
    [HideInInspector] public Color secondaryColor;

    public List<BaseAction> baseActions;
    public List<BaseReaction> baseReactions;

    [HideInInspector] public List<Assets.BaseAction> actions;
    [HideInInspector] public List<BaseReaction> reactions;

    public float currentHp;
    public float currentBuildup;
    public float currentPosture;

    public int ATK;
    public int DEF;
    public int AGI;

    public event Action<Actor> onDeath;


    private void Awake()
    {
        currentHp = maxHp;
        currentBuildup = 0;
        currentPosture = maxPosture;
    }

    public void DealDamage(float damage)
    {
        currentHp -= damage;
        
        if( currentHp <= 0)
        {
            onDeath?.Invoke(this);
            currentPosture = 0;
            currentHp = 0;
        }
    }

    public void DealPostureDamage(float damage)
    {
        currentPosture -= damage;
        Mathf.Clamp(currentPosture, 0, maxPosture);
    }

    public void ChangeBuildup(float value)
    {
        currentBuildup += value;
        Mathf.Clamp(currentBuildup, 0, maxBuildup);
    }

    public float GetCurrentHP()
    {
        return currentHp;
    }

    internal float GetCurrentPosture()
    {
        return currentPosture;
    }

    public bool isDead()
    {
        if (currentHp <= 0) return true;
        else return false;
    }

    public void Reset()
    {
        currentHp = maxHp;
        currentBuildup = 0;
        currentPosture = maxPosture;
        baseActions.RemoveAll(item => item == null);
        Initialize();
    }

    public void Refresh()
    {
        currentBuildup = 0;
        currentPosture = maxPosture;
    }

    public void Initialize()
    {
        actions.Clear();
        reactions.Clear();
        foreach (var skill in baseActions)
        {
            var clone = Instantiate(skill);
            clone.remainingUses = clone.TotalUses;
            actions.Add(clone);
        }
        foreach (var reaction in baseReactions)
        {
            var clone = Instantiate(reaction);
            clone.remainingUses = clone.TotalUses;
            reactions.Add(clone);
        }
    }

}
