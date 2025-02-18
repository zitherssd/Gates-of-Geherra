using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Components.AI;
using Assets.Scripts.Battle.Components.AI.Behaviors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State

{
    public class BlockBehavior : IAIBehavior
    {
        private float ChanceToBlock;
        private AttackSkill lastAttack;
        public BlockBehavior(float chanceToBlock)
        {
            ChanceToBlock = chanceToBlock;
        }

        public bool Execute(AISystem ai, Actor actor)
        {
            // Check for an incoming attack
            AttackSkill incomingAttack;
            if (actor.target.ClosestEnemy.state.IsAttacking(out incomingAttack))
            {
                if (incomingAttack != lastAttack)
                {
                    if (CheckIfInsideHitbox(incomingAttack.HitboxPoints, actor))
                    {
                        lastAttack = incomingAttack;
                        if (UnityEngine.Random.value < ChanceToBlock)
                        {
                            var block = GetBlock(actor, incomingAttack);
                            if (block)
                            {
                                actor.UseAction(block, actor.state.TransitionToIdle);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private BaseAction GetBlock(Actor actor, BaseAction incomingAction)
        {
            var randomBlock = actor.ActorData.actions.OfType<Block>().Where(action => action.IsValid(actor, out _)).OrderBy(x => UnityEngine.Random.value).FirstOrDefault();

            if (randomBlock != null)
            {
                return randomBlock;
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
