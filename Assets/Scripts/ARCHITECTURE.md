# Gates of Gehera - Architecture Overview

## Project Summary

**Gates of Gehera** is a turn-based combat roguelike game with a progression-based structure. The game features:
- Strategic dungeon progression across multiple floors
- Real-time action-oriented combat with skill-based gameplay
- Character progression through training and item acquisition
- Enemy AI controlled by behavior trees
- Save/load system with persistent progression tracking

---

## Table of Contents

1. [High-Level Architecture](#high-level-architecture)
2. [Main Gameplay Flow](#main-gameplay-flow)
3. [Major Systems](#major-systems)
4. [System Organization](#system-organization)
5. [Communication Between Systems](#communication-between-systems)
6. [Architectural Patterns](#architectural-patterns)
7. [Key Dependencies](#key-dependencies)
8. [Structural Issues & Recommendations](#structural-issues--recommendations)
9. [Development Guidelines](#development-guidelines)

---

## High-Level Architecture

The codebase is organized into **7 major folders** under `Assets/Scripts/`:

```
Battle/          - Core combat system, actors, actions, state management
Game/            - High-level game flow, progression, mode switching
Crawler/         - Floor progression system
UI/              - User interface management and handlers
Save/            - Save/load system
Utility/         - Managers, helpers, and shared utilities
Pattern/         - Architectural patterns (StateMachine, interfaces)
Editor/          - Custom editor tools and inspectors
```

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                   GameFlowManager (Main Entry Point)        │
│  Orchestrates: Game modes, progression, save/load setup    │
└──────────┬──────────────────────────────────────────────────┘
           │
    ┌──────┴───────┬──────────────┬─────────────────┐
    │              │              │                 │
    ▼              ▼              ▼                 ▼
┌─────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐
│BattleMan│  │RestArea  │  │Training  │  │FloorManager  │
│  ager   │  │ Manager  │  │ Manager  │  │              │
└────┬────┘  └──────────┘  └──────────┘  └──────────────┘
     │
     ├────▶ Battle System
     │      ├─ BattleStateMachine
     │      ├─ Actor System (Player + Enemy Actors)
     │      ├─ Action/Skill System
     │      └─ Status & Effects
     │
     └────▶ Subsystems
            ├─ UI System (UIManager, handlers)
            ├─ Input System (InputHandler)
            ├─ Audio System (SoundManager)
            └─ Save System (SaveManager)
```

---

## Main Gameplay Flow

### 1. **Game Initialization**
- Scene loads with **GameFlowManager** singleton
- If save exists: Load player data from **SaveManager**
- If new game: Create player from template **ActorData**
- Initialize **UIManager** with player actions
- Transition to **Rest Area** mode

### 2. **Rest Area Loop**
- Player waits in **RestAreaManager**
- Can trigger **Training** (increases stats over time)
- Can initiate **Floor Battle** or **Quick Fight**
- UI displays progression, next floor, stat gains

### 3. **Battle Flow**
1. **BattleManager.Enter()** - Initialize battle with **BattleDefinition**
2. **BattleStateMachine.TransitionTo<BattleStartState>()** - Setup phase
3. **BattleActiveState** - Main combat loop:
   - Player inputs actions via **UIManager** → **InputHandler**
   - Player **Actor** executes **BaseAction**/**BaseSkill**
   - Enemy **Actor** executes AI decisions via **AIBT** (Behavior Tree)
   - **Action System** handles animation, damage, effects
   - **StatusManager** processes ongoing status effects
4. **BattleEndState** - Cleanup, rewards, save progression
5. Return to **Rest Area**

### 4. **Progression**
- Enemies defeated → Reward items, stats
- Training completed → Stat increments
- Floor cleared → Advance to next floor
- Progress saved to **SaveManager**

---

## Major Systems

### 1. **Battle System** (`Battle/`)

The core combat engine managing all in-battle interactions.

#### **BattleManager** (`Battle/BattleManager/BattleManager.cs`)
- **Responsibility**: Orchestrates entire battle lifecycle
- **Key Members**:
  - `PlayerActors` - List of player-controlled actors
  - `EnemyActors` - List of AI-controlled enemies
  - `BattleStateMachine` - Manages battle states
  - Events: `OnBattleStart`, `OnBattleEnd`
- **Key Methods**:
  - `Enter(BattleDefinition battle, Action onBattleEnd)` - Start battle
  - `UpdateBattle()` - Per-frame battle logic

#### **BattleStateMachine** (`Battle/BattleManager/BattleStateMachine.cs`)
- **Responsibility**: State-based battle progression
- **States**:
  - `BattleStartState` - Initialize actors, UI
  - `BattleActiveState` - Main combat loop
  - `BattleEndState` - Cleanup, rewards
- **Pattern**: State Machine (generic pattern from `Pattern/StateMachine.cs`)

#### **BattleDefinition** (`Battle/BattleManager/BattleDefinition.cs`)
- **Responsibility**: Data structure defining battle configuration
- **Contains**: Enemy list, difficulty, rewards

---

### 2. **Actor System** (`Battle/Actor/`)

Represents any in-game combatant (player or enemy).

#### **Actor** (`Battle/Actor/Actor.cs`) - Core Script
- **Responsibility**: Central entity for all actors; aggregates subsystems
- **Key Components**:
  - **ActorStateMachine** - Manages actor animation/movement states (IdleState, ActingState, StaggerState, etc.)
  - **StatusManager** - Manages status effects (poison, stagger, etc.)
  - **AIBT** - Enemy AI behavior tree
  - **MovementSystem** - Physics-based movement and positioning
  - **TargetingSystem** - Track closest enemies, ability targeting
  - **ActorInventory** - Items/equipment
  - **EffectManager** - Visual effects, damage popups, hit visualization
  - **AudioManager** - Sound effects
- **Key Data**:
  - `ActorData` - ScriptableObject with stats, abilities, configuration
- **Key Events**:
  - `OnBeforeTakeDamage`, `OnAfterTakeDamage`
  - `OnBeforeDealDamage`, `OnAfterDealDamage`
  - `OnActionUsed`
- **Key Methods**:
  - `Spawn()` - Initialize actor in battle
  - `ApplyDamageInstance(DamageInstance, caster, action)` - Receive damage
  - `ExecuteAction(BaseAction)` - Perform action
  - `Update()` - Every frame: stamina regen, status ticking, AI updates

#### **ActorData** (`Battle/Actor/ActorData.cs`) - Configuration
- **Responsibility**: Serializable stats and ability configuration
- **Key Data**:
  - Base stats: HP bars, Stamina, Posture, Buildup
  - Attribute multipliers: Strength, Agility, Mind, Spirit
  - Available actions/skills
  - AI ruleset (behavior configuration)
- **Used by**: Actor initialization, UI stat display, damage calculations
- **Note**: Created as ScriptableObjects in Resources folder

#### **ActorStateMachine** (`Battle/Actor/ActorStateMachine.cs`)
- **Responsibility**: Actor animation/action state management
- **States**:
  - `IdleState` - Idle, regenerating stats
  - `ActingState` - Executing an action (windup → recovery)
  - `MoveState` - Moving
  - `StaggerState` - Knocked back, can recover
  - `DeathState` - Dead
  - `BlockState` - Blocking
  - `RollState` - Dodging
  - Plus air variants and specialized states
- **Pattern**: State Machine (implements `IState` interface)
- **Usage**: Called by actions to transition actor into action-execution mode

#### **ActorInventory** (`Battle/Actor/ActorInventory.cs`)
- **Responsibility**: Tracks items held by actor
- **Usage**: Equipment, consumables

---

### 3. **Action/Skill System** (`Battle/Actions/`)

Framework for player and enemy actions (attacks, skills, abilities).

#### **BaseAction** (`Battle/Actions/BaseAction.cs`) - Abstract Base
- **Responsibility**: Template for all executable actions
- **Key Data**:
  - `Type` - INSTANT, VECTOR, CONTINUOUS, CONTINUOUS_VECTOR
  - Costs: Stamina, Buildup
  - Cooldown tracking
  - Damage properties (base damage, posture damage, knockback)
- **Key Methods**:
  - `Begin(Actor caster)` - Initiate action
  - `PerformSpecific(Actor caster, Action callback)` - Override for custom logic
  - `EndAction(ActionEndReason reason)` - Complete action execution
  - `UpdateCooldown()` - Per-frame cooldown decrement
- **Events**: `OnActionEnded`

#### **BaseSkill** (`Battle/Actions/BaseSkill.cs`) - Extends BaseAction
- **Responsibility**: Skills with animation support
- **Members**:
  - `Animation` - Associated animator state
  - Virtual methods for custom lifecycle: `OnHit()`, `OnUpdate()`, `OnEnterWindup()`, `OnEnterRecovery()`

#### **Concrete Skills** (`Battle/Actions/Actions/`)
Examples of implemented skills:
- **AttackSkill** - Basic melee attack with hitbox detection
- **Dodge** - Evasion action
- **Block** - Defense stance
- **Jump** - Aerial positioning
- **Charge** - Buildup attack variant
- **ProjectileAttack** - Ranged attack
- **GenericSkill** - Flexible template skill
- **MoveAction** - Movement without attacking

#### **Action Button Handlers** (`Battle/Actions/`)
- **ActionButtonBattle** - UI button during combat
- **ActionButtonInventory** - UI button for item actions
- **ActionButtonHandler** - Manages button state and selection

---

### 4. **AI System** (`Battle/Actor/AI/`)

Enemy decision-making and behavior control.

#### **AIBT** (`Battle/Actor/AI/AIBT.cs`) - Behavior Tree Manager
- **Responsibility**: Controls enemy AI through behavior tree evaluation
- **Members**:
  - `behaviors` - List of BTNode (behavior tree nodes)
  - `AIState` - Current thinking/acting/moving state
  - `currentState` - Tracks execution state
- **Key Methods**:
  - `Update()` - Evaluate behavior tree each tick
  - `Reset()` - Reinitialize based on actor's AIRuleset
- **Rulesets**: Configurable AI personalities
  - DEFAULT (SandboxGuy)
  - OldMan
  - Ninja
  - Maniac
  - Hungry
  - ShurkienThrower

#### **Behavior Tree** (`Battle/Actor/AI/`)
- **BTNode** - Base node class (Sequence, Selector, etc.)
- **Conditions** - Decision nodes checking actor state:
  - `DistanceToPlayerSmallerThan`
  - `StaminaLessThan`
  - `InsideEnemyHitbox`
  - `OnCooldown`
- **Behaviors** - Action nodes executing decisions:
  - `ApproachBehavior` - Move toward target
  - `AttackWithValidSkill` - Execute best available attack
  - `DodgeBehavior` - Execute dodge
  - `BlockBehavior` - Execute block
  - `MoveAwayFromLevel` - Avoid falling off boundaries
  - `FlankApproachBehavior` - Tactical positioning

---

### 5. **Status & Effects System** (`Battle/Components/Status/`)

Runtime effects and status conditions applied to actors.

#### **StatusManager** (`Battle/Components/Status/StatusManager.cs`)
- **Responsibility**: Track and process status effects
- **Members**:
  - `activeStatuses` - List of current effects
- **Key Methods**:
  - `Add(BaseStatus status)` - Apply new status
  - `Remove(BaseStatus status)` - Remove status
  - `Tick()` - Per-frame status updates

#### **StatusEffect** (`Battle/Components/Status/StatusEffect.cs`) - Base Class
- **Responsibility**: Template for status conditions
- **Key Methods**:
  - `Apply()` - Initial effect setup
  - `Tick()` - Per-frame processing
  - `Remove()` - Cleanup when expired
- **Examples**: PoisonStatus (ongoing damage over time)

#### **EffectLogic** (`Battle/Components/Status/EffectLogic/`)
- Contains specialized effect implementations
- Used by status effects for damage calculations, visual effects

---

### 6. **Effect & Audio System**

#### **EffectManager** (`Battle/Components/Effects/EffectManager.cs`)
- **Responsibility**: Visual feedback for attacks and damage
- **Key Features**:
  - Hitbox visualization (polygon drawing with LineRenderer)
  - Damage popups (floating damage numbers)
  - Color flashing on hit
  - Particle effects coordination
- **Members**:
  - `lineRenderer` - Draws attack hitboxes in real-time
  - `owner` - The actor this manager belongs to

#### **AudioManager** (`Battle/Components/Audio/AudioManager.cs`)
- **Responsibility**: Per-actor audio effects
- **Key Features**:
  - Play sound on damage taken
  - Random pitch variation for variety
  - Uses global **SoundManager** for clip lookup

---

### 7. **Game Flow System** (`Game/`)

High-level game mode and progression management.

#### **GameFlowManager** (`Game/GameFlowManager.cs`) - Main Orchestrator
- **Responsibility**: Central game flow controller; singleton
- **Members**:
  - `playerActor` - Reference to player
  - `battleManager`, `restAreaManager`, `trainingManager` - Subsystem managers
  - `Mode` - Current game mode (Battle, RestArea, Training, etc.)
  - `timelocks` - Time-locked actions (training delays)
- **Key Methods**:
  - `Start()` - Load/create player, initialize
  - `EnterBattle(BattleDefinition, onBattleEnd)` - Transition to battle
  - `SetMode(GameMode)` - Switch game mode
  - `RollNewStats(Actor)` - Generate random starting stats
- **Game Modes**:
  - RestArea - Safe zone UI
  - Battle - In-combat
  - Training - Stat training (time-locked)

#### **RestAreaManager** (`Game/RestAreaManager.cs`)
- **Responsibility**: Rest area logic and transitions
- **Key Methods**:
  - `Enter()` - Transition to rest area, play rest music
- **Features**: Position player, fade UI, play animations

#### **TrainingManager** (`Game/TrainingManager.cs`)
- **Responsibility**: Character progression through training
- **Key Features**:
  - Time-locked training (10s → 15m per training)
  - Random stat awards after completion
  - Button state management
- **Uses**: `TimeLockManager` for tracking real-time delays

#### **FloorManager** (`Crawler/FloorManager.cs`)
- **Responsibility**: Dungeon progression
- **Members**:
  - `currentFloor` - Progression counter
  - `StoryBattles` - Main progression battles per floor
  - `RandomBattles` - Optional quick fights
  - `floorEnemiesList` - Enemy rosters per floor
- **Key Methods**:
  - `ProgressToNextFloor()` - Advance to next floor
  - `QuickFight()` - Optional random battle
  - `GetActorsForFloor()` - Retrieve enemy roster for current floor

---

### 8. **UI System** (`UI/`)

User interface management and interaction handling.

#### **UIManager** (`UI/UIManager.cs`) - Main UI Orchestrator
- **Responsibility**: Central UI controller; singleton
- **Members**:
  - `ActionsHolder` - Left panel for player actions
  - `SkillHolder` - Right panel for skills
  - `TopTextbox`, `MiddleTextbox` - Text display
  - `SlowdownMeter` - Game mechanic meter (slowdown buildup)
  - `selectedAction`, `selectedSkill` - Currently selected action
- **Key Methods**:
  - `InitializePlayerActionButtonPrefabs()` - Setup action UI
  - `SelectAction(BaseAction)` - Handle action selection
  - `GainMeter(float)` - Increase slowdown meter
  - `Fade(bool in, Action callback)` - Fade UI in/out
  - `ShowRestingUI()` - Display rest screen
  - `GetCurrentLoadout()` - Return current action setup for saving
- **Events**: `OnHideUI`, `OnShowUI`

#### **Action UI Handlers** (`UI/`)
- **ActionCardHandler** - Individual action card UI
- **ActionInventoryHandler** - Inventory UI display
- **ActionUIManager** - Placeholder for future expansion
- **TargetingManager** - Handle targeting UI state
- **JoystickManager** - Joystick input for skills
- **DropSlot** - Drag-and-drop UI support

---

### 9. **Save/Load System** (`Save/`)

Game progress persistence.

#### **SaveManager** (`Save/SaveManager.cs`) - Singleton
- **Responsibility**: Game save/load coordination
- **Key Features**:
  - Multiple save slot support (slot-based)
  - JSON serialization
  - WebGL compatibility (PlayerPrefs fallback)
  - DontDestroyOnLoad persistence
- **Members**:
  - `actionDatabase` - Reference for loading action definitions
  - `currentSaveSlot` - Active save slot
- **Key Methods**:
  - `SaveToSlot(int slot)` - Write save data
  - `LoadFromSlot(int slot)` - Read and deserialize save
  - `SaveGame()`, `LoadGame()` - File/PlayerPrefs I/O
  - `SlotExists(int)` - Check slot availability
  - `DeleteSave(int)` - Remove save file

#### **SaveData** (`Save/SaveData.cs`)
- **Responsibility**: Serializable save state structure
- **Members**:
  - `player` - ActorSave (player data snapshot)
  - `currentFloor` - Progression tracking
  - `timelocks` - Time-locked actions state
  - `trainingsDone`, `trainingsDoneThisFloor` - Progression counters

---

### 10. **Input System** (`Utility/InputHandler.cs`)

Input handling and event dispatch.

#### **InputHandler** - Singleton
- **Responsibility**: Centralized input processing
- **Input Methods**:
  - Touch/click detection
  - Swipe threshold detection
  - Hold detection
- **Events**:
  - `OnSwipe(Vector2 delta)` - Swipe input fired
  - `OnClick(Vector2 position)` - Click/tap input fired
  - `OnHold(Vector2 position)` - Long hold input fired
- **Integration**: Uses new Unity Input System with InputActionReference

---

### 11. **Audio System** (`Utility/SoundManager.cs`)

Centralized audio management.

#### **SoundManager** - Singleton
- **Responsibility**: Global audio coordination
- **Members**:
  - `musicSource` - Background music playback
  - `efxSource` - Sound effects playback
  - `music`, `restMusic`, `soundEffects` - Audio clip lists
- **Key Methods**:
  - `PlayMusic(AudioClip)` - Play random track from music list
  - `PlayMusicRest()` - Play random rest-area music
  - `PlaySE(string name)` - Play sound effect by name
  - `RandomizeSfx(params AudioClip[])` - Play random sound with pitch variation
  - `FadeOutMusic()` - Crossfade to silence
  - `GetAudioClipByName(string)` - Look up audio clip

---

### 12. **Utility System** (`Utility/`)

Helper classes and shared functionality.

#### **StaticHelpers** - Utility Methods
- `LinearMap()` - Remap value from one range to another
- `ApplyEasing()` - Apply easing curves to animation values

#### **Other Utilities**:
- **CameraManager** - Camera control and zoom
- **ProjectileHandler** - Projectile instantiation and collision
- **SelectionCircle** - Visual actor selection indicator
- **HpBar**, **HpBarHandler** - Health bar UI and management
- **DamagePopup** - Floating damage number display
- **ParticleController** - Particle effect coordination
- **ColorController** - Actor color manipulation (hit flashes, etc.)
- **ActionDatabase** - Centralized action asset registry
- **Intersections** - Geometric collision detection helpers
- **CanvasHandler** - Canvas setup and management

---

## System Organization

### By Responsibility

| System | Location | Purpose |
|--------|----------|---------|
| **Battle** | `Battle/BattleManager/` | Core battle loop orchestration |
| **Actor** | `Battle/Actor/` | Individual combatant logic |
| **Action** | `Battle/Actions/` | Executable abilities framework |
| **AI** | `Battle/Actor/AI/` | Enemy decision-making |
| **Status** | `Battle/Components/Status/` | Status effects and conditions |
| **Effects** | `Battle/Components/Effects/` | Visual feedback system |
| **Game Flow** | `Game/` | High-level mode management |
| **Map Progression** | `Crawler/` | Floor/dungeon progression |
| **UI** | `UI/` | User interface and interaction |
| **Save/Load** | `Save/` | Persistence layer |
| **Input** | `Utility/InputHandler.cs` | Input event distribution |
| **Audio** | `Utility/SoundManager.cs` | Audio management |

### By Layer

```
Presentation Layer (UI/)
  ↓
Game Flow / Mode Management (Game/, Crawler/)
  ↓
Battle Orchestration (Battle/BattleManager/)
  ↓
Entity Logic (Battle/Actor/, Battle/Actions/, Battle/Components/)
  ↓
Infrastructure (Utility/, Save/, Pattern/)
```

---

## Communication Between Systems

### 1. **Event-Driven Communication**

Events allow decoupled system interaction:

- **Actor Events**:
  ```csharp
  OnBeforeTakeDamage  → StatusManager, EffectManager subscribe
  OnAfterTakeDamage   → AudioManager plays damage sound
  OnBeforeDealDamage  → Damage calculation modifiers
  OnAfterDealDamage   → UI updates, rewards
  OnActionUsed        → UI feedback
  ```

- **Input Events**:
  ```csharp
  InputHandler.OnSwipe    → TargetingManager, MovementController
  InputHandler.OnClick    → ActionSelection
  InputHandler.OnHold     → Charging actions
  ```

- **UIManager Events**:
  ```csharp
  OnHideUI, OnShowUI → State machines, input blocking
  ```

### 2. **Direct References (Coupling Points)**

Some critical connections require direct references:

- **GameFlowManager**:
  - Holds references to `playerActor`, `battleManager`, `restAreaManager`
  - Used by: All managers to coordinate flow

- **BattleManager**:
  - Globally accessible to actors, UI
  - Used for: Battle state queries, actor lists

- **UIManager**:
  - Globally accessible singleton
  - Used by: Battle system, actors for UI updates

- **SaveManager**:
  - Coordinates with `GameFlowManager`, `FloorManager`
  - Used for: Loading/saving progression

### 3. **Dependency Chain Example: Action Execution**

```
1. UIManager (player selects action)
   ↓
2. Player Actor executes BaseAction.Begin()
   ↓
3. Action transitions Actor to ActingState via ActorStateMachine
   ↓
4. ActingState handles animation, windup/recovery
   ↓
5. Action.OnHit() called by animation event
   ↓
6. Damage calculated and Actor.ApplyDamageInstance() called on targets
   ↓
7. Target receives events: OnBeforeTakeDamage, OnAfterTakeDamage
   ↓
8. StatusManager processes status effects
   ↓
9. EffectManager shows damage popups, color flash
   ↓
10. AudioManager plays hit sound
   ↓
11. UIManager updates health bars, slowdown meter
```

### 4. **ScriptableObject-Driven Data**

- **BaseAction** - Created as ScriptableObject in Resources
- **ActorData** - ScriptableObject defines actor baseline
- **AiRuleset** - Configuration driving AIBT behavior
- **BattleDefinition** - BattleDefinition object defines enemy rosters

---

## Architectural Patterns

### 1. **Singleton Pattern**
Used for globally-accessible managers:
- `GameFlowManager.instance`
- `BattleManager.instance`
- `UIManager.instance`
- `SaveManager.instance`
- `InputHandler.instance`
- `SoundManager.instance`

**Pro**: Easy global access
**Con**: Potential hard dependencies, testing complexity

### 2. **State Machine Pattern**
Used for actor and battle state management:
- **ActorStateMachine** - Actor animation/action states
- **BattleStateMachine** - Battle progression states
- **Implementation**: Generic `StateMachine<T>` + `IState` interface

**Pro**: Organized behavior flow, clear state transitions
**Con**: Can accumulate states if not managed carefully

### 3. **Event-Driven System**
Decouples systems through C# events/delegates:
- Actor damage events trigger status effects, audio, UI updates
- Input events propagate to various handlers
- Battle lifecycle events notify managers

**Pro**: Loose coupling, extensible
**Con**: Can create hard-to-follow flows if overused

### 4. **Behavior Tree Pattern (AI)**
Hierarchical decision-making for enemy AI:
- `BTNode` base class for tree nodes
- Sequences (all must succeed) and Selectors (first success wins)
- Condition nodes check actor state
- Behavior nodes execute actions

**Pro**: Flexible AI configuration, easy to extend behaviors
**Con**: Can become complex with deep trees

### 5. **Component Pattern**
Actors aggregate multiple subsystems:
- `Actor` container with `StatusManager`, `EffectManager`, `AudioManager`, etc.
- Each subsystem has specific responsibility
- Subsystems can register for actor events

**Pro**: Modular, testable subsystems
**Con**: Actor becomes a large aggregator

### 6. **Strategy Pattern**
Actions as strategies:
- `BaseAction` / `BaseSkill` as abstract strategies
- Different skill implementations provide different execution logic
- Player and AI select which strategy to execute

**Pro**: Easy to add new action types
**Con**: Requires careful design to avoid large action classes

### 7. **Manager Pattern**
Centralized subsystem controllers:
- `BattleManager` orchestrates battle lifecycle
- `UIManager` controls all UI state
- `SaveManager` handles persistence

**Pro**: Single point of control
**Con**: Risk of becoming "god objects"

---

## Key Dependencies

### Critical Dependency Chains

```
GameFlowManager
  ├─ Actor (player)
  ├─ BattleManager
  │   ├─ Actor (player + enemies)
  │   ├─ BattleStateMachine
  │   ├─ UIManager
  │   └─ InputHandler
  ├─ SaveManager
  │   ├─ ActionDatabase
  │   └─ FloorManager
  ├─ RestAreaManager
  ├─ TrainingManager
  │   └─ TimeLockManager
  └─ FloorManager
      └─ StoryBattles (BattleDefinition list)

Actor
  ├─ ActorData (ScriptableObject)
  ├─ ActorStateMachine
  ├─ StatusManager
  ├─ EffectManager
  ├─ AudioManager → SoundManager
  ├─ AIBT → AI Behaviors/Conditions
  ├─ MovementSystem → Rigidbody
  ├─ TargetingSystem
  └─ ActorInventory

BaseAction
  ├─ Actor (caster)
  ├─ DamageInstance
  └─ Targets (determined by TargetingSystem)
```

### Hard Dependencies (Tight Coupling)

⚠️ **Areas of concern**:
1. **UIManager global singleton** - Almost every system references this
2. **BattleManager global singleton** - Tight dependencies from Actor/Action
3. **Actor nested in BattleManager** - Actors can only function in active battle
4. **Direct event subscriptions** - Audio/effects subscribe to Actor events in constructor

---

## Structural Issues & Recommendations

### 🔴 **Critical Issues**

#### 1. **Heavy Singleton Coupling**
- **Problem**: UIManager, BattleManager, GameFlowManager globally accessed
- **Risk**: Hard to test, difficult to refactor, hidden dependencies
- **Solution**: 
  - Consider dependency injection for critical managers
  - Reduce manager scope (UI manager per scene, etc.)
  - Create service locator pattern if singletons continued

#### 2. **Actor is a "God Object"**
- **Problem**: Actor aggregates 8+ subsystems; handles stats, damage, movement, animation
- **Risk**: Large, hard to maintain, unclear responsibilities
- **Current subsystems**: StatusManager, EffectManager, AudioManager, AIBT, MovementSystem, TargetingSystem, ActorInventory, ActorStateMachine
- **Solution**:
  - Separate visual/audio feedback from actor logic
  - Move targeting logic to battle-scoped service
  - Extract animation state management to dedicated controller

#### 3. **UIManager is Too Large**
- **Problem**: 573+ lines, handles action buttons, inventory, stat displays, meters
- **Risk**: Difficult to extend, changes affect many systems
- **Solution**:
  - Split into focused handlers: ActionUIController, StatDisplayController, MeterController
  - Reduce direct references; use event-driven updates
  - Create UI subsystems for each major component

#### 4. **Event Subscription in Constructors**
- **Problem**: StatusManager, EffectManager subscribe to Actor events during construction
- **Risk**: Constructor side effects, hard to debug, timing issues
- **Solution**:
  - Use OnEnable/OnDisable or explicit initialization methods
  - Consider weak event patterns to prevent memory leaks

### 🟡 **Medium Concerns**

#### 5. **Behavior Tree Evaluation Complexity**
- **Problem**: AIBT behaviors are hard-coded for rulesets (OldMan, Ninja, etc.)
- **Risk**: Difficult to extend with new AI types, lots of duplicated logic
- **Solution**:
  - Create a behavior builder or configuration system
  - Use data-driven behavior definitions
  - Implement behavior composition to reduce duplication

#### 6. **Action System Responsibilities Unclear**
- **Problem**: BaseAction handles costs, cooldowns, state transitions, AND specific logic
- **Risk**: Complex inheritance hierarchy, hard to extend
- **Solution**:
  - Separate "execute action" from "manage action state"
  - Create ActionExecutor service
  - Keep BaseAction focused on data, move logic to executors

#### 7. **Missing Input Blocking During Transitions**
- **Problem**: Input can be registered during state transitions, fade screens
- **Risk**: Race conditions, unintended action triggering
- **Solution**:
  - Implement input queue system
  - Add input-enabled flag to UIManager
  - Validate player input state before execution

#### 8. **Status Effect System Limited**
- **Problem**: StatusEffect base class minimal; specific effects hard-coded
- **Risk**: Difficult to add new status types, no effect stacking rules
- **Solution**:
  - Create effect priority/stacking rules
  - Implement more robust effect lifecycle
  - Consider effect composition patterns

### 🟢 **Low Priority Improvements**

#### 9. **Sparse Documentation in Code**
- **Problem**: Few XML doc comments; complex systems not commented
- **Solution**: Add comprehensive doc comments to public APIs

#### 10. **No Error Handling in Key Systems**
- **Problem**: Null references possible in action lookup, actor spawning
- **Solution**: Add validation, null checks, meaningful error messages

#### 11. **Save System Doesn't Back Up Old Saves**
- **Problem**: Overwriting save loses previous progress if corrupted
- **Solution**: Implement save versioning, backup system

---

## Development Guidelines

### Where to Add New Features

#### **New Action/Skill Type**
→ **Location**: `Battle/Actions/Actions/`
```csharp
[CreateAssetMenu(fileName = "YourSkill", menuName = "ScriptableObjects/Action/YourSkill")]
public class YourSkill : BaseSkill
{
    protected override void PerformSpecific(Actor caster, Action onComplete)
    {
        // Custom logic
        onComplete();
    }
}
```

#### **New Enemy AI Behavior**
→ **Location**: `Battle/Actor/AI/Behaviors/`
1. Extend `BTNode`
2. Override `Evaluate()` method
3. Add to ruleset list in `AIBT.Reset()`
```csharp
public class YourBehavior : BTNode
{
    public override bool Evaluate()
    {
        // Check condition and execute action
        return true; // success
    }
}
```

#### **New Status Effect**
→ **Location**: `Battle/Components/Status/EffectLogic/`
1. Create new Status class extending `BaseStatus`
2. Implement Apply(), Tick(), Remove()
3. Register in actions or other systems that apply it

#### **New Game Mode (UI Flow)**
→ **Location**: `Game/`
1. Create new Manager class
2. Register with `GameFlowManager.SetMode()`
3. Add mode to GameMode enum

#### **New UI Feature**
→ **Location**: `UI/`
1. Create dedicated handler class
2. Subscribe to relevant events
3. Add to UIManager or create separate manager

#### **New Save/Load Data**
→ **Modify**: `Save/SaveData.cs`
1. Add field to SaveData struct
2. Update serialization in SaveManager
3. Load/set in GameFlowManager.Start()

---

### Modification Guidelines

#### **When to Extend vs. Modify**

| Scenario | Action |
|----------|--------|
| Adding new action type | Extend `BaseSkill` in new file |
| Changing action execution | Modify `Actor.ExecuteAction()` or create executor service |
| Adding actor stat | Extend `ActorData`, update `Actor.Update()` |
| Changing damage calculation | Extend `DamageInstance` or modify `Actor.ApplyDamageInstance()` |
| Adding UI element | Create new handler, subscribe to events |
| Changing battle flow | Modify `BattleStateMachine` or `BattleManager.Enter()` |
| New status condition | Extend `BaseStatus` in new file |
| Modifying AI behavior | Extend `BTNode` behavior class |

---

### System Extension Priorities

**Recommended to extend/improve** (minimal breakage):
1. **Action System** - Well-structured inheritance
2. **Status Effects** - Clear interface, additive only
3. **AI Behaviors** - Modular, new behaviors don't affect existing

**Dangerous to modify** (high coupling):
1. **Actor class** - Too many dependencies
2. **UIManager** - Referenced everywhere
3. **BattleManager** - Core orchestration

**Need refactoring before heavy modification**:
1. **GameFlowManager** - Too many responsibilities
2. **BaseAction** - Mixing data and behavior
3. **ActorStateMachine** - Growing state collection

---

### Code Structure Best Practices

1. **Keep managers focused** - One manager, one concern
2. **Use events for notifications** - Avoid bidirectional dependencies
3. **ScriptableObject for config** - ActorData, BattleDefinition model
4. **Separate data from logic** - DamageInstance vs. damage application
5. **Methods should fit screen** - Refactor if >50 lines
6. **States should fit screen** - Consider refactoring ActorStateMachine states
7. **Document public APIs** - Especially for managers and abstract classes
8. **Validate inputs** - Null checks at system boundaries

---

### Testing Considerations

**Testable systems**:
- Damage calculation logic (isolated)
- Behavior tree evaluation (create test actor)
- Status effect application (unit test subsystem)
- Movement physics (with mock actor)

**Hard to test**:
- UI interaction (global UIManager)
- BattleManager orchestration (complex initialization)
- Save/load (file I/O)
- Actor component methods (many dependencies)

**Improvement idea**: Create a "TestBattle" mode that spawns actors without full initialization for behavior testing.

---

## Summary of Key Takeaways

**Strengths**:
✅ Clear separation between Battle, Game Flow, and UI layers
✅ State machine pattern effective for actor and battle states
✅ Behavior tree AI is flexible and extensible
✅ ScriptableObject-driven configuration
✅ Event system provides good decoupling for some systems

**Weaknesses**:
❌ Heavy singleton coupling limits testability
❌ Actor class too large and does too much
❌ UIManager is a god object with many responsibilities
❌ Some tight coupling between action execution and actor state
❌ Limited input validation and error handling

**Priority Improvements**:
1. Reduce UIManager responsibilities (split into focused handlers)
2. Refactor Actor to extract subsystems (movement, animation, damage)
3. Implement dependency injection for key managers
4. Add more comprehensive error handling
5. Document complex systems and decision logic

---

**Document Version**: 1.0  
**Last Updated**: March 14, 2026  
**Scope**: Assets/Scripts only
