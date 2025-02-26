using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public class DodgeBehavior : IAIBehavior
    {
        public bool Execute(AISystem ai, Actor actor)
        {
            // Check for an incoming attack
            AttackSkill incomingAttack;
            if (actor.target.ClosestEnemy.state.IsAttacking(out incomingAttack))
            {
                if (CheckIfInsideHitbox(incomingAttack.HitboxPoints, actor))
                {
                    var reaction = ChooseDodge(actor, incomingAttack);
                    if (reaction != null)
                    {
                        actor.UseAction(reaction, actor.state.TransitionToIdle);

                        return true;
                    }
                }
            }

            return false;
        }

        private BaseAction ChooseDodge(Actor actor, BaseAction incomingAction)
        {
            var randomDodge = actor.ActorData.actions.OfType<Dodge>().Where(action => action.IsValid(actor, out _)).OrderBy(x => UnityEngine.Random.value).FirstOrDefault();

            if (randomDodge != null)
            {

                randomDodge.Direction = -actor.target.DirectionToClosestEnemy;
                return randomDodge;
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


