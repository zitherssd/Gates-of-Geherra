# Gates of Gehera — Project Registry

> Consolidated reference for scripts, scenes, and prefabs in the project.
> **Last verified:** 2026-08-03

---

# 1. Script Registry

## Namespace Map

| Namespace | Location | Purpose |
|-----------|----------|---------|
| `Assets.Scripts.Game` | `Scripts/Game/` | Game flow, session, rest area, training, arena bootstrapping |
| `Assets.Scripts.Battle.Actor` | `Scripts/Battle/Actor/` | Actor, runtime, definition, state machine, inventory, AI |
| `Assets.Scripts.Battle.Actor.States` | `Scripts/Battle/Actor/States/` | Actor states (Idle, Acting, Stagger, Death, etc.) |
| `Assets.Scripts.Battle.Actor.AI` | `Scripts/Battle/Actor/AI/` | Behavior tree AI system |
| `Assets.Scripts.Battle.Actor.Systems` | `Scripts/Battle/Actor/Systems/` | MovementSystem, TargetingSystem |
| `Assets.Scripts.Battle.Manager` | `Scripts/Battle/Manager/` | BattleManager, BattleStateMachine, Arena, SpawnGroup |
| `Assets.Scripts.Battle.Manager.States` | `Scripts/Battle/Manager/States/` | BattleStartState, BattleActiveState, BattleEndState |
| `Assets.Scripts.Battle.Actions` | `Scripts/Battle/Actions/` | BaseAction, BaseSkill, DamageInstance, MoveData, RewardPool, WorldActionTrigger |
| `Assets.Scripts.Battle.Actions.Skills` | `Scripts/Battle/Actions/Skills/` | Concrete actions (AttackSkill, GenericSkill, Dodge, Block, Jump, MoveAction, ...) |
| `Assets.Scripts.Battle.Actions.Effects` | `Scripts/Battle/Actions/Effects/` | IEffect + implementations |
| `Assets.Scripts.Battle.Actions.HitWindows` | `Scripts/Battle/Actions/HitWindows/` | HitWindow, HitWindowManager |
| `Assets.Scripts.Battle.Status` | `Scripts/Battle/Status/` | BaseStatus, StatusManager, Poison, Stagger, BlockStatus, ... |
| `Assets.Scripts.Battle.Components.Effects` | `Scripts/Battle/Components/Effects/` | EffectManager, EffectsRepository |
| `Assets.Scripts.Battle.Components.Audio` | `Scripts/Battle/Components/Audio/` | AudioManager |
| `Assets.Scripts.Battle.Timelock` | `Scripts/Battle/Timelock/` | CooldownDisplay (namespaced); `TimeLock` + `TimeLockManager` are global-namespace |
| `Assets.Scripts.Battle.Items` | `Scripts/Battle/Items/` | BaseItem, BaseConsumable, BaseTrinket, CraftingSystem |
| `Assets.Scripts.Crawler` | `Scripts/Crawler/` | FloorManager, SkillGenerator, CrawlerManager (stub) |
| `Assets.Scripts.Save` | `Scripts/Save/` | SaveManager, SaveData, ActorSaveData |
| `Assets.Scripts.Core` | `Scripts/Core/` | StateMachine, IState, StaticHelpers, Intersections, ActionSlot, RhombusMeshCollider |
| `Assets.Scripts.UI` | `Scripts/UI/` | UIManager, ActionButton*, ActionCardHandler, ... |
| `Assets.Scripts.UI.Title` | `Scripts/UI/Title/` | MainMenuController, MainMenuSlot |
| `Assets.Scripts.Utility` | `Scripts/Utility/` (Camera/, Feedback/, Input/, Audio/, Misc/) | Cameras, slowdown, sound, input, misc helpers (single namespace) |

