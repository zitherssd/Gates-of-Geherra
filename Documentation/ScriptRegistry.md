# Gates of Gehera — Script Registry

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
| `Assets.Scripts.Battle.Actions` | `Scripts/Battle/Actions/` | BaseAction, BaseSkill, DamageInstance |
| `Assets.Scripts.Battle.Actions.Actions` | `Scripts/Battle/Actions/Actions/` | Concrete action implementations |
| `Assets.Scripts.Battle.Actions.Actions.Effects` | `Scripts/Battle/Actions/Actions/Effects/` | IEffect implementations |
| `Assets.Scripts.Battle.Components.Status` | `Scripts/Battle/Components/Status/` | BaseStatus, StatusManager |
| `Assets.Scripts.Battle.Components.Effects` | `Scripts/Battle/Components/Effects/` | EffectManager |
| `Assets.Scripts.Battle.Items` | `Scripts/Battle/Items/` | BaseItem, BaseConsumable, BaseTrinket, CraftingSystem |
| `Assets.Scripts.Crawler` | `Scripts/Crawler/` | FloorManager, SkillGenerator, CrawlerManager |
| `Assets.Scripts.Save` | `Scripts/Save/` | SaveManager, SaveData, ActorSaveData |
| `Assets.Scripts.Pattern` | `Scripts/Pattern/` | StateMachine, IState, IAttack |
| `Assets.Scripts.UI` | `Scripts/UI/` | UIManager, ActionCardHandler, etc. |
| `Assets.Scripts.Utility` | `Scripts/Utility/` | CameraManager, SlowdownManager, SoundManager, InputHandler, etc. |
| (global) | `Scripts/TitleScene/` | MainMenuController, MainMenuSlot |

---

## Core Game Flow Scripts

### GameSession
**File**: `Scripts/Game/GameSession.cs`  
**Responsibility**: Persistent, data-only source of truth for the entire run. Survives scene loads (DontDestroyOnLoad).  
**Public API**:
- `Instance` (singleton, auto-creates)
- `StartNewRun(playerName)` — rolls 3d6 stats, resets floor/training counters
- `AdoptPlayerRuntime(runtime)` — takes ownership of a loaded runtime
- `ClearRun()` — wipes all run state (death/return to title)
- `NotifyPlayerSpawned(player)` — fires `OnPlayerSpawned` event
**Key Fields**: `PlayerRuntime`, `currentFloor`, `trainingsDone`, `trainingsDoneThisFloor`, `timelocks`, `PendingBattle`, `ReturnScene`, `Loadout`
**Events**: `OnPlayerSpawned : Action<Actor>`

### GameFlowManager
**File**: `Scripts/Game/GameFlowManager.cs`  
**Responsibility**: Per-scene coordinator for the CaveScene. Handles player body spawning, save loading, mode switching (Rest ↔ Battle).  
**Public API**:
- `SetMode(GameMode)` — switches rest/battle mode, shows/hides UI
- `EnterBattle(BattleDefinition, onBattleEnd)` — loads arena scene or calls in-scene BattleManager.Enter
- Delegating properties: `trainingsDone`, `trainingsDoneThisFloor`, `timelocks` (forward to GameSession)
**Dependencies**: `GameSession`, `SaveManager`, `BattleManager`, `UIManager`, `RestAreaManager`

### ArenaBootstrapper
**File**: `Scripts/Game/ArenaBootstrapper.cs`  
**Responsibility**: Entry point for dedicated arena scenes. Spawns + binds player, registers with BattleManager, starts battle.  
**Serialized Fields**: `playerPrefab`, `debugBattle`
**Dependencies**: `GameSession`, `BattleManager`, `UIManager`

---

## Actor System

### Actor
**File**: `Scripts/Battle/Actor/Actor.cs`  
**Responsibility**: The primary MonoBehaviour for any fighter (player or enemy). Coordinates all sub-systems.  
**Public Events**:
- `OnBeforeTakeDamage(DamageInstance)`
- `OnAfterTakeDamage(DamageInstanceResult)`
- `OnBeforeDealDamage(DamageInstance)`
- `OnAfterDealDamage(DamageInstance)`
- `OnActionUsed(BaseAction)`
- `OnReset`
**Public Methods**:
- `UseAction(BaseAction)` — begins action, interrupts current if needed
- `ApplyDamageInstance(DamageInstance, attacker, action)` — applies damage, stagger, knockback
- `DealDamage(DamageInstance, target, action)` — fires deal-events and delegates to target
- `Bind(ActorRuntime)` — attach persistent runtime (arena scenes, no stat reset)
- `SetDefinition(ActorDefinition)` — set SO template, optionally build runtime
- `Spawn()` — resets runtime to definition (enemy spawn)
- `Init()` — one-time setup after SetDefinition (colors, etc.)
**Serialized Fields**: `Definition` (ActorDefinition SO), `playerSpawnPoint`, `PostureRegenCooldownDelay`, `StaminaRegenRate`, `PostureRegenRate`
**Sub-components** (not MonoBehaviours): `EffectManager effects`, `TargetingSystem target`, `MovementSystem movement`, `ActorInventory inventory`, `AIBT ai`

