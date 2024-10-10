using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Reactions
{
    [CreateAssetMenu(fileName = "Dodge", menuName = "ScriptableObjects/Reaction/Dodge")]

    public class Dodge : BaseSkill
    {
        [Range(0, 10)]
        public float force;
        public float time;

        protected override void PerformSpecific(Actor actor, Action onActionComplete)
        {
            CameraManager.instance.SlowTrack = true;
            actor.movement.ResetMomentum();
            actor.movement.SetFriction(0);
            actor.movement.AddForce(force * Direction * StickMult);
            actor.StartCoroutine(actor.WaitForTime(() => actor.movement.SetFriction(), time));

            actor.state.TransitionTo(actor.state.actingState.Set(this, () => { onActionComplete?.Invoke();}));

        }

        public override void OnEnterWindup(Animator animator)
        {
            animator.speed = time;
        }
    }

}
