using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "Taunt", menuName = "ScriptableObjects/Skills/Taunt")]
    public class Taunt : BaseSkill
    {
        private Actor casterActor;

        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            this.casterActor = casterActor;
            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, onPerformEnd));
        }

        public override void OnHit()
        {
            casterActor.GetComponentInChildren<ParticleSystem>().Play(); //to b ereplace
            casterActor.transform.position = casterActor.transform.position + GetRelativeToCamera(Direction) * 10f;
            casterActor.GetComponentInChildren<ParticleSystem>().Play();
        }
    }
}
