using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Stagger : BaseStatus
    {
        public Stagger(Actor owner) : base(owner)
        {

        }

        public override void Apply()
        {
            if(owner.activeStates.OfType<Stagger>().Any())
            {
                owner.activeStates.Remove(this);
            }
            else
            {
                owner.KnockbackDealt += ModifyKnockback;
                owner.PlayAnimation("PostureBroken");
                owner.PlayAudio("Attack1");
            }
        }
        public override void Remove()
        {
            base.Remove();
            owner.KnockbackDealt -= ModifyKnockback;
        }

        private float ModifyKnockback(float force, Vector3 direction)
        {
            return force = force * 2;
        }


    }
}