### ActorRuntime
**File**: `Scripts/Battle/Actor/ActorRuntime.cs`  
**Responsibility**: Serializable runtime data model. Survives scene loads when owned by GameSession.  
**Key Fields**: `Name`, `hpBars`, `Strength`, `Agility`, `Mind`, `Spirit`, `actions`, `items`, `currentBuildup/Posture/Stamina`
**Computed Properties**: `maxBuildup = base + Mind`, `maxPosture = base + Strength`, `maxStamina = base + Agility×2`
**Events**: `OnDeath`

### ActorDefinition
**File**: `Scripts/Battle/Actor/ActorDefinition.cs`  
**Responsibility**: ScriptableObject template for any actor. Defines all starting values, AI ruleset, colors, starting actions/items.  
**CreateAssetMenu**: `ScriptableObjects/Actor`  
**Key Assets**: `Resources/Actors/MC.asset`, `Resources/Actors/Old Prisoner.asset`, `Resources/Actors/Enraged Maniac.asset`, `Resources/Actors/Shuriken Thrower.asset`, etc.

### ActorStateMachine
**File**: `Scripts/Battle/Actor/ActorStateMachine.cs`  
**Responsibility**: Manages actor state transitions. Exposes animation event entry points.  
**Animation Event Methods** (called from Animator clips):
- `EnterWindup(int windupFrames)`
- `OnHit()`
- `EnterRecovery()`
- `OnEnd()`
**States Registered**: `IdleState`, `InactiveState`, `FumbleState`, `AirStaggerState`, `AirNeutralState`, `LandingState`, `ActingState`, `RollState`, `BlockState`, `MoveState`, `GettingUpState`, `StaggerState`, `DeathState`

### Actor States
**All in**: `Scripts/Battle/Actor/States/`

| State | Trigger | Notes |
|-------|---------|-------|
| `IdleState` | Default from ActingState/MoveState end | Player/enemy waiting |
| `InactiveState` | `Actor.Start()` | Before battle begins |
| `ActingState` | `BaseAction.PerformSpecific()` | During action animation |
| `MoveState` | `MoveAction.PerformSpecific()` | During movement action |
| `BlockState` | `Block.PerformSpecific()` | During block stance |
| `StaggerState` | `Actor.ApplyDamageInstance()` when posture ≤ 50% | Duration-based stagger |
| `DeathState` | `Actor.ApplyDamageInstance()` when HP = 0 | Death animation, triggers BattleActive check |
| `FumbleState` | [UNVERIFIED] | |
| `AirNeutralState` | `Jump.PerformSpecific()` | |
| `AirStaggerState` | [UNVERIFIED] | |
| `RollState` | `Jump` with roll tag | |
| `LandingState` | On landing | |
| `GettingUpState` | After FumbleState | |

---

## Battle Management Scripts

### BattleManager
**File**: `Scripts/Battle/Manager/BattleManager.cs`  
**Responsibility**: Orchestrates a single battle. Spawns enemies, positions player, manages state machine.  
**Public API**:
- `Enter(BattleDefinition, onBattleEnd)` — full battle setup + state machine start
- `TriggerBattleEnd()` — invokes `OnBattleEnd`
- `Player` property — first PlayerActor or GameFlowManager fallback
**Events**: `OnBattleEnd`, `OnNewTurn`
**Singleton**: `BattleManager.instance`
**Notes**: Sets `Physics.gravity = (0, -6, 0)` in Start().

### BattleStateMachine / States
**Files**: `Scripts/Battle/Manager/BattleStateMachine.cs`, `Scripts/Battle/Manager/States/`  
**States**:
- `BattleStartState` — enables UI, fires `ItemEventBus.OnNewBattle`, transitions to Active
- `BattleActiveState` — monitors for all-dead condition, triggers end slowdown
- `BattleEndState` — awards items/skills, saves, returns to rest or ReturnScene

