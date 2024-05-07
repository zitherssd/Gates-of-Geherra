using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Poison : BaseStatus
    {
        public Poison(Actor owner) : base(owner)
        {

        }
        public override void Apply()
        {
            owner.DamageDealt += ModifyDamage;
        }

        public float ModifyDamage(float damage)
        {
            float modifiedDamage = damage * 0.5f;
            return modifiedDamage;
        }
    }
}