> **Assemblies**: `GoG.Runtime` (all of `Scripts/` except `Editor/`), `GoG.Editor` (`Scripts/Editor/`, Editor-only), `LeanTween.Runtime` / `LeanTween.Editor` (`ThirdParty/LeanTween/`). Game code no longer compiles into `Assembly-CSharp`.
>
> **Namespace exceptions** (files in the **global namespace**, not `Assets.Scripts.*`): `ActionDatabase` (`Utility/Misc/ActionDatabase.cs`), `TimeLock` + `TimeLockManager` (`Battle/Timelock/`), `ItemEventBus` (`Battle/Items/ItemEventBus.cs`), `MainMenuController` (`UI/Title/`), and all `Editor/*.cs`. `UIManager` (`Scripts/UI/UIManager.cs`) lives in the root `Assets.Scripts` namespace (not `.UI`). `TargetingManager.cs` (`Scripts/UI/`) is entirely commented out.

---

## Core Game Flow

### GameSession
**File**: `Scripts/Game/GameSession.cs` — Persistent, data-only source of truth for the entire run. DontDestroyOnLoad.  
**Key API**: `Instance`, `StartNewRun(name)`, `AdoptPlayerRuntime(runtime)`, `ClearRun()`, `NotifyPlayerSpawned(player)`  
**Key Fields**: `PlayerRuntime`, `currentFloor`, `trainingsDone`, `trainingsDoneThisFloor`, `timelocks`, `PendingBattle`, `ReturnScene`, `Loadout`  
**Events**: `OnPlayerSpawned : Action<Actor>`

### GameFlowManager
**File**: `Scripts/Game/GameFlowManager.cs` — Per-scene coordinator for CaveScene. Handles player body spawn, save load, mode switching.  
**Key API**: `SetMode(GameMode)`, `EnterBattle(BattleDefinition, onBattleEnd)`  
**Dependencies**: `GameSession`, `SaveManager`, `BattleManager`, `UIManager`, `RestAreaManager`

### ArenaBootstrapper
**File**: `Scripts/Game/ArenaBootstrapper.cs` — Entry point for dedicated arena scenes. Spawns+binds player, starts battle.  
**Fields**: `playerPrefab`, `debugBattle`

---

## Actor System

### Actor
**File**: `Scripts/Battle/Actor/Actor.cs` — Primary MonoBehaviour for any fighter. Coordinates all sub-systems.  
**Events**: `OnBeforeTakeDamage`, `OnAfterTakeDamage`, `OnBeforeDealDamage`, `OnAfterDealDamage`, `OnActionUsed`, `OnReset`  
**Key Methods**: `UseAction()`, `ApplyDamageInstance()`, `DealDamage()`, `Bind(runtime)`, `SetDefinition(def)`, `Spawn()`, `Init()`  
**Sub-components**: `EffectManager effects`, `TargetingSystem target`, `MovementSystem movement`, `ActorInventory inventory`, `AIBT ai`

### ActorRuntime
**File**: `Scripts/Battle/Actor/ActorRuntime.cs` — Serializable runtime data model. Survives scene loads on GameSession.  
**Fields**: `Name`, `hpBars`, `Strength`, `Agility`, `Mind`, `Spirit`, `actions`, `items`, `currentBuildup/Posture/Stamina`  
**Computed**: `maxBuildup = base + Mind`, `maxPosture = base + Strength`, `maxStamina = base + Agility×2`

### ActorDefinition
**File**: `Scripts/Battle/Actor/ActorDefinition.cs` — ScriptableObject template. `CreateAssetMenu: ScriptableObjects/Actor`.  
**Key Assets**: `Resources/Actors/MC.asset`, `Old Prisoner.asset`, `Enraged Maniac.asset`, `Hulk.asset`, `Shuriken Thrower.asset`, `Boxer.asset`, `Malnourished Individual.asset`, `Tutorial Guy.asset`

### ActorStateMachine
**File**: `Scripts/Battle/Actor/ActorStateMachine.cs` — State transitions + animation event entry points.  
**Animation Events**: `EnterWindup(frames)`, `OnHit()`, `EnterRecovery()`, `OnEnd()`  
**States**: `IdleState`, `InactiveState`, `FumbleState`, `AirStaggerState`, `AirNeutralState`, `LandingState`, `ActingState`, `RollState`, `BlockState`, `MoveState`, `GettingUpState`, `StaggerState`, `DeathState`

