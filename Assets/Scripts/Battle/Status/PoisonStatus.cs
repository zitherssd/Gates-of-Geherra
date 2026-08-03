using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    [CreateAssetMenu(fileName = "PoisonStatus", menuName = "ScriptableObjects/Status/PoisonStatus")]
    public class PoisonStatus : BaseStatus
    {
        public float DamagePerTick = 1f;
        public float Duration = 5f;
        public float TickInterval = 1f;

        private float _durationTimer;
        private float _tickTimer;

        public override void Apply()
        {
            _durationTimer = 0f;
            _tickTimer = 0f;
        }

        public override void Tick()
        {
            if (owner == null) return;

            _durationTimer += Time.deltaTime;
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= TickInterval)
            {
                _tickTimer -= TickInterval;
                var damageInstance = new DamageInstance { Damage = DamagePerTick };
                owner.ApplyDamageInstance(damageInstance, null, null);
            }

            if (_durationTimer >= Duration)
            {
                owner.statusManager.Remove(this);
            }
        }
    }
}