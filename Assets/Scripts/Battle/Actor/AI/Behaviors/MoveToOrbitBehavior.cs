using Assets.Scripts.Battle.Actions.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    internal class MoveToOrbitBehavior : BTNode
    {
        public float radius = 3f;
        public float angularSpeed = 50f;

        private float angle;

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            var moveSkill = actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault();
            if (moveSkill == null) return NodeState.Failure;
            var player = actor.target.ClosestEnemy;
            angle += angularSpeed * Time.deltaTime;
            angle %= 360f; // Keep angle within 0-360

            // Convert angle to radians
            float rad = angle * Mathf.Deg2Rad;

            // Calculate new position around the player
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
            Vector3 destination = player.transform.position + offset;

            var agent = actor.movement.agent;
            agent.SetDestination(destination);

            moveSkill.Direction = agent.desiredVelocity.normalized;
            actor.UseAction(moveSkill);

            return NodeState.Sucess;
        }
    }
}
