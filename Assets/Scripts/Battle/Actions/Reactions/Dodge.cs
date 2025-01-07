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
        private Actor owner;

        protected override void PerformSpecific(Actor actor, Action onActionComplete)
        {
            CameraManager.instance.SlowTrack = true;
            actor.movement.SetFriction(0);
            actor.movement.AddForce(force * Direction * StickMult);
            actor.state.TransitionTo(actor.state.actingState.Set(this, () => { 
                actor.movement.SetFriction();
                actor.movement.FaceTarget(actor.target.target);
                onActionComplete?.Invoke(); 
                UIManager.instance.GainMeter(SlowdownMeterGain);  
            }));
            owner = actor;
        }

        public override void OnEnterWindup(Animator animator)
        {
            animator.speed = time;
        }
        public override void OnEnterRecovery(Animator animator)
        {
            owner.movement.SetFriction(1.5f);
        }
    }

}
