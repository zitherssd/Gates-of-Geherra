# Gates of Gehera — Scene Registry

## Scene Index (Build Settings)

| Build Index | Path | Enabled | Purpose |
|-------------|------|---------|---------|
| 0 | `Assets/Scenes/TitleScene.unity` | Yes | Main menu / game entry |
| 1 | `Assets/Scenes/CaveScene.unity` | Yes | Primary game scene (rest + battle) |
| 2 | `Assets/Scenes/DebugScene.unity` | **No** | Developer debug / legacy |
| 3 | `Assets/Scenes/SandboxScene.unity` | Yes | Sandbox / free testing |
| 4 | `Assets/Scenes/ArenaScenes/Crossing.unity` | Yes | Dedicated arena battle |

---

## TitleScene

**Path**: `Assets/Scenes/TitleScene.unity`  
**Build Index**: 0

### Purpose
Main menu / game entry point. Title screen animation, new game / sandbox buttons.

### Entry Points
- Application launch.

### Exit Paths
- "Start" → `CaveScene` (new game).
- "Sandbox" → `SandboxScene`.
- [UNVERIFIED] legacy `DebugScene` via `StartFight()` in `MainMenuController`.

### Major GameObjects
[UNVERIFIED — scene not fully inspected via MCP]

### Managers Present
- `SaveManager` (DontDestroyOnLoad — persists from this scene forward).

### Key Components
- `MainMenuController` — title animation (LeanTween), button wiring.
- `MainMenuSlot` — [UNVERIFIED purpose].

### Dependencies
- `SaveManager` must be present.
- `CaveScene` name must be valid in Build Settings.

---

## CaveScene

**Path**: `Assets/Scenes/CaveScene.unity`  
**Build Index**: 1

### Purpose
The primary game scene. Hosts both the **rest area** (between battles) and **in-scene battles** (legacy flow). Also serves as the default scene when no dedicated arena is configured.

### Entry Points
- Loaded from `TitleScene` on new game.
- Loaded from `ArenaBootstrapper` (ReturnScene hand-off) after a dedicated arena battle.

### Exit Paths
- Player death → `TitleScene`.
- Floor progression with `arena != Arena.None` on a `BattleDefinition` → dedicated arena scene (e.g. `Crossing`).
- Legacy: battles play within the scene itself using `Level_*` sub-objects.

### Root GameObjects (16 total)

| Name | Components | Notes |
|------|-----------|-------|
| `Main Camera` | `CameraManager`, `CameraOcclusionManager`, `Camera`, `AudioListener`, `AudioSource` | Camera tracking with enemy weighting |
| `EventSystem` | `EventSystem`, `InputSystemUIInputModule` | Unity Input System UI events |
| `Battle UI` | `UIManager`, `Canvas`, `CanvasHandler` | Action buttons, FadeImage, middle text, FPS counter |
| `RestPosition` | `Transform` | Rest-area spawn anchor |
| `Skills UI` | `Canvas` | Skill/inventory view canvas |
| `Resting UI` | `Canvas`, `CanvasGroup` | Descend, Skills, Items, Train, Explore buttons |
| `Choose Skills UI` | `Canvas`, `GridLayoutGroup` | Post-battle skill selection cards |
| `Directional Light` | `Light`, `DirectionalLightController` | Scene lighting |
| `Level_Original` | `Transform` (43 children) | Legacy battle arena geometry |
| `Level_Hall` | `Transform` (10 children) | Legacy battle arena geometry |
| `Level_Resting` | `Transform` (4 children) | Rest area environment |
| `Managers` | — | Parent for all scene managers (5 children) |
| `PlayerBattler` | `Actor`, `ActorStateMachine`, `StatusManager`, `NavMeshAgent`, `Animator`, `CapsuleCollider`, `Rigidbody` | The pre-placed player body |
| `NavMesh Surface` | `NavMeshSurface` | Nav mesh for AI movement |
| `ROOT UI` | `Canvas` | Tooltip + Prompt overlays |
| `GlobalVolume` | `Volume`, `SlowdownVisualEffect` | Post-processing + slowdown visual |

### Managers Sub-hierarchy

