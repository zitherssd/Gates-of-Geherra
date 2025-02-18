using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "Taunt", menuName = "ScriptableObjects/Skills/Taunt")]
    public class GenericSkill : BaseSkill
    {
        private Actor casterActor;

        [SerializeReference, SubclassSelector]

        public List<IEffect> OnHitEffects = new List<IEffect>();

        public override void OnHit()
        {
            foreach (var effect in OnHitEffects)
            {
                effect.Eval(casterActor);
            }
        }
    }

    public interface IEffect
    {
         void Eval(Actor actor);
    }

    [Serializable]
    public class StaminaHealEffect : IEffect
    {
        public float HealAmount;

        public void Eval(Actor actor)
        {
            actor.ActorData.DealStaminaDamage(-HealAmount);
        }
    }

    [Serializable]
    public class MeterGainEffect : IEffect
    {
        public float MeterGainValue;
        public void Eval(Actor actor)
        {
            if (actor.isControllable)
                UIManager.instance.GainMeter(MeterGainValue);
        }
    }

}
