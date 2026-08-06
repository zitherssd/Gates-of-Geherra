using Assets.Scripts.Battle.Actions;

namespace Assets.Scripts.Battle.Actions.Effects
{
    /// <summary>
    /// Effect evaluated once per enemy actually hit by an attack.
    /// Target-centric (unlike IEffect, which is caster-centric), so statuses can be
    /// applied to the specific actor(s) the hitbox connected with. Placed on
    /// DamageEffect.OnHitEffects. See AddStatusOnHitEffect for applying a status.
    /// </summary>
    public interface IOnHitEffect
    {
        void Eval(Actor.Actor target, Actor.Actor caster, BaseAction action);
    }
}
