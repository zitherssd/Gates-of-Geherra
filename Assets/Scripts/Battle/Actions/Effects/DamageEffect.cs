using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using System;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Battle.Actions.Effects
{
    [Serializable]
    public class DamageEffect : IEffect, IItemEffect
    {
        public bool UseHitbox = true;
        public IHitbox HitboxEffect; //How to get hitbox?
        public DamageInstance DamageData;
        
        private int currentWindowIndex = -1;


        public virtual void Eval(Actor.Actor actor, BaseAction action)
        {
            var gs = action as GenericSkill;
            if (gs == null) return;
            
            
            // Get current active window index
            currentWindowIndex = gs.CurrentActiveWindowIndex;
            
            // Only apply damage if we're in an active hit window
            if (currentWindowIndex < 0)
            {
                return;
            }
            
            // Only query hitbox if UseHitbox is enabled
            if (!UseHitbox)
            {
                return;
            }
            
            // Query hitbox from hit windows in animation phase
            HitboxEffect = null;
            foreach (var window in gs.animationPhase.hitWindows)
            {
                HitboxEffect = window.windowEffects.OfType<IHitbox>().FirstOrDefault();
                if (HitboxEffect != null)
                {
                    break;
                }
            }
            if (HitboxEffect == null)
            {
                return;
            }
            
            var enemies = HitboxEffect.CheckEnemiesInsideHitbox(actor);
            
            foreach (var enemy in enemies)
            {
                // Check if this enemy can be hit in current window
                if (!gs.HitWindowMgr.CanHit(enemy, currentWindowIndex))
                {
                    continue;
                }
                
                ApplyDamageEffects(actor, enemy, action);
                gs.HitWindowMgr.RecordHit(enemy, currentWindowIndex);

                // TECH behavior: successful hit cancels recovery so combos can continue.
                if (action != null && action.Tags != null && action.Tags.Contains(TAG.TECH))
                {
                    action.EndAction(ActionEndReason.Completed);
                    return;
                }
            }
        }

        public void Eval(Actor.Actor actor)
        {
            ApplyDamageEffects(actor, actor, null);
        }

        public void ApplyDamageEffects(Actor.Actor casterActor, Actor.Actor targetActor, BaseAction action)
        {
            // Need ro revise
            var targetBlocking = targetActor.state.IsBlocking();
            if (targetBlocking)
                casterActor.Runtime.ChangeBuildup(DamageData.BuildupGainOnHit / 2);
            else
                casterActor.Runtime.ChangeBuildup(DamageData.BuildupGainOnHit);

            targetActor.ApplyDamageInstance(DamageData, casterActor, action);
        }
    }
}
