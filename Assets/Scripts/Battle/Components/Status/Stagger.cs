using UnityEngine;

namespace Assets.Scripts.Battle.Components.Status
{
    public class Stagger : BaseStatus
    {

        public Stagger()
        {
            base.singleInstance = true;
        }

        public override void Apply()
        {
            owner.KnockbackRecieved += ModifyKnockback;
            owner.PlayAnimation("PostureBroken");
            owner.audio.PlayAudio("Attack1");
        }
        public override void Remove()
        {
            base.Remove();
            owner.KnockbackRecieved -= ModifyKnockback;
        }

        private float ModifyKnockback(float force, Vector3 direction)
        {
            return force = force * 2;
        }


    }
}