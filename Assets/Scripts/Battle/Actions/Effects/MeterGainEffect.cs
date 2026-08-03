using System;
using Assets.Scripts.Utility;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class MeterGainEffect : IEffect
    {
        public float MeterGainValue;
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            // Slowdown handled by state-based system in IdleState
        }
    }

}

