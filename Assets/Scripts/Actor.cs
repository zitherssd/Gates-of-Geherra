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
    public List<BaseSkill> skills;
    public List<BaseReaction> reactions;

    public float currentHp;
    public float currentBuildup;
    public float currentPosture;

    public int ATK;
    public int DEF;
    public int AGI;



    public int INT;

    private void Awake()
    {
        currentHp = baseHP;
        currentBuildup = baseBuildup;
        currentPosture = basePosture;
    }

    public void DealDamage(float damage)
    {
        currentHp -= damage;
        if( currentHp <= 0)
        {
            currentHp = 0;

        }
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
        foreach (var skill in skills)
        {
            skill.remainingUses = skill.TotalUses;
        }
        foreach (var reaction in reactions)
        {
            reaction.remainingUses = reaction.TotalUses;
        }
    }

}