---

## Action System Scripts

### BaseAction
**File**: `Scripts/Battle/Actions/BaseAction.cs`  
**Responsibility**: Abstract base for all playable actions (ScriptableObject).  
**Key Fields**: `guid`, `Type` (BUTTONTYPE), `Name`, `Rarity`, `CooldownTimer`, `TotalUses`, `BuildupCost`, `BuildupGain`, `StaminaCost`, `Tags`, `StickMult`
**Events**: `OnActionEnded : Action<BaseAction, ActionEndReason>`
**Lifecycle**: `Begin(actor)` → `PerformSpecific(actor, onEnd)` → `EndAction(reason)` → `Cleanup(reason)`

### BaseSkill (extends BaseAction)
**File**: `Scripts/Battle/Actions/BaseSkill.cs`  
**Adds**: Animation lifecycle hooks — `OnHit()`, `OnUpdate(dt)`, `OnEnterWindup(animator)`, `OnEnterRecovery(animator)`

### Concrete Action Types
(see `action-system-architecture.md` memory for full details)
- `AttackSkill` — hitbox-based melee
- `GenericSkill` — effect-list driven flexible action
- `ProjectileAttack` — spawns projectile prefab
- `Dodge` — evasion with force
- `Block` — enters BlockState
- `Charge` — directional lunge with knockback
- `Jump` — air state transition
- `MoveAction` — directional movement

---

## AI System

### AIBT
**File**: `Scripts/Battle/Actor/AI/AIBT.cs`  
**Responsibility**: Behavior tree controller. Ticks behaviors in priority order each frame when actor is Idle or Moving.  
**AI Rulesets** (from `AiRuleset` enum):
- `DEFAULT` / `SandboxGuy` — passive, moves away from level bounds
- `OldManBehavior` — blocks incoming, attacks, circles
- `EngragedManiac` — aggressive attack-first, dodge, chase
- `Ninja` — [UNVERIFIED — no behavior list assigned]
- `ShurkienThrower` — ranged harassment, keeps 3–5 unit distance
- `TacticalFlanker` — blocks, hesitation-gated attack, circles at distance

### AI Behaviors (in `Scripts/Battle/Actor/AI/Behaviors/`)
- `ApproachBehavior`, `FlankApproachBehavior`, `GroupFlankBehavior` — movement toward player
- `MoveToOrbitBehavior` — circular movement
- `MoveBehavior`, `DashBehavior`, `DodgeBehavior` — tactical repositioning
- `AttackWithValidSkill`, `AttackProjectile` — attack execution
- `BlockBehavior`, `BlockCancelBehavior` — defensive behaviors
- `ReactionBehavior` — [UNVERIFIED — likely reactive defense]

### AI Conditions (in `Scripts/Battle/Actor/AI/Conditions/`)
- `DistanceConditions` — `DistanceToPlayerGreaterThan`, `DistanceToPlayerSmallerThan`
- `GlobalAttackTokenCondition` — rate-limits AI attacks globally
- `HesitateCondition` — probability-gated delay
- `InsideEnemyHitbox` — checks if inside player attack range
- `StaminaConditions` — stamina threshold checks

---

## Save System Scripts

### SaveManager
**File**: `Scripts/Save/SaveManager.cs`  
**Responsibility**: Disk persistence. DontDestroyOnLoad.  
**API**: `SaveToSlot(int)`, `LoadFromSlot(int)`, `SaveGame(slot, data)`, `LoadGame(slot)`, `DeleteSave(slot)`, `SlotExists(slot)`  
**Platform**: File I/O on PC; PlayerPrefs on WebGL.  
**Dependencies**: `GameSession`, `GameFlowManager`, `UIManager`, `ActionDatabase`

### ActionDatabase
**File**: `Scripts/Utility/ActionDatabase.cs`  
**Responsibility**: Lookup table for `BaseAction` by GUID. Used by save system to restore actions.  
**Asset**: `Resources/Action Database.asset`

---

## UI Scripts

### UIManager
**File**: `Scripts/UI/UIManager.cs`  
**Responsibility**: Central UI controller for action buttons, fades, text, UI state.  
**API**:
- `InitializePlayerActionButtonPrefabs(actions, loadout)` — creates/rebuilds action buttons
- `EnableUI()` / `DisableUI()` / `ShowUI()` / `HideUI()`
- `EnableBattleSkills()` / `DisableBattleSkills()`
- `ShowRestingUI()` / `HideRestingUI()`
- `Fade(isFadeIn, onComplete)`
- `GetCurrentLoadout()` → `List<ActionSlotSaveData>`
**Action Containers**: `LeftContainer`, `LeftContainerSecondary`, `RightContainer`, `RightContainerSecondary`
**Static Events**: `OnHideUI`, `OnShowUI`

