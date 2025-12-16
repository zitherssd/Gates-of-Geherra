using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    public abstract class EffectLogic : ScriptableObject
    {
        // Called when the status effect is first applied to the target.
        public virtual void OnApply(Actor.Actor target) { }

        // Called every frame while the status effect is active.
        public virtual void OnUpdate(Actor.Actor target) { }
        
        // Called once per second while the status effect is active.
        public virtual void OnTick(Actor.Actor target) { }

        // Called when the status effect is removed or expires.
        public virtual void OnRemove(Actor.Actor target) { }
    }
}
