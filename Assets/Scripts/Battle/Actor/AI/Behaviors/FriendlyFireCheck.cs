using Assets.Scripts.Battle.Manager;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    /// <summary>
    /// Static helpers for keeping ranged AI from hitting friendly actors
    /// (friendly fire) with straight-line projectiles.
    ///
    /// The thrower is an enemy, so its "friendlies" are the other actors in
    /// <see cref="BattleManager.EnemyActors"/> (self excluded) and its target is
    /// the player. The projectile corridor is modeled as a horizontal line at a
    /// given height with a half-width radius around it.
    /// </summary>
    internal static class FriendlyFireCheck
    {
        /// <summary>
        /// True when any alive friendly sits inside the projectile corridor
        /// between <paramref name="actor"/> and its target, meaning a throw would
        /// hit that friendly.
        /// </summary>
        /// <param name="actor">The throwing actor.</param>
        /// <param name="clearRadius">Half-width of the corridor that counts as blocked.</param>
        /// <param name="height">Height above ground the corridor is checked at (projectile travel height).</param>
        public static bool IsBlocked(Actor actor, float clearRadius, float height)
        {
            if (actor.target.ClosestEnemy == null) return false;

            Vector3 origin = actor.transform.position;
            origin.y = height;
            Vector3 target = actor.target.ClosestEnemy.transform.position;
            target.y = height;
            Vector3 toTarget = target - origin;
            float dist = toTarget.magnitude;
            if (dist < 0.001f) return false;
            Vector3 dir = toTarget / dist;

            foreach (var friendly in BattleManager.instance.EnemyActors)
            {
                if (friendly == null || friendly == actor) continue;
                if (friendly.Runtime == null || friendly.Runtime.isDead()) continue;

                Vector3 p = friendly.transform.position;
                p.y = height;
                Vector3 toFriendly = p - origin;
                float along = Vector3.Dot(toFriendly, dir);
                // Friendly must be between the thrower and the target (with a little margin).
                if (along < 0.1f || along > dist) continue;
                Vector3 closestPoint = origin + dir * along;
                if (Vector3.Distance(p, closestPoint) < clearRadius) return true;
            }
            return false;
        }

        /// <summary>
        /// Picks a lateral (perpendicular-to-the-line) direction to sidestep so
        /// the thrower can escape the nearest blocking friendly. If a blocker is
        /// found we step away from its side of the line; otherwise a
        /// deterministic side (derived from the instance id) is used.
        /// </summary>
        public static Vector3 ChooseSidestepDirection(Actor actor, float height)
        {
            Vector3 origin = actor.transform.position;
            origin.y = height;

            if (actor.target.ClosestEnemy == null)
                return actor.GetInstanceID() % 2 == 0 ? Vector3.right : Vector3.left;

            Vector3 target = actor.target.ClosestEnemy.transform.position;
            target.y = height;
            Vector3 toTarget = target - origin;
            Vector3 dir = toTarget.normalized;
            Vector3 lateralA = Vector3.Cross(Vector3.up, dir).normalized;
            Vector3 lateralB = -lateralA;

            // Find the friendly that is between us and the target and closest to the line.
            Actor nearest = null;
            float nearestPerp = float.MaxValue;
            foreach (var friendly in BattleManager.instance.EnemyActors)
            {
                if (friendly == null || friendly == actor) continue;
                if (friendly.Runtime == null || friendly.Runtime.isDead()) continue;

                Vector3 p = friendly.transform.position;
                p.y = height;
                Vector3 toFriendly = p - origin;
                float along = Vector3.Dot(toFriendly, dir);
                if (along < 0f || along > toTarget.magnitude) continue;
                float perp = Vector3.Cross(dir, toFriendly).magnitude;
                if (perp < nearestPerp)
                {
                    nearestPerp = perp;
                    nearest = friendly;
                }
            }

            if (nearest != null)
            {
                // Step to the side of the line OPPOSITE the blocker.
                float side = Vector3.Dot(lateralA, nearest.transform.position - origin);
                return side > 0 ? lateralB : lateralA;
            }

            return actor.GetInstanceID() % 2 == 0 ? lateralA : lateralB;
        }
    }
}
