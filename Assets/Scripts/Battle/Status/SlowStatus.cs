using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    [CreateAssetMenu(fileName = "SlowStatus", menuName = "ScriptableObjects/Status/SlowStatus")]
    public class SlowStatus : BaseStatus
    {
        public float SpeedMultiplier = 0.5f;

        public override void Apply()
        {
            base.Apply();
            if (owner != null && owner.movement != null)
                owner.movement.MoveSpeedMultiplier = SpeedMultiplier;
        }

        public override void Remove()
        {
            if (owner != null && owner.movement != null)
                owner.movement.MoveSpeedMultiplier = 1f;
        }
    }
}
