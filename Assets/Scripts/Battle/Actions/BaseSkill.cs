using Assets.Scripts.Battle.States;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Actions
{
    public class BaseSkill : BaseAction
    {
        public System.Collections.IEnumerator WaitForOneFrame(Action action)
        {
            // This will wait for one frame
            yield return new WaitForSeconds(0.01f);

            // Code here will be executed on the frame after the wait
            action.Invoke();
        }
        public int RandomFromList(int count)
        {
            var random = new System.Random();
            var index = random.Next(count);
            return index;
        }
        public Vector3 GetRelativeToCamera(Vector2 direction)
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