---

## Battle Management

### BattleManager
**File**: `Scripts/Battle/Manager/BattleManager.cs` — Orchestrates a single battle. Spawns enemies, positions player.  
**API**: `Enter(BattleDefinition, onBattleEnd)`, `TriggerBattleEnd()`  
**Property**: `Player` (computed — resolves to `PlayerActors[0]`, falling back to `GameFlowManager.playerActor`).  
**Events**: `OnBattleEnd`, `OnNewTurn`  
**Note**: Sets `Physics.gravity = (0, -6, 0)` in Start().

### BattleStateMachine / States
**Files**: `Scripts/Battle/Manager/BattleStateMachine.cs`, `States/`  
**States**: `BattleStartState` (enables UI, fires events), `BattleActiveState` (monitors all-dead), `BattleEndState` (awards, saves, transitions)

---

## Action System

### BaseAction
**File**: `Scripts/Battle/Actions/BaseAction.cs` — Abstract SO base for all playable actions.  
**Key Fields**: `guid`, `Type` (BUTTONTYPE), `Name`, `Rarity`, `CooldownTimer`, `TotalUses`, `BuildupCost/Gain`, `StaminaCost`, `Tags`, `StickMult`  
**Lifecycle**: `Begin(actor)` → `PerformSpecific(actor, onEnd)` → `EndAction(reason)` → `Cleanup(reason)`

### BaseSkill (extends BaseAction)
**File**: `Scripts/Battle/Actions/BaseSkill.cs` — Adds animation hooks: `OnHit()`, `OnUpdate(dt)`, `OnEnterWindup(animator)`, `OnEnterRecovery(animator)`

### Concrete Action Types
| Type | File | Behavior |
|------|------|----------|
| `AttackSkill` | `Actions/AttackSkill.cs` | Hitbox-based melee |
| `GenericSkill` | `Actions/GenericSkill.cs` | Effect-list driven (IEffect), uses HitWindows |
| `ProjectileAttack` | `Actions/ProjectileAttack.cs` | Spawns projectile prefab |
| `Dodge` | `Actions/Dodge.cs` | Force-based evasion |
| `Block` | `Actions/Block.cs` | Enters BlockState |
| `Charge` | `Actions/Charge.cs` | Directional lunge |
| `Jump` | `Actions/Jump.cs` | Air state transition |
| `MoveAction` | `Actions/MoveAction.cs` | Directional movement |

### HitWindow System
**Files**: `HitWindows/HitWindow.cs` (struct), `HitWindows/HitWindowManager.cs` (class)  
Frame-based hit windows with per-enemy hit limits. Used by `GenericSkill` in `OnUpdate()`.

### Action Assets (Resources/Actions)
| Asset | Type | Notes |
|-------|------|-------|
| `Weave` (`Defensive/Weave.asset`) | GenericSkill (INSTANT) | 2 rechargeable charges (~7s each), Ninjutsu animation. Iframes frames 2–8 via `AddInvincible` (frame-2 window; auto-removes on cleanup) + `RemoveStatusEffect` (frame-9 window) on InvincibilityStatus; camera-away hop via `AddForceAwayCamera`. Added to `MC.asset` baseActions (player starting kit). |
| `Hulk_Slam` (`AI/Hulk_Slam.asset`) | GenericSkill | Enemy (Hulk) — sluggish (windup 0.3×), wide hitbox ±0.5 × z 0.3–1.2, Punch anim, hit 11–14, Range 1.3, Posture 6, knockback AWAY 0.8 |
| `Hulk_Kick` (`AI/Hulk_Kick.asset`) | GenericSkill | Enemy (Hulk) — quick (windup 1.3×), ForwardKick anim, hit 16–19, Range 1.1, Posture 12, knockback UP_AND_AWAY 3/3 |

