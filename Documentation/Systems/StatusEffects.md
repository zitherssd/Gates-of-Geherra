# Status Effect System

> **Last verified:** 2026-08-03

## Purpose

Applies time-based or event-based modifiers to actors during combat (poison, damage amplification, block bonuses, forced stagger, etc.).

---

## Responsibilities

- Track active status effects on each actor.
- Tick effects each frame via `StatusManager`.
- Unified auto-removal timer in `BaseStatus` (`Duration` seconds; `0` = manual removal only).
- Apply status from actions three ways:
  1. `AddStatusEffect` (hit-window effect) — status to caster or closest enemy (buffs/debuffs).
  2. `AddStatusOnHitEffect` (`DamageEffect.OnHitEffects`) — status to each enemy actually hit.
  3. `RemoveStatusEffect` — turn a status OFF (e.g. invincibility toggled off in a later hit window).

---

## Main Scripts

| Script | Role |
|--------|------|
| `StatusManager.cs` | Per-actor MonoBehaviour; holds active statuses, ticks them; `singleInstance` honored; `HasStatus<T>()`/`GetStatus<T>()` helpers |
| `BaseStatus.cs` | Abstract base for all status ScriptableObjects; unified `Duration` auto-removal via `TickStatus()` |
| `PoisonStatus.cs` | Periodic damage over time |
| `BurnStatus.cs` | Fire damage over time (same DoT pattern as poison) |
| `SlowStatus.cs` | Movement speed reduction via `MovementSystem.MoveSpeedMultiplier` |
| `InvincibilityStatus.cs` | Full damage immunity via `Actor.IsInvincible` |
| `BlockStatus.cs` | Modifiers active during block stance |
| `DamageMultiplierStatus.cs` | Scales incoming or outgoing damage |
| `DoubleDamageStatus.cs` | 2× damage multiplier variant |
| `Stagger.cs` | Likely forces actor into StaggerState |
| `AddStatusEffect.cs` | (`Actions/Effects/`) IEffect — applies a status to caster or closest enemy |
| `RemoveStatusEffect.cs` | (`Actions/Effects/`) IEffect — removes an active status by type |
| `AddStatusOnHitEffect.cs` | (`Actions/Effects/`) IOnHitEffect — applies a status to each enemy actually hit |
| `IOnHitEffect.cs` | (`Actions/Effects/`) target-centric interface, evaluated per hit by `DamageEffect.OnHitEffects` |

---

## Known Status Assets (`Resources/Statuses/`)

| Asset | Type |
|-------|------|
| `DamageMultiplierStatus.asset` | `DamageMultiplierStatus` |
| `PoisonStatus.asset` | `PoisonStatus` |
| `SlowStatus.asset` | `SlowStatus` (SpeedMultiplier 0.5, Duration 4) |
| `BurnStatus.asset` | `BurnStatus` (DamagePerTick 2, TickInterval 1, Duration 5) |
| `InvincibilityStatus.asset` | `InvincibilityStatus` (Duration 0 = until removed) |

---

## Data Sources

- `BaseStatus` ScriptableObjects define effect parameters (damage rate, multiplier, duration, etc.)
- `AddStatusEffect` as a hit-window effect → applies status to caster or closest enemy
- `AddStatusOnHitEffect` in `DamageEffect.OnHitEffects` → applies status to each enemy actually hit
- `RemoveStatusEffect` as a hit-window effect → removes an active status by type

---

## Events

None confirmed. Status ticks operate via `StatusManager.Tick()` called from `Actor.Update()`.

---

## External Dependencies

- `Actor.statusManager` — `StatusManager` is a MonoBehaviour on the Actor GameObject
- `Actor.Update()` — calls `statusManager.Tick()` every frame
- `DamageInstance` — status effects likely call back into damage or modifier APIs [UNVERIFIED details]

---

## Extension Points

- Add a new status: subclass `BaseStatus`, override `Apply()`/`Remove()`/`TickStatus()`, set `Duration` for auto-removal, create asset via `CreateAssetMenu` (`ScriptableObjects/Status/...`).
- Buff/debuff from an action: add `AddStatusEffect` to a hit window's `windowEffects` (target = caster or closest enemy).
- Status on hit: add `AddStatusOnHitEffect` to `DamageEffect.OnHitEffects` (target = each enemy actually hit).
- Toggle a status mid-skill: `AddStatusEffect` in window 1, `RemoveStatusEffect` in window 2 (e.g. invincibility on/off).

### Hit-Window Invincibility Toggle (recipe)
A `GenericSkill` with 2 hit windows (`animationPhase.hitWindows`) and no damage:
- Window 1: `maxTriggersPerPlayer = 1` → `windowEffects = [AddStatusEffect → InvincibilityStatus, Caster]`
- Window 2: `maxTriggersPerPlayer = 1` → `windowEffects = [RemoveStatusEffect → InvincibilityStatus, Caster]`
Invincibility is active from window 1's first frame until window 2 removes it. Set `InvincibilityStatus.Duration` to a positive safety cap if interruptions must not leave the actor permanently invincible.

---

## Risks

- **Stagger source ambiguity**: Both `Actor.ApplyDamageInstance()` (posture-based) and `Stagger` status can force stagger — overlap logic is [UNVERIFIED].
- **Runtime instancing**: `AddStatusEffect`/`AddStatusOnHitEffect` clone the status asset at runtime (`UnityEngine.Object.Instantiate`) so per-actor state (timers) doesn't leak between actors. Always apply via these effects, never by adding the raw asset.
- **`OnBeforeTakeDamage` ordering bug**: `Actor.ApplyDamageInstance()` fires `OnBeforeTakeDamage` AFTER `Calculate()` — damage-modifying listeners (including Block) don't affect the current hit. Tracked in `improvements.md` §2.7. Invincibility avoids this by early-returning before `Calculate()`.
- **singleInstance**: Slow/Burn/Invincibility are `singleInstance = true` — re-applying replaces the existing same-type status rather than stacking.
