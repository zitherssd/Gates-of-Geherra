using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class ReactionBehavior : IAIBehavior
    {
        private float reactionTimer = 0f; // Time until AI can react
        private float cooldownTimer = 0f; // Time until AI can react again
        public float Cooldown = 3f;
        private float transitionDelay = 0.25f;
        public ReactionBehavior(float cooldownTimer)
        {
            Cooldown = cooldownTimer;
        }

        public ReactionBehavior()
        {
            Cooldown = 3f;
        }


        public bool Execute(AISystem ai, Actor actor)
        {
            // Check for an incoming attack
            AttackSkill incomingAttack;
            if (actor.target.ClosestEnemy.state.IsAttacking(out incomingAttack))
            {
                if (CheckIfInsideHitbox(incomingAttack.HitboxPoints, actor))
                {
                    if (reactionTimer <= 0)
                    {
                        var reaction = ChooseReaction(actor, incomingAttack);
                        if (reaction != null)
                        {
                            actor.UseAction(reaction, actor.state.TransitionToIdle);

                            // Set cooldown time (e.g., 0.5s)
                            return true;
                        }
                        // Set a random thinking delay (50ms to 250ms)
                        //reactionTimer = Random.Range(0.03f, 0.15f);
                    }
                }
            }

            // If reactionTimer is still running, decrease it
            //if (reactionTimer > 0)
            //{
            //    reactionTimer -= Time.deltaTime;
            //    if (reactionTimer <= 0)
            //    {
            //        // Time to react!
            //        var reaction = ChooseReaction(actor, incomingAttack);
            //        if (reaction != null)
            //        {
            //            actor.UseAction(reaction, actor.state.TransitionToIdle);
                        
            //            // Set cooldown time (e.g., 0.5s)
            //            return true;
            //        }
            //    }
            //}

            return false;
        }

        private BaseAction ChooseReaction(Actor actor, BaseAction incomingAction)
        {
            var allBlocksAndDodges = actor.ActorData.actions.Where(action => action is Block || action is Dodge);

            var validReactions = allBlocksAndDodges.Where(item => item.IsValid(actor, out _));
            var randomReaction = validReactions.OrderBy(x => Random.value).FirstOrDefault();

            if (randomReaction != null)
            {
                if (randomReaction is Dodge dodge)
                {
                    dodge.Direction = Vector3.Cross(actor.target.DirectionToClosestEnemy, Vector3.up);
                    return dodge;
                }
                if (randomReaction is Block block)
                {
                    //block.onSucessfulBlock += actor.state.TransitionToIdle;
                    return block;
                }
            }
            return null;
        }

        private bool CheckIfInsideHitbox(List<Vector3> HitboxPoints, Actor actor)
        {
            // Transform hitbox points to world space based on caster's position and orientation
            List<Vector3> transformedPoints = new List<Vector3>();
            var validActors = new List<Actor>();

            foreach (var point in HitboxPoints)
            {
                // Rotate and position each point relative to the caster
                Vector3 worldPoint = actor.target.ClosestEnemy.transform.position + actor.target.ClosestEnemy.transform.TransformDirection(point);
                transformedPoints.Add(worldPoint);
            }

            // Check if the center of the capsule is inside
            if (AttackSkill.IsPointInsidePolygon(actor.transform.position, transformedPoints))
            {
                return true;
            }

            float capsuleRadius = actor.GetComponent<CapsuleCollider>().radius;

            // Check if the capsule collider intersects the polygon
            foreach (var edgeStart in transformedPoints)
            {
                int nextIndex = (transformedPoints.IndexOf(edgeStart) + 1) % transformedPoints.Count;
                Vector3 edgeEnd = transformedPoints[nextIndex];

                if (AttackSkill.IsCircleIntersectingLine(actor.transform.position, capsuleRadius, edgeStart, edgeEnd))
                {
                    return true;
                }
            }
            return false;
        }
    }
}


