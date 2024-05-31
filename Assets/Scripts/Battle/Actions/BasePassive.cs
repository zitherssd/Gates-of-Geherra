using Assets;
using Assets.Scripts.Battle;
using System;

public class BasePassive : BaseAction
{

    public Trigger triggerType;
    public float chanceToTrigger;
    public Effect effect;
    public bool IsConsumed;
    private Actor _casterActor;

    protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
    {
        //update internal reference
        _casterActor = casterActor;

        //subscribe
        casterActor.ActorData.onHpBarLost += PerformEffect;
    }

    private void PerformEffect()
    {

        if (IsConsumed)
            _casterActor.ActorData.onHpBarLost -= PerformEffect;
    }

    private void OnDestroy()
    {
        _casterActor.ActorData.onHpBarLost -= PerformEffect;
    }

    public enum Trigger
    {
        OnCorpusLoss, OnLastCorpus
    }
}

public class Effect
{
    public bool Rejuvenate;
    public float HpHeal;
    public float StaminaHeal;
    public float BuildupHeal;
    public int strGain;
    public int conGain;
    public int spdGain;
    public int sptGain;
}