### Effects (Actions/Effects/)
`AddRandomForce` (`IEffect`, in `AddForceDirection.cs`) — applies a small force in a random horizontal direction. Used for INSTANT actions, where `Direction` is never set (so `AddForceDirection` is a no-op).
`AddForceAwayCamera` (`IEffect`, in `AddForceDirection.cs`) — applies a force in the camera's forward direction (projected to the horizontal plane), i.e. away from the camera. Used for INSTANT actions, where `Direction` is never set. Example: Weave's dodge hop.
`AddInvincible` (`IEffect + IEndableEffect`, in `AddStatusEffect.cs`) — grants `InvincibilityStatus` to the caster; auto-removed on action cleanup (safety net). Pair with `RemoveStatusEffect` in a later hit window for a precise frame window (Weave frames 2–8).

---

## AI System

### AIBT
**File**: `Scripts/Battle/Actor/AI/AIBT.cs` — Behavior tree controller, ticks highest-priority valid behavior each frame.  
**Rulesets** (`AiRuleset` enum in `AIBT.cs`): `DEFAULT` (passive/SandboxGuy), `OldMan` (defensive), `Maniac` (aggressive), `Hungry` (maps to `OldManBehavior`), `ShurkienThrower` (ranged), `TacticalFlanker` (tactical), `SmartApproach` (crowd-aware surround), `Hulk` (approach + attack only, no defense), `Ninja` (empty — non-functional). Note: internal behavior-list variable names differ from enum members (e.g. `SandboxGuy`, `EngragedManiac`, `ShurkienThrowerBehavior`).

### AI Behaviors
`ApproachBehavior`, `SmartApproachBehavior`, `FlankApproachBehavior` (+ `CircleApproachBehavior`, defined in the same file), `GroupFlankBehavior`, `MoveToOrbitBehavior`, `MoveBehavior`, `DashBehavior`, `DodgeBehavior`, `AttackWithValidSkill`, `AttackProjectile`, `BlockBehavior`, `BlockCancelBehavior`, `ReactionBehavior`

### AI Conditions
`DistanceConditions`, `GlobalAttackTokenCondition`, `HesitateCondition`, `InsideEnemyHitbox`, `StaminaConditions`

---

## Save System

| Script | File | Role |
|--------|------|------|
| `SaveManager` | `Scripts/Save/SaveManager.cs` | Disk persistence (JSON / PlayerPrefs on WebGL). DontDestroyOnLoad. |
| `SaveData` | `Scripts/Save/SaveData.cs` | Root serializable container |
| `ActorSaveData` | `Scripts/Save/ActorSaveData.cs` | Actor state serializer. **Note**: `CON` stores Agility, `AGI` stores Mind (legacy naming bug). |
| `ActionSlotSaveData` | `Scripts/Save/ActionSlotSaveData.cs` | (ContainerID, SlotIndex, ActionGuid) |
| `ActionDatabase` | `Scripts/Utility/Misc/ActionDatabase.cs` | GUID→BaseAction lookup. Asset at `Resources/Database/Action Database.asset`. |

---

## UI System

### UIManager
**File**: `Scripts/UI/UIManager.cs` — Central UI controller. Scene-local singleton.  
**API**: `InitializePlayerActionButtonPrefabs()`, `EnableUI/DisableUI`, `ShowUI/HideUI`, `Fade()`, `GetCurrentLoadout()`  
**Events**: `OnHideUI`, `OnShowUI`

### UI Scripts
| Script | Role |
|--------|------|
| `ActionButtonBattle.cs` | In-battle action execution button |
| `ActionButtonInventory.cs` | Inventory action browsing button |
| `ActionButtonHandler.cs` | Shared drag/drop logic |
| `ActionCardHandler.cs` | Post-battle skill selection card |
| `DropSlot.cs` | Drag-and-drop container |
| `JoystickManager.cs` | Virtual on-screen joystick |
| `StatPanelUI.cs` | Player stat display |
| `DamagePopup.cs` | Floating damage numbers |
| `HpBar.cs` / `HpBarHandler.cs` | World-space HP bar |

