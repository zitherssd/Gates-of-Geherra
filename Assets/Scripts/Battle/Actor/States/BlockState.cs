using System;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Pattern;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public class BlockState : IState
    {
        private readonly Actor owner;

        [Range(0, 2)] public float DamageModifier = 1f;
        [Range(0, 2)] public float PostureModifier = 1f;
        [Range(0, 2)] public float KnockbackModifier = 1f;
        private float duration;
        private float timeSpentInBlockDuration = 0f;
        private Block skill;
        private int numberOfHits = 0;

        public BlockState(Actor owner)
        {
            this.owner = owner;
        }

        public BlockState Set(Block block)
        {
            if (block.blockType == Block.BlockType.Block)
            {
                timeSpentInBlockDuration = 0f;
                numberOfHits = 0;
            }
            this.DamageModifier = block.DamageModifier;
            this.PostureModifier = block.PostureModifier;
            this.KnockbackModifier = block.KnockbackModifier;
            this.duration = block.duration;
            this.skill = block;
            return this;
        }

        public void Enter()
        {
            owner.GetComponentInChildren<ActorUIController>().SetCC(duration, "Block");
            owner.PlayAnimation("Block");
            owner.OnBeforeTakeDamage += ModifyIncomingDamage;
            owner.StaminaRegenRate = 0.5f;
        }

        public void Exit()
        {
            var action = skill;
            if (action != null)
            {
                owner.GetComponentInChildren<ActorUIController>().HideCC();
                owner.OnBeforeTakeDamage -= ModifyIncomingDamage;
                owner.PlayAnimation("Idle");
                owner.StaminaRegenRate = 1f;

                // If the action is still running when we exit, it means it was interrupted.
                skill = null; // Clean up for next use.
                if (owner.GetCurrentAction() == action)
                {
                    action.EndAction(ActionEndReason.Interrupted);
                }
            }
        }

        private void ModifyIncomingDamage(DamageInstance damageInstance)
        {
            if (skill == null) return;

            // This is a successful block.
            skill.onSucessfulBlock?.Invoke();

            // Apply modifiers
            float originalDamage = damageInstance.Damage;
            damageInstance.Damage *= DamageModifier;
            damageInstance.PostureDamage *= PostureModifier;
            damageInstance.KnockbackForce *= KnockbackModifier;

            // Consume stamina
            owner.Runtime.DealStaminaDamage(originalDamage * skill.StaminaCostMult);

            // Handle block-type specific logic
            if (skill.blockType == Block.BlockType.Parry)
            {
                if (owner.isControllable)
                {
                    // Slowdown handled by state-based system in IdleState
                }
                owner.Runtime.ChangeBuildup(skill.BuildupGainOnBlock);
                skill.EndAction(ActionEndReason.Completed); // Parry ends the block immediately with success.
            }
            else if (skill.blockType == Block.BlockType.Block)
            {
                // Increase duration logic
                switch (numberOfHits)
                {
                    case 0:
                        duration = Mathf.Min(1, duration + 0.5f);
                        break;
                    case 1:
                        duration = Mathf.Min(1, duration + 0.33f);
                        break;
                    default:
                        duration = Mathf.Min(1, duration + 0.25f);
                        break;
                }
                owner.GetComponentInChildren<ActorUIController>().SetCC(duration, "Block");
                owner.Runtime.ChangeBuildup(skill.BuildupGainOnBlock);
                numberOfHits++;
            }
            else if (skill.blockType == Block.BlockType.Guard)
            {
                if (owner.isControllable)
                {
                    // Slowdown handled by state-based system in IdleState
                }
            }
        }

        public void Update()
        {
            if (skill == null)
            {
                // If skill is null, we should not be in this state. Transition to idle.
                // This can happen if the block action ends for any reason (e.g. successful parry).
                if (owner.state.CurrentState == this)
                {
                    owner.state.TransitionToIdle();
                }
                return;
            }

            if (skill.blockType == Block.BlockType.Block)
            {
                timeSpentInBlockDuration += Time.deltaTime;
                if (timeSpentInBlockDuration >= 2.5f)
                {
                    skill.EndAction(ActionEndReason.Completed);
                    return;
                }
            }
            
            duration -= Time.deltaTime;
            if (duration < 0)
            {
                skill.EndAction(ActionEndReason.Completed);
                return;
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            //throw new NotImplementedException();
        }
    }
}