using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class MeterGainEffect : IEffect
    {
        public float MeterGainValue;
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            if (actor.isControllable)
                UIManager.instance.GainMeter(MeterGainValue);
        }
    }

}