---

## Utility Scripts

| Script | File | Role |
|--------|------|------|
| `CameraManager` | `Utility/Camera/CameraManager.cs` | Dynamic camera, shake, slow-track |
| `CameraOcclusionManager` | `Utility/Camera/CameraOcclusionManager.cs` | Makes occluding objects transparent |
| `SlowdownManager` | `Utility/Feedback/SlowdownManager.cs` | TimeScale manipulation (4 modes) |
| `SlowdownVisualEffect` | `Utility/Feedback/SlowdownVisualEffect.cs` | Post-process during slowdown |
| `SoundManager` | `Utility/Audio/SoundManager.cs` | Music + SFX playback |
| `InputHandler` | `Utility/Input/InputHandler.cs` | Touch gesture detection |
| `StaticHelpers` | `Core/StaticHelpers.cs` | Math utilities (LinearMap) |
| `TimeLockManager` | `Battle/Timelock/TimelockManager.cs` | Real-time named timers (global namespace) |

---

## Item System

| Script | File | Role |
|--------|------|------|
| `BaseItem` | `Items/BaseItem.cs` | Base SO: ItemName, Description, Icon, stackable |
| `BaseConsumable` | `Items/BaseConsumable.cs` | Adds `List<IItemEffect> Effects` |
| `BaseTrinket` | `Items/BaseTrinket.cs` | Auto-subscribes effects to ItemEventBus on equip |
| `ItemEventBus` | `Items/ItemEventBus.cs` | Static event bus. Triggers: OnNewBattle, OnNewFloor, OnDamageTaken |
| `ActorInventory` | `Actor/ActorInventory.cs` | Manages equip/unequip lifecycle |

---

## Status Effects

| Script | File | Role |
|--------|------|------|
| `StatusManager` | `Status/StatusManager.cs` | Per-actor status ticker (owner wiring, singleInstance, HasStatus/GetStatus helpers) |
| `BaseStatus` | `Status/BaseStatus.cs` | Abstract SO base (unified `Duration` auto-removal via `TickStatus`) |
| `PoisonStatus` | `Status/PoisonStatus.cs` | Damage over time |
| `BurnStatus` | `Status/BurnStatus.cs` | Fire damage over time |
| `SlowStatus` | `Status/SlowStatus.cs` | Movement speed reduction (`MovementSystem.MoveSpeedMultiplier`) |
| `InvincibilityStatus` | `Status/InvincibilityStatus.cs` | Full damage immunity (`Actor.IsInvincible`) |
| `BlockStatus` | `Status/BlockStatus.cs` | Block stance modifier |
| `DamageMultiplierStatus` | `Status/DamageMultiplierStatus.cs` | Damage scaling |
| `DoubleDamageStatus` | `Status/DoubleDamageStatus.cs` | 2× damage |
| `Stagger` | `Status/Stagger.cs` | Force stagger state |

**Status-related effects** (`Actions/Effects/`):
- `AddStatusEffect` (`IEffect`) — applies a status to the caster or closest enemy (window effects / buffs-debuffs)
- `RemoveStatusEffect` (`IEffect`) — removes an active status by type (e.g. turn invincibility OFF in a later hit window)
- `IOnHitEffect` (`interface`) — target-centric hook evaluated per enemy hit by `DamageEffect.OnHitEffects`
- `AddStatusOnHitEffect` (`IOnHitEffect`) — applies a status to each enemy actually hit (1vN safe)

---

# 2. Scene Registry

## Scene Index (Build Settings)

