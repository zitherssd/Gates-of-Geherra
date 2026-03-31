using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using System;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class ClearHitbox : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction action)
        {
            actor.effects.ClearHitbox();
        }
    }

}
