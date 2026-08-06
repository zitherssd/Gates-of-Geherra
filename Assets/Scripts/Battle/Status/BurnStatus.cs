using Assets.Scripts.Battle.Actions;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    [CreateAssetMenu(fileName = "BurnStatus", menuName = "ScriptableObjects/Status/BurnStatus")]
    public class BurnStatus : BaseStatus
    {
        public float DamagePerTick = 2f;
        public float TickInterval = 1f;

        private float _tickTimer;

        public override void Apply()
        {
            base.Apply();
            _tickTimer = 0f;
        }

        protected override void TickStatus()
        {
            if (owner == null) return;

            _tickTimer += Time.deltaTime;

            if (_tickTimer >= TickInterval)
            {
                _tickTimer -= TickInterval;
                var damageInstance = new DamageInstance { Damage = DamagePerTick };
                owner.ApplyDamageInstance(damageInstance, null, null);
            }
        }
    }
}