| Build Index | Path | Enabled | Purpose |
|-------------|------|---------|---------|
| 0 | `Assets/Scenes/TitleScene.unity` | Yes | Main menu / game entry |
| 1 | `Assets/Scenes/CaveScene.unity` | Yes | Primary game scene (rest + battle) |
| 2 | `Assets/Scenes/DebugScene.unity` | **No** | Developer debug / legacy |
| 3 | `Assets/Scenes/SandboxScene.unity` | Yes | Sandbox / free testing |
| 4 | `Assets/Scenes/ArenaScenes/Crossing.unity` | Yes | Dedicated arena battle |
| 5 | `Assets/Scenes/ArenaScenes/Cave.unity` | Yes | Dedicated arena battle (`Arena.Cave`) |

---

## TitleScene (Build 0)

**Entry**: Application launch.  
**Exit**: "Start" → CaveScene, "Sandbox" → SandboxScene.  
**Managers**: `SaveManager` (DDOL from this point).  
**Components**: `MainMenuController` (LeanTween title, button wiring), `MainMenuSlot`.  
**Dependencies**: `SaveManager` must be present, `CaveScene` in Build Settings.

---

## CaveScene (Build 1)

**Purpose**: Primary game scene — rest area hub + legacy in-scene battles.  
**Entry**: From TitleScene (new game) or ArenaBootstrapper (ReturnScene hand-off).  
**Exit**: Death → TitleScene; floor progression with arena → dedicated arena scene.

### Root GameObjects (16 total)

| Name | Components |
|------|-----------|
| `Main Camera` | `CameraManager`, `CameraOcclusionManager`, `Camera`, `AudioListener`, `AudioSource` |
| `EventSystem` | `EventSystem`, `InputSystemUIInputModule` |
| `Battle UI` | `UIManager`, `Canvas`, `CanvasHandler` |
| `RestPosition` | `Transform` (rest-area spawn anchor) |
| `Skills UI` | `Canvas` |
| `Resting UI` | `Canvas`, `CanvasGroup` (Descend/Skills/Items/Train/Explore buttons + InfoPanel) |
| `Choose Skills UI` | `Canvas`, `GridLayoutGroup` |
| `Directional Light` | `Light`, `DirectionalLightController` |
| `Level_Original` | `Transform` (43 children — legacy arena) |
| `Level_Hall` | `Transform` (10 children — legacy arena) |
| `Level_Resting` | `Transform` (4 children — rest environment) |
| `Managers` | Parent for scene managers (see below) |
| `PlayerBattler` | `Actor`, `ActorStateMachine`, `StatusManager`, `NavMeshAgent`, `Animator`, `CapsuleCollider`, `Rigidbody` |
| `NavMesh Surface` | `NavMeshSurface` |
| `ROOT UI` | `Canvas` (Tooltip + Prompt overlays) |
| `GlobalVolume` | `Volume`, `SlowdownVisualEffect` |

### Managers Sub-hierarchy

| Name | Components |
|------|-----------|
| `BattleManager` | `BattleManager` |
| `CrawlerManager` | `SkillGenerator`, `FloorManager`, `CrawlerManager` (stub) |
| `RestAreaManager` | `RestAreaManager` |
| `TrainingManager` | `TrainingManager` |
| `VirtualJoystick` | `JoystickManager` |

Also on the parent `Managers` object: `UIManager`, `SoundManager`, `GameFlowManager`, `SlowdownManager`, `SlowdownInputController`.

### Scene-Specific Systems
- `GameFlowManager` — player spawning, mode switching, save loading.
- `FloorManager` — story/random battle sequencing.
- `SkillGenerator` — post-battle skill selection cards.

---

## DebugScene (Build 2 — Disabled)

Developer testing scene. Disabled from production builds.  
Entry: `MainMenuController.StartFight()` (not wired to any active button [UNVERIFIED]).

---

## SandboxScene (Build 3)

Free-form testing sandbox. No floor/story constraints.  
Entry: `MainMenuController.StartSandbox()`.

---

## ArenaScenes/Crossing (Build 4)

