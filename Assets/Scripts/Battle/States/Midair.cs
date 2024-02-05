using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Midair : BaseStatus
    {
        public Midair(BaseActorBattler owner) : base(owner)
        {

        }

        public override void Apply()
        {
            owner.PlayAnimation("HurtAir");
        }
        public override void Remove()
        {
            base.Remove();
        }
    }
}