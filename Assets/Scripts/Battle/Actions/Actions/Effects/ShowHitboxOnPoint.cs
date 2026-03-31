using Assets.Scripts.Battle.Actor.AI.Behaviors;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class ShowHitboxOnPoint : IEffect, IEndableEffect, IHitbox
    {
        public List<Vector3> HitboxPoints;
        public float HitboxScale = 1f;
        public Vector3 point;

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            actor.effects.SetHitbox(HitboxPoints.Select(point => point * HitboxScale).ToList(), actor.transform.position + action.Direction * action.StickMult);
            point = actor.transform.position + action.Direction * action.StickMult;
        }

        public void End(Actor.Actor actor, BaseAction action)
        {
            actor.effects.ClearHitbox();
        }
        public List<Actor.Actor> CheckEnemiesInsideHitbox(Actor.Actor casterActor)
        {
            List<Vector3> transformedPoints = new List<Vector3>();
            var validActors = new List<Actor.Actor>();

            Vector3 hitboxCenter = point;

            foreach (var hitboxPoint in HitboxPoints)
            {
                Vector3 rotated = casterActor.transform.TransformDirection(hitboxPoint * HitboxScale);
                Vector3 worldPoint = hitboxCenter + rotated;

                transformedPoints.Add(worldPoint);
            }

            // ------------ Collision checks -----------------

            List<Actor.Actor> enemyActors = casterActor.isControllable
                ? BattleManager.instance.EnemyActors
                : BattleManager.instance.PlayerActors;

            foreach (var enemy in enemyActors.Where(actor => actor.state.IsAlive()))
            {
                Vector3 enemyPosition = enemy.transform.position;
                if (enemy.state.IsKnockedDown()) continue;
                
                // Check center inside polygon
                if (Intersections.IsPointInsidePolygon(enemyPosition, transformedPoints))
                {
                    if (!enemy.state.IsKnockedDown())
                    {
                        validActors.Add(enemy);
                        continue;
                    }
                }

                float radius = enemy.GetComponent<CapsuleCollider>().radius;

                // Edge intersection test
                for (int i = 0; i < transformedPoints.Count; i++)
                {
                    Vector3 start = transformedPoints[i];
                    Vector3 end = transformedPoints[(i + 1) % transformedPoints.Count];

                    if (Intersections.IsCircleIntersectingLine(enemyPosition, radius, start, end))
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
