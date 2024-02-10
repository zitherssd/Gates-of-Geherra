using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Poison : BaseStatus
    {
        public Poison(BaseActorBattler owner) : base(owner)
        {

        }
        public override void Apply()
        {
            owner.ApplyDamageModifiers += ModifyDamage;
        }

        public float ModifyDamage(float damage)
        {
            float modifiedDamage = damage * 0.5f;
            return modifiedDamage;
        }
    }
}