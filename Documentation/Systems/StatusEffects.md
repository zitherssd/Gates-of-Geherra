# Status Effect System

## Purpose

Applies time-based or event-based modifiers to actors during combat (poison, damage amplification, block bonuses, forced stagger, etc.).

---

## Responsibilities

- Track active status effects on each actor.
- Tick effects each frame via `StatusManager`.
- Apply status from actions via `AddStatusEffect` effect or `Stagger` component.
- Manage effect duration and removal.

---

## Main Scripts

| Script | Role |
|--------|------|
| `StatusManager.cs` | Per-actor MonoBehaviour; holds active statuses, ticks them |
| `BaseStatus.cs` | Abstract base for all status ScriptableObjects |
| `BaseEffect.cs` | [UNVERIFIED — may be related to status or separate] |
| `BlockStatus.cs` | Modifiers active during block stance |
| `DamageMultiplierStatus.cs` | Scales incoming or outgoing damage |
| `DoubleDamageStatus.cs` | 2× damage multiplier variant |
| `PoisonStatus.cs` | Applies periodic damage |
| `Poison.cs` | [UNVERIFIED — may be runtime state or separate poison implementation] |
| `Stagger.cs` | Likely forces actor into StaggerState |
| `ApplyStatusEffect.cs` | [UNVERIFIED — possibly action-wrapper to apply a status] |
| `AddStatusEffect.cs` | (`Actions/Effects/`) IEffect implementation — applies BaseStatus to caster or target |

---

## Known Status Assets (`Resources/`)

| Asset | Type |
|-------|------|
| `DamageMultiplierStatus.asset` | `DamageMultiplierStatus` |
| `PoisonStatus.asset` | `PoisonStatus` |

---

## Data Sources

- `BaseStatus` ScriptableObjects define effect parameters (damage rate, multiplier, duration, etc.)
- `AddStatusEffect` effect in `GenericSkill.OnHitEffects` → applies the status on hit

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

- Add a new status: create `BaseStatus` subclass, implement tick/apply logic, create asset.
- Apply status from an action: add `AddStatusEffect` to `GenericSkill.OnHitEffects`, reference the status asset.

---

## Risks

- **Stagger source ambiguity**: Both `Actor.ApplyDamageInstance()` (posture-based) and `Stagger` status can force stagger — overlap logic is [UNVERIFIED].
- **Status ScriptableObjects are shared references**: Multiple actors using the same status asset may share mutable state if the ScriptableObject stores runtime data. Verify status instancing.
- **`StatusManager` tick details [UNVERIFIED]**: Duration tracking, stack limits, and removal logic were not fully reviewed.
