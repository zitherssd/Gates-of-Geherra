using Assets.Scripts.Battle.Actions.Actions.Effects;
using Assets.Scripts.Battle.Actor.States;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "AnimationAction", menuName = "ScriptableObjects/Action/Generic")]
    public class GenericSkill : BaseSkill
    {
        [SerializeReference, SubclassSelector]
        public List<IEffect> OnStartEffects = new List<IEffect>();
        [SerializeReference, SubclassSelector]
        public List<IEffect> OnHitEffects = new List<IEffect>();
        [SerializeReference, SubclassSelector]
        public List<IEndableEffect> OnEndEffects = new List<IEndableEffect>();
        [SerializeReference, SubclassSelector]
        public List<IUpdateableEffect> OnUpdateEffects = new List<IUpdateableEffect>();

        private List<IEndableEffect> runtimeEndEffects;
        private List<IUpdateableEffect> runtimeUpdateEffects;

        public float windupTimeMult = 1f;
        public float recoveryTimeMult = 1f;
        protected override void PerformSpecific(Actor.Actor casterActor, Action onPerformEnd)
        {
            runtimeEndEffects = new List<IEndableEffect>(OnEndEffects);
            runtimeUpdateEffects = new List<IUpdateableEffect>(OnUpdateEffects);

            foreach (var effect in OnStartEffects)
            {
                {
                    if (effect is IEndableEffect endable) runtimeEndEffects.Add(endable);
                    effect.Eval(_caster, this);
                }
            }
            casterActor.state.TransitionTo<ActingState>().Set(this);
        }

        protected override void Cleanup(ActionEndReason reason)
        {
            base.Cleanup(reason);
            foreach (var effect in runtimeEndEffects)
            {
                effect.End(_caster, this);
            }
        }

        public override void OnHit()
        {
            foreach (var effect in OnHitEffects)
            {
                if(effect is IEndableEffect endable) runtimeEndEffects.Add(endable);
                if(effect is IUpdateableEffect updatable) runtimeUpdateEffects.Add(updatable);
                effect.Eval(_caster,this);
            }
            if (Tags.Contains(TAG.TECH) && _caster.state.CurrentState is ActingState acting)
            {
                acting.OnEnd();
            }
        }
        public override void OnUpdate(float dt)
        {
            foreach (var effect in runtimeUpdateEffects)
            {
                effect.Update(_caster, this, dt);
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
}

