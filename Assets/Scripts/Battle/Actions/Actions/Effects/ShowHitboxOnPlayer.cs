using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class ShowHitboxOnPlayer : IEffect, IEndableEffect, IHitbox
    {
        public List<Vector3> HitboxPoints;
        public float HitboxScale = 1f;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            actor.effects.SetHitbox(HitboxPoints.Select(point => point * HitboxScale).ToList());
        }

        public void End(Actor.Actor actor, BaseAction action)
        {
            actor.effects.ClearHitbox();
        }
        public List<Actor.Actor> CheckEnemiesInsideHitbox(Actor.Actor casterActor)
        {
            // Transform hitbox points to world space based on caster's position and orientation
            List<Vector3> transformedPoints = new List<Vector3>();
            var validActors = new List<Actor.Actor>();

            foreach (var point in HitboxPoints)
            {
                // Rotate and position each point relative to the caster
                Vector3 worldPoint = casterActor.transform.position + casterActor.transform.TransformDirection(point * HitboxScale);
                transformedPoints.Add(worldPoint);
            }

            List<Actor.Actor> enemyActors = casterActor.isControllable
                ? BattleManager.instance.EnemyActors
                : BattleManager.instance.PlayerActors;


            foreach (var enemy in enemyActors.Where(actor => actor.state.IsAlive()))
            {
                Vector3 enemyPosition = enemy.transform.position;


                // Check if the center of the capsule is inside
                if (Intersections.IsPointInsidePolygon(enemyPosition, transformedPoints))
                {
                    validActors.Add(enemy);
                    continue;
                }
                float capsuleRadius = enemy.GetComponent<CapsuleCollider>().radius;

                // Check if the capsule collider intersects the polygon
                foreach (var edgeStart in transformedPoints)
                {
                    int nextIndex = (transformedPoints.IndexOf(edgeStart) + 1) % transformedPoints.Count;
                    Vector3 edgeEnd = transformedPoints[nextIndex];

                    if (Intersections.IsCircleIntersectingLine(enemyPosition, capsuleRadius, edgeStart, edgeEnd))
                    {
                        validActors.Add(enemy);
                        break;
                    }
                }
            }

            return validActors;
        }
    }

}
