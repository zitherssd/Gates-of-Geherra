using Assets;
using Assets.Scripts.Battle;
using System;

public class BasePassive : BaseAction
{
    public Trigger triggerType;
    public float chanceToTrigger;
    public Effect effect;
    public bool IsConsumed;

    //public void Apply();

    //public void Remove();
    

    //protected void PerformSpecific(Actor casterActor, Action onPerformEnd)
    //{
    //    //Should never run
    //    PerformEffect();
    //    //Remove();
    //    onPerformEnd();
    //}

    private void PerformEffect()
    {
        //if (IsConsumed)
            //_casterActor.ActorData.OnHpBarLost -= PerformEffect;
    }

    private void OnDestroy()
    {
        //_casterActor.ActorData.OnHpBarLost -= PerformEffect;
    }

    public enum Trigger
    {
        OnCorpusLoss, OnLastCorpus, OnRecievingTheSkill
    }
}

public class Effect
{
    public bool HealCorpusLoss;
    public float HpHeal;
    public float StaminaHeal;
    public float BuildupHeal;
    public int strGain;
    public int conGain;
    public int spdGain;
    public int sptGain;
}