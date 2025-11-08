using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Components.Status;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Reactions
{
    [CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Action/Block")]

    public class Block : BaseSkill
    {
        [Range(0, 2)] public float DamageModifier = 1f;
        [Range(0, 2)] public float PostureModifier = 1f;
        [Range(0, 2)] public float KnockbackModifier = 1f;
        public float StaminaCostMult = 1f;
        public float duration;
        public float startupDelay;
        public Action onSucessfulBlock;
        protected override void PerformSpecific(Actor.Actor actor, Action onReactionComplete)
        {
            OnCancel = onReactionComplete;
            var blockState = actor.state.TransitionTo<BlockState>();
            blockState.Set(this);
        }
    }


}
