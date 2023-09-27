using Assets;
using Assets.Scripts.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Actor", menuName = "ScriptableObjects/Actor", order = 1)]
public class Actor : ScriptableObject
{



    public string Name;
    public float baseHP;
    public float baseBuildup;
    public float basePosture;
    public bool Controllable;
    [HideInInspector] public Color mainColor;
    [HideInInspector] public Color secondaryColor;

    public List<BaseSkill> baseSkills;
    public List<BaseReaction> baseReactions;

    [HideInInspector] public List<BaseSkill> skills;
    [HideInInspector] public List<BaseReaction> reactions;

    public float currentHp;
    public float currentBuildup;
    public float currentPosture;

    public int ATK;
    public int DEF;
    public int AGI;

    public event Action<Actor> onDeath;
    public event Action<Actor> onPostureBroken;

    public event Action<float> OnDamageDealt;
    public event Action<float> OnPostureDamage;


    private void Awake()
    {
        currentHp = baseHP;
        currentBuildup = baseBuildup;
        currentPosture = basePosture;
    }

    public void DealDamage(float damage)
    {
        OnDamageDealt?.Invoke(damage);

        currentHp -= damage;
        
        if( currentHp <= 0)
        {
            onDeath?.Invoke(this);
            currentPosture = 0;
            currentHp = 0;
        }
    }

    public bool DealPostureDamage(float postureDamage)
    {
        OnPostureDamage?.Invoke(postureDamage);

        currentPosture -= postureDamage;
        
        if (currentPosture <= 0)
        {
            if (currentHp == 0) { currentPosture = 0; return false; }

            onPostureBroken?.Invoke(this);
            return true;
        }
        else return false;
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
        currentHp = baseHP;
        currentBuildup = baseBuildup;
        currentPosture = basePosture;
        baseSkills.RemoveAll(item => item == null);
        Initialize();
    }

    public void Initialize()
    {
        skills.Clear();
        reactions.Clear();
        foreach (var skill in baseSkills)
        {
            var clone = Instantiate(skill);
            clone.remainingUses = clone.TotalUses;
            skills.Add(clone);
        }
        foreach (var reaction in baseReactions)
        {
            var clone = Instantiate(reaction);
            clone.remainingUses = clone.TotalUses;
            reactions.Add(clone);
        }
    }

}
