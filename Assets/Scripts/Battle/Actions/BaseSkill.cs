using System;
using System.Reflection;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    public class BaseSkill : BaseAction
    {
        public ANIMATION Animation;
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
        public virtual void OnHit() { }

        public virtual void OnEnterWindup(Animator animator) { }

        public virtual void OnEnterRecovery(Animator animator) { }

        public float CalculateDuration(Animator animator)
        {
            // Get the AnimatorStateInfo for the current state in layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Get the RuntimeAnimatorController (holds all animation clips in the Animator)
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;

            if (controller != null)
            {
                // Loop through all animation clips and find the one matching the current state
                foreach (AnimationClip clip in controller.animationClips)
                {
                    if (clip.name == stateInfo.shortNameHash.ToString())
                    {

                        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                        FieldInfo eventsField = typeof(AnimationClip).GetField("m_Events", flags);

                        if (eventsField != null)
                        {
                            // Get the array of AnimationEvents
                            AnimationEvent[] animationEvents = (AnimationEvent[])eventsField.GetValue(clip);

                            // Loop through each event and log its time
                            foreach (AnimationEvent animEvent in animationEvents)
                            {
                                Debug.Log($"Event {animEvent.functionName} is set to fire at {animEvent.time} seconds.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Could not find 'm_Events' field via reflection.");
                        }
                    }
                }
            }
            float duration = 0f;
            


            return duration;
        }

    }

}