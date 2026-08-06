using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    public abstract class BaseStatus : ScriptableObject
    {
        public bool singleInstance = false;

        /// <summary>
        /// Duration in seconds before the status is removed automatically.
        /// 0 = no auto-removal (removed manually, e.g. via RemoveStatusEffect).
        /// </summary>
        public float Duration = 0f;

        [System.NonSerialized]
        public Actor.Actor owner;

        protected float durationTimer;

        public virtual void Apply()
        {
            durationTimer = 0f;
        }

        public virtual void Remove() { }

        /// <summary>
        /// Called every frame by StatusManager. Handles unified auto-removal after
        /// Duration elapses, then delegates per-frame logic to TickStatus().
        /// Subclasses should override TickStatus(), not Tick().
        /// </summary>
        public void Tick()
        {
            TickStatus();

            if (Duration > 0f)
            {
                durationTimer += Time.deltaTime;
                if (durationTimer >= Duration && owner != null && owner.statusManager != null)
                {
                    owner.statusManager.Remove(this);
                }
            }
        }

        protected virtual void TickStatus() { }
    }
}