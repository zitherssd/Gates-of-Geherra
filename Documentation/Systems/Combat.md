# Combat System

> **Last verified:** 2026-08-03

## Purpose

The core real-time combat system. Manages all fighting between actors — damage dealing, hitbox detection, actions, stagger, and death.

---

## Responsibilities

- Execute player and AI actions via `BaseAction` subclasses.
- Apply damage, posture damage, and knockback via `DamageInstance`.
- Detect hits via polygon-capsule hitbox math (no Unity colliders for hitboxes).
- Transition actors through combat states (Acting, Stagger, Death, etc.).
- Manage posture/stamina/buildup resources.
- Trigger status effects from attacks.

---

## Main Scripts

| Script | Role |
|--------|------|
| `Actor.cs` | Central coordinator; `UseAction()`, `ApplyDamageInstance()`, `DealDamage()` |
| `ActorStateMachine.cs` | State machine; animation event entry points |
| `BaseAction.cs` | Base action lifecycle: Begin→PerformSpecific→EndAction |
| `BaseSkill.cs` | Adds animation hooks: OnEnterWindup, OnHit, OnEnterRecovery |
| `AttackSkill.cs` | Hitbox-based melee attack |
| `GenericSkill.cs` | Effect-list driven flexible action |
| `ProjectileAttack.cs` | Spawns `ProjectileHandler` prefab |
| `Dodge.cs` | Force-based evasion |
| `Block.cs` | Enters `BlockState` for defense stance |
| `Charge.cs` | Directional force attack |
| `Jump.cs` | Transitions to air state |
| `DamageInstance.cs` | Data container for a single damage event; `Calculate()` applies modifiers |
| `ActingState.cs` | State during action animation; routes animation events |
| `StaggerState.cs` | Duration-based stagger |
| `DeathState.cs` | Death animation state |
| `EffectManager.cs` | Per-actor effect state (hitbox polygon, colors) |
| `Intersections.cs` | Math: `IsPointInsidePolygon`, `IsCircleIntersectingLine` |

---

## Data Sources

- `ActorDefinition.baseActions` → cloned into `ActorRuntime.actions` on spawn
- `ActorRuntime.currentPosture/Stamina/Buildup` → resource tracking
- `BaseAction.StaminaCost`, `BuildupCost`, `CooldownTimer` → cost gating
- `AttackSkill.HitboxPoints` → hitbox polygon definition

---

## Events

| Event | Source | Consumer |
|-------|--------|---------|
| `Actor.OnBeforeTakeDamage(DamageInstance)` | `ApplyDamageInstance()` | Items (DamageMultiplier trinket) |
| `Actor.OnAfterTakeDamage(DamageInstanceResult)` | `ApplyDamageInstance()` | UI, audio |
| `Actor.OnBeforeDealDamage(DamageInstance)` | `DealDamage()` | Items |
| `Actor.OnAfterDealDamage(DamageInstance)` | `DealDamage()` | Items |
| `Actor.OnActionUsed(BaseAction)` | `UseAction()` | AI, UI |
| `BaseAction.OnActionEnded(action, reason)` | `EndAction()` | `Actor.OnActionEnded()` → state transition |
| `ActorRuntime.OnDeath` | `DealDamage()` when HP→0 | [UNVERIFIED — no listeners found in reviewed code] |

---

## External Dependencies

- `SlowdownManager` — triggered on player hit (0.4s temporary slowdown), on player action use (exit state slowdown)
- `CameraManager.SlowTrack` — enabled on death hit
- `BattleManager.EnemyActors / Player` — checked by `BattleActiveState`
- `StatusManager` — ticked in `Actor.Update()`, applies status effects
- `ItemEventBus` — `OnDamageTaken` raised in `ApplyDamageInstance()`

---

## Extension Points

- Add new action type: create class extending `BaseSkill`, override `PerformSpecific()` and animation hooks.
- Add new effect: implement `IEffect` (and optionally `IEndableEffect`, `IUpdateableEffect`), use via `GenericSkill.OnHitEffects`.
- Modify damage calculation: edit `DamageInstance.Calculate()`.
- Add new actor state: implement `IState`, register in `ActorStateMachine.Awake()`.

---

## Risks

- **`BattleManager.instance` null-guarded** in `Actor.Update()` (fixed 2026-08-03) — but an `Actor` outside a battle still has no battle systems wired.
- **Animation event timing is fragile** — `EnterWindup`, `OnHit`, `EnterRecovery`, `OnEnd` must be precisely placed in the Animator. Wrong frame → action never fires damage or never ends.
- **Hitbox polygon is manual** — `AttackSkill.HitboxPoints` defined as world-space offsets; incorrect setup produces invisible or oversized hitboxes.
- **No multi-hit immunity** — same enemy can be hit multiple times per attack if DamageTickEffect is active.

---

## Common Modification Locations

- Add a new attack: `Resources/Actions/Attacks/` (new `AttackSkill` or `GenericSkill` asset).
- Change damage formula: `DamageInstance.Calculate()`.
- Change stagger trigger/duration: `Actor.ApplyDamageInstance()` — staggers on posture break (`currentPosture` reaches 0); duration from overkill past 0 (0% → 0.4s, 100% → 1.8s). Knockback-up only applies on break hits or already-staggered actors.
- Extend AI combatant reactions: `Actor.ApplyDamageInstance()` damage/death branches.
