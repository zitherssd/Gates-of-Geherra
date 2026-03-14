using System;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Reactions
{
    [CreateAssetMenu(fileName = "Dodge", menuName = "ScriptableObjects/Action/Dodge")]

    public class Dodge : BaseSkill
    {
        [Range(0, 10)]
        public float force;
        public float recoveryTimeMult;
        public float windupTimeMult;
        public float friction;
        private Actor.Actor owner;

        protected override void PerformSpecific(Actor.Actor actor, Action onActionComplete)
        {
            CameraManager.instance.SlowTrack = true;
            actor.movement.SetFriction(0);
            actor.movement.AddForce(force * Direction * StickMult);
            actor.state.TransitionTo<ActingState>().Set(this);
            owner = actor;
        }

        protected override void Cleanup(ActionEndReason reason)
        {
            base.Cleanup(reason);
            if (reason == ActionEndReason.Completed)
            {
                owner.movement.SetFriction();
                owner.movement.FaceTarget(owner.target.ClosestEnemy);
            }
        }

        public override void OnEnterWindup(Animator animator)
        {
            animator.speed = windupTimeMult;
        }
        public override void OnEnterRecovery(Animator animator)
        {
            animator.speed = recoveryTimeMult;
            owner.movement.SetFriction(friction);
        }
    }

}
