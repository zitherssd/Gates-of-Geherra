using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "AnimationAction", menuName = "ScriptableObjects/Actions/Animation")]
    public class GenericSkill : BaseSkill
    {
        private Actor.Actor casterActor;
        private Action _onPerormEnd;

        [SerializeReference, SubclassSelector]
        public List<IEffect> OnStartEffects = new List<IEffect>();
        [SerializeReference, SubclassSelector]
        public List<IEffect> OnHitEffects = new List<IEffect>();
        [SerializeReference, SubclassSelector]
        public List<IEffect> OnEndEffeects = new List<IEffect>();
        public float windupTimeMult = 1f;
        public float recoveryTimeMult = 1f;
        protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
        {
            OnCancel = onPerformEnd;
            this.casterActor = casterActor;
            foreach (var effect in OnStartEffects)
            {
                {
                    effect.Eval(casterActor, this);
                }
            }
            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, onPerformEnd));
        }


        public override void OnHit()
        {
            foreach (var effect in OnHitEffects)
            {
                effect.Eval(casterActor,this);
            }
            if (Tags.Contains(TAG.TECH))
            {
                casterActor.state.actingState.OnEnd();
            }
        }
        public override void OnEnterWindup(Animator animator)
        {
            animator.speed = windupTimeMult;
        }
        public override void OnEnterRecovery(Animator animator)
        {
            animator.speed = recoveryTimeMult;
        }
    }

    public interface IEffect
    {
        void Eval(Actor.Actor actor, BaseAction action);
    }

    [Serializable]
    public class StaminaHealEffect : IEffect
    {
        public float HealAmount;

        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.ActorData.DealStaminaDamage(-HealAmount);
        }
    }

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

    [Serializable]
    public class AddForceDirection : IEffect
    {
        public float Force;


        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.AddForce(t.Direction.normalized * Force);
        }

        enum TARGET { }
    }

    [Serializable]
    public class FaceDirection : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.FaceDirection(t.Direction.normalized);
        }
    }

    [Serializable]
    public class FlashColor : IEffect
    {
        public float intensity;

        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.effects.FlashWhite(intensity);
        }
    }

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
    [Serializable]
    public class Teleport : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.transform.position += t.Direction * t.StickMult;

        }
    }
    [Serializable]
    public class FaceClosestEnemy : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.FaceTarget(actor.target.ClosestEnemy);
        }
    }

    [Serializable]
    public class KillMomentum : IEffect
    {
        public void Eval(Actor.Actor actor, BaseAction t)
        {
            actor.movement.ResetMomentum();
        }
    }

    enum DirectionType { Joystick, Closest_Enemy}

}

