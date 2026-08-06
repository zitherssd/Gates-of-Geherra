# Time Control System (Slowdown)

> **Last verified:** 2026-08-03

## Purpose

Provides cinematic time-dilation effects: a state-based pause (for action confirmation), a temporary slowdown (on hit), and a hold-to-resume mechanic.

---

## Responsibilities

- Modify `Time.timeScale` smoothly using `AnimationCurve` transitions.
- Maintain `Time.fixedDeltaTime` in sync with `timeScale`.
- Support four modes: Normal, StateSlowdown (0.05×), HoldResume (return to 1× while held), TemporarySlowdown (curve-based auto-expire).
- Block temporary slowdowns when state slowdown is active.

---

## Main Scripts

| Script | Role |
|--------|------|
| `SlowdownManager.cs` | Singleton; all `Time.timeScale` modifications |
| `SlowdownInputController.cs` | Maps player hold input to HoldResume mode |
| `SlowdownVisualEffect.cs` | Post-process visual change during slowdown (on `GlobalVolume`) |

---

## Slowdown Modes

| Mode | TimeScale | Trigger | Exit |
|------|-----------|---------|------|
| `Normal` | 1.0× | Default | N/A |
| `StateSlowdown` | ~0.05× | `EnterStateSlowdown()` | `ExitStateSlowdown()` |
| `HoldResume` | 1.0× (while held) | `StartHoldResume()` | `EndHoldResume()` |
| `TemporarySlowdown` | Curve 0.05→1.0 | `TriggerTemporarySlowdown(duration)` | Auto-expires |

---

## API

```csharp
SlowdownManager.instance.EnterStateSlowdown()          // Player uses action
SlowdownManager.instance.ExitStateSlowdown()           // Battle end, action interrupt
SlowdownManager.instance.TriggerTemporarySlowdown(t)   // On player hit (0.4s), battle end (2s)
SlowdownManager.instance.StartHoldResume()             // Hold button down
SlowdownManager.instance.EndHoldResume()               // Release button
```

---

## Usage Locations

| Call | Location |
|------|----------|
| `ExitStateSlowdown()` | `Actor.UseAction()` (player only), `BattleActiveState`, `BattleEndState`, `RestAreaManager.Enter()` |
| `TriggerTemporarySlowdown(0.4f)` | `Actor.ApplyDamageInstance()` when player is hit |
| `TriggerTemporarySlowdown(2f)` | `BattleActiveState` on enemy or player death |
| `EnterStateSlowdown()` | `SlowdownInputController` (hold input), `ActorStateMachine` [UNVERIFIED] |

---

## Events

| Event | Listeners |
|-------|-----------|
| `OnStateSlowdownStart` | `SlowdownVisualEffect` (presumed) |
| `OnStateSlowdownEnd` | `SlowdownVisualEffect` (presumed) |
| `OnTemporarySlowdownStart(duration)` | [UNVERIFIED] |
| `OnTemporarySlowdownEnd` | [UNVERIFIED] |

---

## External Dependencies

- `Time.timeScale` and `Time.fixedDeltaTime` (global Unity values)
- `SlowdownInputController` on the `Managers` object
- `Actor.isControllable` — only player actor triggers slowdown on action/hit

---

## Risks

- **`TAG.RECHARGE_DURING_SLOWDOWN`** actions use `Time.unscaledDeltaTime` — correctly exempt from slowdown, but other non-game timers must also use unscaled time where needed.
- **Competing slowdown requests**: If multiple systems call `TriggerTemporarySlowdown()` in the same frame, the last call wins (previous coroutine is cancelled via `CancelTemporarySlowdown()`).
- **State slowdown persists after scene transition**: Must explicitly call `ExitStateSlowdown()` before loading a new scene; several call sites already do this but not all paths are guaranteed.
