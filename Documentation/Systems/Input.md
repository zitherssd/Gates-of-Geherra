# Input System

## Purpose

Handles player input (touch / on-screen controls) and routes it to UI action buttons and camera controls.

---

## Responsibilities

- Detect touch press, release, hold, and swipe gestures.
- Provide an on-screen virtual joystick for movement.
- Connect input to `ActionButtonBattle` for skill execution.
- Interface with `SlowdownManager` via `SlowdownInputController`.

---

## Main Scripts

| Script | Role |
|--------|------|
| `InputHandler.cs` | Gesture detection (swipe/click/hold) via Unity Input System |
| `JoystickManager.cs` | Manages virtual joystick state |
| `ModifiedOnScreenStick.cs` | Extended `OnScreenStick` with project-specific modifications |
| `SlowdownInputController.cs` | Maps hold input to state slowdown enter/exit |
| `ActionButtonBattle.cs` | Receives tap input, resolves action, calls `Actor.UseAction()` |
| `ActionButtonHandler.cs` | Shared drag detection across action button types |
| `DragMove.cs` | Drag gesture handler for moving UI elements |

---

## Input Architecture

```
Unity Input System (InputActionReference)
  ├── touchPressed → InputHandler.PressBegin()
  ├── touchEnd     → InputHandler.PressEnd() → OnSwipe or OnClick
  ├── touchHold    → InputHandler.PressHold() → OnHold
  └── touchPosition → current pointer position

OnScreenStick (VirtualJoystickBase)
  → ModifiedOnScreenStick
  → drives movement direction for MoveAction / player movement

ActionButton (screen tap)
  → ActionButtonBattle.OnPointerDown/Up
  → resolves BaseAction from ActionButtonHandler.ReferencedAction
  → calls BattleManager.PlayerActors[0].UseAction(action)

Hold Gesture
  → SlowdownInputController
  → SlowdownManager.StartHoldResume() / EndHoldResume()
  → exits state slowdown while held
```

---

## Data Sources

- `InputActionReference` assets — set per field on `InputHandler` in Inspector
- `ActionButtonHandler.ReferencedAction` — the BaseAction tied to this button

---

## Events

| Event | Source | Consumers |
|-------|--------|-----------|
| `InputHandler.OnSwipe(delta)` | Pointer drag > 50px | [UNVERIFIED consumers] |
| `InputHandler.OnClick(pos)` | Short press | [UNVERIFIED consumers] |
| `InputHandler.OnHold(pos)` | Hold input action | [UNVERIFIED consumers] |

---

## External Dependencies

- `BattleManager.PlayerActors[0]` — action execution target
- `SlowdownManager` — hold-to-resume slowdown

---

## Risks

- **Touch-only architecture**: `InputHandler` uses `touchPressed`, `touchEnd`, etc. — keyboard/gamepad not handled for gameplay actions. [UNVERIFIED if Input System actions have fallback bindings]
- **Debug logs on all events**: `InputHandler.Start()` adds Debug.Log listeners to all events — these remain in release builds.
- **`OnPointerUp()` is empty**: `InputHandler.OnPointerUp()` has commented-out code — may indicate incomplete implementation.
