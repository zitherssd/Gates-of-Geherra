using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Status;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
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
        public float BuildupGainOnBlock;
        public BlockType blockType = BlockType.Block;
        public Action onSucessfulBlock;
        protected override void PerformSpecific(Actor.Actor actor, Action onReactionComplete)
        {
            actor.state.GetState<BlockState>().Set(this);
            var blockState = actor.state.TransitionTo<BlockState>();
        }

        public enum BlockType
        {
            Block,
            Parry,
            Guard,
        }
    }


}
