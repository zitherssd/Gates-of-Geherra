using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    [CreateAssetMenu(fileName = "DamageOverTimeLogic", menuName = "Status Effects/Logics/Damage Over Time")]
    public class DamageOverTimeLogic : EffectLogic
    {
        public float Damage;

        public override void OnTick(Actor.Actor target)
        {
            target.Runtime.DealDamage(Damage);
        }
    }
}
