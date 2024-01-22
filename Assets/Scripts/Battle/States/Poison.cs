using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Poison : BaseStatus
    {
        public override void ApplyEffects()
        {
            owner.onDamageRecieved += ModifyDamage;
        }

        public void ModifyDamage(float damage)
        {

        }
    }
}