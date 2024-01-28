using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Stagger : BaseStatus
    {
        public Stagger(BaseActorBattler owner) : base(owner)
        {
        }

        ~Stagger()
        {
            owner.onKnockbackRecieved -= ModifyKnockback;
        }
        public override void Apply()
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