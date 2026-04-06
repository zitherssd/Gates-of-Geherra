# System Context: Gates-of-Gehera

This document outlines the high-level architecture of the Unity game "Gates-of-Gehera", based on an analysis of the `Assets/Scripts` folder.

## Core Architectural Patterns

The game's architecture is based on a **manager-based, state machine-driven** design. A central `GameFlowManager` controls the overall game state, delegating to specialized managers for specific game modes like battles or exploration.

The character representation follows a **component-based** approach, with an `Actor` class at its core, composed of data (`ActorData`), a state machine (`ActorStateMachine`), and other components.

## Key Components

### 1. Game Flow

- **`GameFlowManager.cs`**: The heart of the game's architecture. It's a singleton that manages the main game loop and transitions between different game states (`GameMode` enum), such as `RestArea`, `Battle`, and `Training`. It's also responsible for initializing the player character, loading and saving game data via the `SaveManager`.

### 2. Battle System

- **`BattleManager.cs`**: This manager orchestrates battle sequences. It's responsible for:
    - Setting up the battle environment based on a `BattleDefinition`.
    - Spawning enemy `Actor`s and positioning the player `Actor`.
    - Managing the battle's lifecycle through a `BattleStateMachine`.
    - Triggering events for the start and end of a battle.

- **`Actor`**: Represents any character in a battle (player or enemy). It's a `MonoBehaviour` that holds a reference to its `ActorData` and has a state machine to control its behavior.

- **`ActorData`**: A `ScriptableObject` that holds the stats and attributes of an actor (e.g., Strength, Agility, Mind, Spirit, actions). This allows for easy creation of different types of characters.

- **`ActorStateMachine`**: Manages the state of an `Actor` during battle (e.g., Idle, Attacking, Hurt).

- **Enemy AI (`AIBT.cs`, `BTNode.cs`)**: Enemies use a Behavior Tree architecture for tactical decision-making. AI Rulesets (like `TacticalFlanker`) combine nodes to coordinate behaviors, including:
    - **Condition Nodes**: `HesitateCondition` (adds delay before attacking), `GlobalAttackTokenCondition` (prevents enemies from attacking simultaneously).
    - **Action Nodes**: `ApproachBehavior`, `CircleApproachBehavior` (intelligently surrounding the player while respecting personal space), `BlockBehavior`, etc.

- **`BaseAction.cs` & `BaseSkill.cs`**: These are likely the base classes for defining all character abilities and actions within the battle system.

### 3. Dungeon Crawling (Inferred)

- **`CrawlerManager.cs` & `FloorManager.cs`**: The presence of these files suggests a dungeon crawler or exploration mechanic. `CrawlerManager` is likely intended to manage the player's progression through a dungeon, while `FloorManager` would be responsible for generating or managing individual floors or levels. `CrawlerManager.cs` is currently not implemented.

### 4. UI System

- **`UIManager.cs`**: A central manager for the game's user interface. It handles UI elements like action buttons, joystick controls, and targeting indicators. The UI system appears to be decoupled from the core game logic, with managers like `ActionUIManager` handling specific UI domains.

### 5. Save System

- **`SaveManager.cs`** (inferred from `GameFlowManager.cs`): This component is responsible for saving and loading the game's state, including player progress and character data. `GameFlowManager` interacts with it to handle player data at the start of the game.

## Overall Flow

1.  The game starts with `GameFlowManager` initializing the player `Actor`, either by creating a new character or loading data from the `SaveManager`.
2.  `GameFlowManager` then sets the game mode, for example to `RestArea`, enabling the `RestAreaManager`.
3.  When a battle is triggered (e.g., by the player interacting with an element in the world), `GameFlowManager` transitions the game to `Battle` mode.
4.  `GameFlowManager` calls `BattleManager.Enter()` to start the battle.
5.  `BattleManager` sets up the scene, spawns enemies, and starts its `BattleStateMachine`.
6.  The `BattleStateMachine` controls the turn-based or real-time flow of the battle, with `Actor`s performing actions.
7.  When the battle ends, `BattleManager` informs `GameFlowManager`, which then transitions the game to another state (e.g., back to `RestArea` or to a victory screen).

This architecture provides a clear separation of concerns and allows for modular development of different game features.
