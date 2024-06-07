using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.State.States
{
    public class BlockState : IState
    {
        private readonly Actor owner;

        [Range(0, 2)] public float DamageModifier = 1f;
        [Range(0, 2)] public float PostureModifier = 1f;
        [Range(0, 2)] public float KnockbackModifier = 1f;

        public BlockState(Actor owner)
        {
            this.owner = owner;
        }

        public BlockState Set(float DamageModifier, float PostureModifier, float KnockbackModifier)
        {
            this.DamageModifier = DamageModifier;
            this.PostureModifier = PostureModifier;
            this.KnockbackModifier = KnockbackModifier;
            return this;
        }

        public void Enter()
        {
            owner.KillAnimationEndEvent();
            owner.PlayAnimation("Block");
            owner.DamageRecieved += ModifyDamage;
            owner.KnockbackRecieved += ModifyKnockback;
            BattleManager.instance.OnNewTurn += Remove;
        }

        public void Exit()
        {
            owner.DamageRecieved -= ModifyDamage;
            owner.KnockbackRecieved -= ModifyKnockback;
            BattleManager.instance.OnNewTurn -= Remove;
        }

        public float ModifyDamage(float damage)
        {
            float modifiedDamage = damage * DamageModifier;
            return modifiedDamage;
        }

        public float ModifyKnockback(float knockback, Vector3 direction)
        {
            return knockback * KnockbackModifier;
        }

        public void Remove(uint currentTurn)
        {
            owner.state.TransitionTo(owner.state.idleState);
        }

        public void Update()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
            throw new System.NotImplementedException();
        }
    }
}