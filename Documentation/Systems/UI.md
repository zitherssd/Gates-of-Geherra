# UI System

> **Last verified:** 2026-08-03

## Purpose

Manages all user-facing interfaces: combat action buttons (drag-and-drop), battle feedback, menus, fades, skill selection, and tooltip/prompt overlays.

---

## Responsibilities

- Spawn, position, and manage draggable action buttons from the player's action inventory.
- Route button taps to `Actor.UseAction()`.
- Control which UI panels are active during Rest vs Battle modes.
- Present skill selection cards post-battle.
- Display damage/posture popups.
- Manage screen fade transitions.
- Expose stat info panel.
- Provide tooltip and prompt overlays.

---

## Main Scripts

| Script | Role |
|--------|------|
| `UIManager.cs` | Central controller; singleton |
| `ActionButtonBattle.cs` | Action button in combat — taps execute the action |
| `ActionButtonInventory.cs` | Action button in inventory/skill view — used for arrangement only |
| `ActionButtonHandler.cs` | Shared drag/drop logic for action buttons |
| `ActionCardHandler.cs` | Post-battle skill selection card |
| `ActionInventoryHandler.cs` | Inventory view list management |
| `ActionUIManager.cs` | [UNVERIFIED — additional action UI logic] |
| `DropSlot.cs` | Target container for drag-and-drop |
| `JoystickManager.cs` | Virtual on-screen joystick |
| `StatPanelUI.cs` | Displays player stats (STR, AGI, MND, SPT, HP, etc.) |
| `TargetingManager.cs` | [UNVERIFIED — possibly UI targeting indicator] |
| `DamagePopup.cs` | (`Utility/`) Floating damage numbers |
| `HpBar.cs` / `HpBarHandler.cs` | (`Utility/`) HP bar display |
| `CanvasHandler.cs` | (`Utility/`) Canvas utility helper |

---

## UI Layout (CaveScene)

```
Battle UI (Canvas — UIManager)
├── ActionContainers (Canvas, CanvasGroup)
│   ├── LeftContainer (DropSlot, GridLayoutGroup)      ← Left main actions
│   ├── LeftContainerSecondary (DropSlot)               ← Left alt actions
│   ├── RightContainer (DropSlot, GridLayoutGroup)     ← Right main actions
│   ├── RightContainerSecondary (DropSlot)              ← Right alt actions
│   └── VirtualJoystickBase (Image)
├── SlowdownMeter (Slider — inactive)
├── FadeImage (Image — screen fade)
├── Middle Text (TextMeshProUGUI)
└── FPS (TextMeshProUGUI, FrameRateManager)

Resting UI (Canvas, CanvasGroup)
├── DescendButton → FloorManager.ProgressToNextFloor
├── SkillsButton  → opens Skills UI
├── InfoPanel     → StatPanelUI
├── ItemsButton   → opens item inventory
├── TrainButton   → TrainingManager.TriggerTraining
└── ExploreButton → FloorManager.QuickFight

Skills UI (Canvas)
└── SkillsInventoryViewport

Choose Skills UI (Canvas, GridLayoutGroup)
└── [SkillCard prefab instances — spawned at runtime]

ROOT UI (Canvas)
├── Tooltip (TooltipUI, CanvasGroup)
└── Prompt (CanvasGroup)
```

---

## Action Button System

### Initialization
`UIManager.InitializePlayerActionButtonPrefabs(actions, loadout?)`:
1. Destroy existing `PlayerActions` GameObjects.
2. For each action in `ActorRuntime.actions`: Instantiate `buttonPrefab` (ActionButton).
3. If `loadout` provided: place each button in `ContainerID` at `SlotIndex`.
4. Else if existing layout present: re-capture and re-apply via `GetCurrentLoadout()`.
5. Register buttons in `PlayerActions` list.

### Execution (Battle)
- `ActionButtonBattle` on tap → reads `ActionButtonHandler.ReferencedAction`
- Validates: cooldown, uses, stamina, buildup
- Calls `BattleManager.Player.UseAction(action)`

### Drag-and-Drop
- `ActionButtonHandler` (or `DragMove`) handles drag gesture.
- `DropSlot` accepts drop and reparents button.
- Layout saved to `GameSession.Loadout` on battle-end and at save.

---

## Data Sources

- `ActorRuntime.actions` — source of all action buttons
- `GameSession.Loadout : List<ActionSlotSaveData>` — persisted slot arrangement
- `UIManager.ActionsHolder`, `LeftSecondaryContainer`, `SkillHolder`, `RightSecondaryContainer` — container references

---

## Events

| Event | Source |
|-------|--------|
| `UIManager.OnHideUI` | `UIManager.HideUI()` |
| `UIManager.OnShowUI` | `UIManager.ShowUI()` |

---

## External Dependencies

- `BattleManager.Player` — target actor for action execution
- `GameSession.Loadout` — persistent layout source
- `SlowdownManager` — read by `SlowdownInputController`
- `SoundManager` — audio on button tap [UNVERIFIED]

---

## Extension Points

- Add a new UI panel: create Canvas child in scene, wire show/hide to `UIManager`.
- Add a new action container: add new `DropSlot` to `ActionContainers`, register in `UIManager`.
- Change button layout: modify `UIManager.InitializePlayerActionButtonPrefabs()`.

---

## Risks

- **`BattleManager.Player` null check**: Action buttons assume the player is valid; if no player is bound (no run), clicking a button throws. Prefer `BattleManager.Player` over `PlayerActors[0]`.
- **UIManager is scene-local**: It is NOT DontDestroyOnLoad. Arena scenes must have their own UIManager instance, or actions from CaveScene's UIManager reference won't be available.
- **Loadout rebuild destroys all buttons**: `InitializePlayerActionButtonPrefabs()` destroys all existing buttons — do not call this mid-battle.
