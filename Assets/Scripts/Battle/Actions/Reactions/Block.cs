using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Components.Status;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Reactions
{
    [CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Reaction/Block")]

    public class Block : BaseReaction
    {
        [Range(0, 2)] public float DamageModifier = 1f;
        [Range(0, 2)] public float PostureModifier = 1f;
        [Range(0, 2)] public float KnockbackModifier = 1f;
        protected override void PerformSpecific(Actor actor, Action onReactionComplete)
        {
            actor.KillAnimationEndEvent();
            actor.PlayAnimation("Block");
            actor.statusManager.Add(new BlockStatus(DamageModifier, PostureModifier, KnockbackModifier));
        }
    }


}