| Name | Components |
|------|-----------|
| `BattleManager` | `BattleManager` |
| `CrawlerManager` | `SkillGenerator`, `FloorManager`, `CrawlerManager` |
| `RestAreaManager` | `RestAreaManager` |
| `TrainingManager` | `TrainingManager` |
| `VirtualJoystick` | `JoystickManager` |

Parent `Managers` object also has: `UIManager`, `SoundManager`, `GameFlowManager`, `SlowdownManager`, `SlowdownInputController`.

### Battle UI Sub-hierarchy

| Name | Components |
|------|-----------|
| `ActionContainers` | `Canvas`, `CanvasGroup` |
| └ `LeftContainer` | `DropSlot`, `GridLayoutGroup` |
| └ `LeftContainerSecondary` | `DropSlot`, `GridLayoutGroup` |
| └ `RightContainer` | `DropSlot`, `GridLayoutGroup` |
| └ `RightContainerSecondary` | `DropSlot`, `GridLayoutGroup` |
| └ `VirtualJoystickBase` | `Image` |
| `SlowdownMeter` | `Slider` (inactive) |
| `FadeImage` | `Image` (screen fade) |
| `Middle Text` | `TextMeshProUGUI` |
| `FPS` | `TextMeshProUGUI`, `FrameRateManager` |

### Resting UI Sub-hierarchy

| Name | Components |
|------|-----------|
| `DescendButton` | `Button` → calls `FloorManager.ProgressToNextFloor` |
| `SkillsButton` | `Button` → opens skill inventory |
| `InfoPanel` | `StatPanelUI` → displays player stats |
| `ItemsButton` | `Button` → opens item inventory |
| `TrainButton` | `Button` → calls `TrainingManager.TriggerTraining` |
| `ExploreButton` | `Button` → calls `FloorManager.QuickFight` |

### Persistent Objects
- None — `GameSession` and `SaveManager` are DontDestroyOnLoad from TitleScene. CaveScene-local managers are NOT persistent.

### Scene-Specific Systems
- `GameFlowManager` — handles player spawning, mode switching (RestArea ↔ Battle), and save loading.
- `FloorManager` — manages story/random battle sequencing.
- `SkillGenerator` — post-battle skill card UI and reward draw.

---

## DebugScene

**Path**: `Assets/Scenes/DebugScene.unity`  
**Build Index**: 2 (disabled in build)

### Purpose
Developer testing scene. Disabled from production builds.

### Entry Points
- `MainMenuController.StartFight()` (not wired to any active button [UNVERIFIED]).

[UNVERIFIED — scene not inspected; disabled in build settings]

---

## SandboxScene

**Path**: `Assets/Scenes/SandboxScene.unity`  
**Build Index**: 3

### Purpose
Free-form testing sandbox. No floor/story constraints.

### Entry Points
- `MainMenuController.StartSandbox()` → `SandboxScene`.

[UNVERIFIED — scene not inspected via MCP]

---

## ArenaScenes/Crossing

**Path**: `Assets/Scenes/ArenaScenes/Crossing.unity`  
**Build Index**: 4

### Purpose
Dedicated arena battle scene for one or more `BattleDefinition` assets that set `arena = Arena.Crossing`.

### Entry Points
- `GameFlowManager.EnterBattle()` → loads this scene when `BattleDefinition.arena == Arena.Crossing`.
- `ArenaBootstrapper` bootstraps the battle on `Start()`.

### Exit Paths
- Victory → `ReturnScene` (stored on `GameSession`) reloaded.
- Death → `TitleScene`.

### Major GameObjects
[UNVERIFIED — not fully inspected; scene presumed to have `ArenaBootstrapper`, `BattleManager`, `SpawnGroup`, Camera, lights]

### Key Components
- `ArenaBootstrapper` — spawns player body, binds to `GameSession.PlayerRuntime`, starts battle.
- `SpawnGroup` — defines `playerSpawn`, `enemySpawns[]`, `cameraAnchor`.

### Dependencies
- `GameSession.PendingBattle` must be set before scene load.
- `GameSession.ReturnScene` must be set to return correctly.
- Player prefab must be assigned on `ArenaBootstrapper`.
