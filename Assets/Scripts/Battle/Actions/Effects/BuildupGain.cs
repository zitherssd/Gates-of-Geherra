using System;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class BuildupGain : IEffect
    {
        public float BuildupAmount;

        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.Runtime.ChangeBuildup(BuildupAmount);
        }
    }

}

