using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Stagger : BaseStatus
    {
        public Stagger(BaseActorBattler owner)
        {
            this.owner = owner;
            ApplyEffects();
        }

        ~Stagger()
        {
            owner.onKnockbackRecieved -= ModifyKnockback;
        }
        public override void ApplyEffects()
        {
            if(owner.activeStates.OfType<Stagger>().Any())
            {
                owner.activeStates.Remove(this);
            }
            else
            {
                owner.onKnockbackRecieved += ModifyKnockback;
                owner.HandlePostureBreak();
            }
        }

        public override void ModifyKnockback(Vector3 direction, float force)
        {
            force = force * 2;
        }
    }
}