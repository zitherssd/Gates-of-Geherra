using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public abstract class BaseStatus
    {
        public BaseActorBattler owner;
        public abstract void ApplyEffects();
        public virtual void ModifyDamage(float damage) { }
        public virtual void ModifyPosture(float damage) { }
        public virtual void ModifyKnockback(Vector3 direction, float force) { }
           
    }
}