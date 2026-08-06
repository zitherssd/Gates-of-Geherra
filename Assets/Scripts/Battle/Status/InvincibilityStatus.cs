using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    [CreateAssetMenu(fileName = "InvincibilityStatus", menuName = "ScriptableObjects/Status/InvincibilityStatus")]
    public class InvincibilityStatus : BaseStatus
    {
        // Immunity is enforced by Actor.IsInvincible / the early-return in
        // Actor.ApplyDamageInstance while this status is active.
        //
        // Duration = 0 keeps it active until removed (e.g. via RemoveStatusEffect in a
        // later hit window). Set a positive Duration for a safety cap so an interruption
        // can't leave the actor permanently invincible.
    }
}
