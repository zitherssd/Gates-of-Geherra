using System;
using UnityEngine;
using static Assets.BaseActorBattler;

namespace Assets.Scripts.Actions
{
    [CreateAssetMenu(fileName = "Reaction", menuName = "ScriptableObjects/Reaction", order = 1)]
    public class BaseReaction : BaseAction
    {
        public REACTIONTYPE ReactionType;
        private Vector2 direction;

        public float DamageModifier(float inputDamage)
        {
            switch (ReactionType)
            {
                case REACTIONTYPE.Dodge:
                    return inputDamage * 0f;
                case REACTIONTYPE.Block:
                    return inputDamage * 0.5f;
                case REACTIONTYPE.NONE:
                    return inputDamage;
                default:
                    return inputDamage;
            }
        }
        public float KnockbackModifier(float inputKnockbackForce)
        {
            switch (ReactionType)
            {
                case REACTIONTYPE.Dodge:
                    return inputKnockbackForce * 0f;
                case REACTIONTYPE.Block:
                    return inputKnockbackForce * 0.75f;
                case REACTIONTYPE.NONE:
                    return inputKnockbackForce;
                default:
                    return inputKnockbackForce;
            }
        }

        public float PostureModifier(float inputPosture)
        {
            switch (ReactionType)
            {
                case REACTIONTYPE.Dodge:
                    return inputPosture * 0f;
                case REACTIONTYPE.Block:
                    return inputPosture * 0.75f;
                case REACTIONTYPE.NONE:
                    return inputPosture;
                default:
                    return inputPosture;
            }
        }

        public void React(BaseActorBattler actor, Action onReactionComplete)
        {
            switch (ReactionType)
            {
                case REACTIONTYPE.Dodge:
                    var target = direction.normalized;
                    var camera = Camera.main;
                    var forward = camera.transform.forward; forward.y = 0;
                    var right = camera.transform.right; right.y = 0;
                    forward.Normalize(); right.Normalize();

                    var desiredMoveDirection = forward * target.y + right * target.x;
                    actor.MoveToPosition(desiredMoveDirection * 6 + actor.transform.position, MoveState.Sliding, () =>
                    {
                        actor.PlayAnimation("Idle"); 
                    });
                    actor.PlayAnimation("Step");
                    onReactionComplete();
                    break;
                case REACTIONTYPE.Block:
                    actor.isBlocking = true;
                    actor.PlayAnimation("Block");
                    onReactionComplete();
                    break;
                case REACTIONTYPE.NONE:
                    onReactionComplete();
                    break;
                default:
                    break;
            }
        }
        public void WarmUp(BaseActorBattler casterActor, Action onWarmUpEnd)
        {
            if (TotalUses != 0) remainingUses--;
            switch (ReactionType)
            {
                case REACTIONTYPE.NONE:
                    onWarmUpEnd();
                    break;
                case REACTIONTYPE.Dodge:
                    if (casterActor.isControllable())
                        InputManager.GetInstance().WaitForSwipe(() =>
                        {
                            direction = InputManager.GetInstance().GetSwipeDirection();
                            onWarmUpEnd();
                        });
                    else
                    {
                        direction = new Vector2(UnityEngine.Random.RandomRange(-200, 200), UnityEngine.Random.RandomRange(-200, 200));
                        onWarmUpEnd();
                    }
                    break;
                case REACTIONTYPE.Block:
                    onWarmUpEnd();
                    break;
            }


            //Play an animation before moving,
            //Show skill name,
            //Certain special effects
        }

        public override bool IsValid(BaseActorBattler caster, out string InvalidReason)
        {
            InvalidReason = "";
            return true;
        }

        //public new bool HasUsesLeft()
        //{
        //    if (TotalUses == 0) return true;
        //    if (remainingUses > 0) return true;
        //    else return false;
        //}

        public enum REACTIONTYPE { NONE, Dodge, Block }

    }
}