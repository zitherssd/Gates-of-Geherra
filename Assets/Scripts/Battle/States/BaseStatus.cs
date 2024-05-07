using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public abstract class BaseStatus
    {
        public Actor owner;

        public BaseStatus(Actor owner)
        {
            this.owner = owner;
            Apply();
        }

        public abstract void Apply();
        public virtual void Remove() { owner.activeStates.Remove(this); }
        //public virtual void ModifyDamage(float damage) { }
        public virtual void ModifyPosture(float damage) { }
        public virtual void ModifyKnockback(Vector3 direction, float force) { }
           
    }
}