### ActionButtonBattle / ActionButtonInventory
**Files**: `Scripts/Battle/Actions/ActionButtonBattle.cs`, `ActionButtonInventory.cs`  
**Responsibility**: Button behaviour for in-battle action execution vs. inventory browsing.

### DropSlot
**File**: `Scripts/UI/DropSlot.cs`  
**Responsibility**: Drag-and-drop target containers for action button rearrangement.

### JoystickManager
**File**: `Scripts/UI/JoystickManager.cs`  
**Responsibility**: Virtual on-screen joystick for touch controls.

---

## Utility Scripts

### CameraManager
**File**: `Scripts/Utility/CameraManager.cs`  
**Responsibility**: Dynamic camera positioning. Weights position between player and enemies. Supports shake, slow-track.  
**Key Fields**: `UpDistance`, `BackDistance`, `SlowTrack`, `Override`  
**Dependencies**: `GameSession.OnPlayerSpawned`, `BattleManager.PlayerActors`

### SlowdownManager
**File**: `Scripts/Utility/SlowdownManager.cs`  
**Responsibility**: Time-scale manipulation. State slowdown (0.05×) and temporary slowdown (animated curve).  
**API**: `EnterStateSlowdown()`, `ExitStateSlowdown()`, `TriggerTemporarySlowdown(duration)`, `StartHoldResume()`, `EndHoldResume()`  
**Events**: `OnStateSlowdownStart`, `OnStateSlowdownEnd`, `OnTemporarySlowdownStart`, `OnTemporarySlowdownEnd`

### SoundManager
**File**: `Scripts/Utility/SoundManager.cs`  
**Responsibility**: Audio playback. `PlayMusic(clip)`, `PlayMusicRest()`, `PlaySE(name)`, `FadeOutMusic()`

### InputHandler
**File**: `Scripts/Utility/InputHandler.cs`  
**Responsibility**: Touch/pointer input. Detects swipe vs click vs hold.  
**Events**: `OnSwipe(Vector2 delta)`, `OnClick(Vector2 pos)`, `OnHold(Vector2 pos)`  
**Input**: Unity Input System (`InputActionReference` for press/end/hold/position)

### StaticHelpers
**File**: `Scripts/Utility/StaticHelpers.cs`  
**Responsibility**: Math utility methods. `LinearMap()` used in stagger duration calculation.

### TimeLockManager
**File**: `Scripts/Utility/` [UNVERIFIED location — referenced as `Assets.Scripts.Game.TimeLockManager`]  
**Responsibility**: Named real-time timers. `Add(name, TimeSpan)`, `Get(name)`, `Remove(name)`.  
**Uses**: Training cooldown, QuickFight cooldown.

---

## Item System Scripts

### BaseItem / BaseConsumable / BaseTrinket
**Files**: `Scripts/Battle/Items/`  
- `BaseItem` — base ScriptableObject with `ItemName`, `Description`, `Icon`, `stackable`
- `BaseConsumable` — adds `List<IItemEffect> Effects`; effects eval on `ActorInventory.UseConsumable()`
- `BaseTrinket` — adds `List<ItemTrigger> Triggers` + `List<IItemEffect> Effects`; auto-subscribes to `ItemEventBus` on equip

### ItemEventBus
**File**: `Scripts/Battle/Items/ItemEventBus.cs`  
**Responsibility**: Static event bus for item triggers. Maps `ItemTrigger` → `(Actor owner, IItemEffect)` pairs.  
**Known Triggers**: `OnNewBattle`, `OnNewFloor`, `OnDamageTaken`

### ActorInventory
**File**: `Scripts/Battle/Actor/ActorInventory.cs`  
**Responsibility**: Runtime item list for an actor. Handles equip/unequip of trinkets automatically.  
**API**: `AddItem(item)`, `RemoveItem(item)`, `UseConsumable(item)`, `RebindTo(sharedList)`

### CraftingSystem
**File**: `Scripts/Battle/Items/CraftingSystem.cs`  
**Responsibility**: Static recipe-based item crafting. `TryCraft(provided, recipe, out result)`.  
**Notes**: Currently no UI integration found.
