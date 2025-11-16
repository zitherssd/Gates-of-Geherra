using Assets.Scripts.Utility;
using System;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class SlowTrack : IEffect
    {
        public bool active;


        public void Eval(Actor.Actor actor, BaseAction t)
        {
            if (actor.isControllable)
                CameraManager.instance.SlowTrack = active;
        }
    }

}

