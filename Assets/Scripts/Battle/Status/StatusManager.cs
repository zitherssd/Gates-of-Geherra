using Assets.Scripts.Battle.Manager;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actor;
using UnityEngine;
using Assets.Scripts.Battle.Actions;

namespace Assets.Scripts.Battle.Status
{
    public class StatusManager : MonoBehaviour
    {
        // Helper class to track the runtime instance of an effect
        public class ActiveStatusEffect
        {
            public StatusEffect Effect { get; }
            public float RemainingDuration { get; set; }
            public int Stacks { get; set; }
            private float _tickTimer;

            public ActiveStatusEffect(StatusEffect effect)
            {
                Effect = effect;
                RemainingDuration = effect.Duration;
                Stacks = 1;
                _tickTimer = 1f; // Fire the first tick immediately if needed
            }

            // Returns true if the tick is ready
            public bool Tick(float deltaTime)
            {
                _tickTimer += deltaTime;
                if (_tickTimer >= 1f)
                {
                    _tickTimer -= 1f;
                    return true;
                }
                return false;
            }
        }

        private Actor.Actor _actor;
        private readonly List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();

        public void Awake()
        {
            _actor = GetComponent<Actor.Actor>();
            if (_actor == null)
            {
                Debug.LogError("StatusManager requires an Actor component on the same GameObject.", this);
                enabled = false;
                return;
            }

            // Subscribe to Actor events
            _actor.OnBeforeTakeDamage += HandleBeforeTakeDamage;
            _actor.OnAfterTakeDamage += HandleAfterTakeDamage;
            _actor.OnBeforeDealDamage += HandleBeforeDealDamage;
            _actor.OnAfterDealDamage += HandleAfterDealDamage;
            _actor.OnActionUsed += HandleActionUsed;
            //BattleManager.instance.OnBattleEnd += HandleBattleEnd;
        }

        public void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (_actor == null) return;
            _actor.OnBeforeTakeDamage -= HandleBeforeTakeDamage;
            _actor.OnAfterTakeDamage -= HandleAfterTakeDamage;
            _actor.OnBeforeDealDamage -= HandleBeforeDealDamage;
            _actor.OnAfterDealDamage -= HandleAfterDealDamage;
            _actor.OnActionUsed -= HandleActionUsed;
            //BattleManager.instance.OnBattleEnd -= HandleBattleEnd;
        }

        void Update()
        {
            // Use a copy to allow modification during iteration
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var activeEffect = _activeEffects[i];

                // Handle duration
                if (activeEffect.Effect.Duration > 0)
                {
                    activeEffect.RemainingDuration -= Time.deltaTime;
                    if (activeEffect.RemainingDuration <= 0)
                    {
                        RemoveStatus(activeEffect);
                        continue; // Skip to next effect
                    }
                }
                
                // Handle per-frame logic
                foreach (var logic in activeEffect.Effect.EffectLogics)
                {
                    logic.OnUpdate(_actor);
                }
                
                // Handle per-second tick
                if (activeEffect.Tick(Time.deltaTime))
                {
                    foreach (var logic in activeEffect.Effect.EffectLogics)
                    {
                        logic.OnTick(_actor);
                    }
                }
            }
        }

        public void AddStatus(StatusEffect effect)
        {
            var existingEffect = _activeEffects.FirstOrDefault(e => e.Effect == effect);

            if (existingEffect != null)
            {
                if (effect.IsStackable)
                {
                    existingEffect.Stacks = Mathf.Min(existingEffect.Stacks + 1, effect.MaxStacks);
                }
                // Always refresh duration
                existingEffect.RemainingDuration = effect.Duration;
            }
            else
            {
                var newActiveEffect = new ActiveStatusEffect(effect);
                _activeEffects.Add(newActiveEffect);
                foreach (var logic in effect.EffectLogics)
                {
                    logic.OnApply(_actor);
                }
            }
        }

        private void RemoveStatus(ActiveStatusEffect activeEffect)
        {
            foreach (var logic in activeEffect.Effect.EffectLogics)
            {
                logic.OnRemove(_actor);
            }
            _activeEffects.Remove(activeEffect);
        }
        
        // --- Event Handlers ---
        // In a future step, these methods will loop through active effects
        // and call corresponding methods on their EffectLogic scripts.

        private void HandleBeforeTakeDamage(DamageInstance info) { }
        private void HandleAfterTakeDamage(DamageInstanceResult info) { }
        private void HandleBeforeDealDamage(DamageInstance info) { }
        private void HandleAfterDealDamage(DamageInstance info) { }
        private void HandleActionUsed(Actions.BaseAction action) { }
        private void HandleBattleEnd() { }
    }
}

