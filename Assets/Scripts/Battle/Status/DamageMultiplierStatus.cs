using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    [CreateAssetMenu(fileName = "DamageMultiplierStatus", menuName = "ScriptableObjects/Status/DamageMultiplierStatus")]
    public class DamageMultiplierStatus : BaseStatus
    {
        public float Multiplier = 2f;

        public override void Apply()
        {
            if (owner == null) return;
            owner.OnBeforeDealDamage += ModifyDamage;
        }

        public override void Remove()
        {
            if (owner == null) return;
            owner.OnBeforeDealDamage -= ModifyDamage;
        }

        private void ModifyDamage(DamageInstance damageInstance)
        {
            damageInstance.Damage *= Multiplier;
            owner.statusManager.Remove(this);
        }
    }
}