**Purpose**: Dedicated arena for battles with `arena = Arena.Crossing`.  
**Entry**: `GameFlowManager.EnterBattle()` loads this scene.  
**Exit**: Victory → ReturnScene (on GameSession); Death → TitleScene.  
**Components**: `ArenaBootstrapper`, `BattleManager`, `SpawnGroup`, Camera, lights.  
**Dependencies**: `GameSession.PendingBattle` and `ReturnScene` must be set before load.

---

## ArenaScenes/Cave (Build 5)

**Purpose**: Dedicated arena for battles with `arena = Arena.Cave` (mapped to scene `"Cave"` in `ArenaCatalog.SceneName`).  
**Entry**: `GameFlowManager.EnterBattle()` loads this scene.  
**Exit**: Victory → ReturnScene (on GameSession); Death → TitleScene.  
**Components**: `ArenaBootstrapper`, `BattleManager`, `SpawnGroup`, Camera, lights.  
**Dependencies**: `GameSession.PendingBattle` and `ReturnScene` must be set before load.

---

# 3. Prefab Registry

## Assets/Prefabs

### Actor.prefab
Generic enemy actor body. Spawned via `BattleManager.SpawnEnemies()`.  
**Components**: `Actor`, `ActorStateMachine`, `StatusManager`, `Rigidbody`, `CapsuleCollider`, `Animator`, `NavMeshAgent`.

### PlayerBattler.prefab
Player character body. Pre-placed in CaveScene; instantiated from `ArenaBootstrapper.playerPrefab` in arena scenes.  
**Components**: Same as Actor.prefab plus `VelocityIndicator`, `ParticleController`, `SpriteShapeRenderer`.  
**Note**: Body is re-created each scene; runtime state lives on `GameSession.PlayerRuntime`.

### ReworkedActor.prefab
[UNVERIFIED] — Possibly in-progress reworked Actor prefab.

### HpBar.prefab
World-space HP bar above actors. Components: `HpBar`, UI Images.

### Projectile.prefab
Projectile for `ProjectileAttack` actions. Components: `ProjectileHandler`, `Rigidbody`, `Collider`.

### Fireball.prefab
Fire-themed projectile variant.

### ActionButton.prefab
Draggable action button in UI containers. Spawned by `UIManager.InitializePlayerActionButtonPrefabs()`.  
**Components**: `ActionButtonBattle` or `ActionButtonInventory`, `Button`, `Image`, drag-drop handlers.

### ActionButtonMenu.prefab
Action button variant for inventory/menu context. Components: `ActionButtonInventory`.

### InventorySlot.prefab
Slot in skill/inventory view panel. [UNVERIFIED details]

### SkillCard.prefab
Post-battle skill selection card. Spawned by `SkillGenerator`. Components: `ActionCardHandler`, `Button`.

### Fire.prefab / Torch.prefab / Wall_Torch Variant.prefab
Environmental decoration. No runtime dependencies.

### GameObject.prefab
[UNVERIFIED] — Generic placeholder name.

## Assets/Resources/
- `HpPopup.prefab` / `PosturePopup.prefab` — Floating damage numbers, loaded via `Resources.Load`.

## Assets/Prefabs/Effects/
[UNVERIFIED] — Presumed particle prefabs for `PlayParticleEffect`.

---

## Runtime Spawning Summary

| Spawner | Prefab | When |
|---------|--------|------|
| `BattleManager.SpawnEnemies()` | `Actor.prefab` | On `BattleManager.Enter()` |
| `ArenaBootstrapper.SpawnAndBindPlayer()` | `PlayerBattler.prefab` | Arena scene Start() |
| `GameFlowManager.EnsurePlayerBody()` | `playerPrefab` | CaveScene Start() |
| `UIManager.InitializePlayerActionButtonPrefabs()` | `ActionButton.prefab` | After scene load or battle end |
| `SkillGenerator.DrawSkillsFromSelection()` | `SkillCard.prefab` | After battle victory |
| `ProjectileAttack.OnHit()` | `ProjectileAttack.projectilePrefab` | During combat |
| `PlayParticleEffect.Eval()` | `particlePrefab` | During action effects |
