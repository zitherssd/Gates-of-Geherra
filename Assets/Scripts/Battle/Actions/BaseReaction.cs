using Assets.Scripts.Battle.States;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.BaseActorBattler;

namespace Assets.Scripts.Actions
{
    [CreateAssetMenu(fileName = "Reaction", menuName = "ScriptableObjects/Reaction", order = 1)]
    public class BaseReaction : BaseAction
    {
        public static BaseReaction NoReaction = null;

        public REACTIONTYPE ReactionType;
        public float knockbackModifier;
        public float damageModifier;
        public float postureModifier;
        private Vector2 direction;

        public override void Perform(BaseActorBattler actor, Action onReactionComplete)
        {
            UpdateReaminingUses();
            ResetCooldown();

            switch (ReactionType)
            {
                case REACTIONTYPE.Dodge:
                    var desiredMoveDirection = GetRelativeToCamera(StickValue);
                    actor.PlayAnimation("Step");

                    actor.GetComponent<Rigidbody>().AddForce(desiredMoveDirection.normalized * 100 * 2);

                    //actor.MoveToPosition(desiredMoveDirection * 6 + actor.transform.position, MoveState.Sliding, () =>
                    //{
                    //    actor.PlayAnimation("Idle"); 
                    //});
                    break;
                case REACTIONTYPE.Block:
                    actor.PlayAnimation("Block");
                    actor.activeStates.Add(new Block(actor));
                    //addstate for blocking
                    //onReactionComplete();
                    break;
                case REACTIONTYPE.NONE:
                    //onReactionComplete();
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
                        InputManager.instance.WaitForSwipe(delta =>
                        {
                            direction = delta;
                            onWarmUpEnd();
                        });
                    else
                    {
                        direction = new Vector2(UnityEngine.Random.Range(-200, 200), UnityEngine.Random.Range(-200, 200));
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
        public enum REACTIONTYPE { NONE, Dodge, Block }

        private Vector3 GetRelativeToCamera(Vector2 direction)
        {
            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * direction.y + right * direction.x;
            return desiredMoveDirection;
        }

    }
}