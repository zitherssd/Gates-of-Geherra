# Gates of Gehera — System Dependency Graph

## Text Tree

```
GameSession (DontDestroyOnLoad)
├── SaveManager (DontDestroyOnLoad)
│   └── ActionDatabase
├── GameFlowManager (CaveScene)
│   ├── BattleManager
│   ├── RestAreaManager
│   ├── UIManager
│   └── FloorManager
├── ArenaBootstrapper (Arena Scenes)
│   ├── BattleManager
│   └── UIManager
└── PlayerRuntime
    ├── Actor (body — spawned per scene)
    │   ├── ActorStateMachine
    │   │   └── States (Idle, Acting, Stagger, Death, Block, ...)
    │   ├── ActorInventory
    │   │   └── ItemEventBus (static)
    │   ├── EffectManager
    │   ├── StatusManager
    │   ├── MovementSystem (NavMeshAgent)
    │   └── TargetingSystem

BattleManager
├── BattleStateMachine
│   ├── BattleStartState
│   │   └── ItemEventBus (OnNewBattle)
│   ├── BattleActiveState
│   │   ├── SlowdownManager
│   │   ├── UIManager
│   │   └── CameraManager
│   └── BattleEndState
│       ├── SlowdownManager
│       ├── UIManager
│       ├── SoundManager
│       ├── SaveManager
│       ├── SkillGenerator
│       └── GameSession
├── Actor[] PlayerActors
└── Actor[] EnemyActors (spawned from ActorDefinition)

FloorManager
├── GameSession (currentFloor)
├── BattleDefinition[] StoryBattles
├── BattleDefinition[] RandomBattles
└── GameFlowManager.EnterBattle()

SkillGenerator
├── Resources/Actions/Droptable/ (runtime load)
└── UIManager (skill cards)

UIManager
├── ActionButtonBattle → Actor.UseAction()
├── DropSlot (drag-and-drop containers)
├── GameSession.Loadout (layout persistence)
└── StatPanelUI

Actor (generic)
├── BaseAction[] (from ActorRuntime.actions)
│   ├── AttackSkill → EffectManager + Intersections
│   ├── GenericSkill → IEffect[] (Effects system)
│   ├── ProjectileAttack → ProjectileHandler prefab
│   ├── Dodge → MovementSystem
│   ├── Block → BlockState
│   ├── Charge → MovementSystem
│   ├── Jump → AirNeutralState
│   └── MoveAction → MoveState + MovementSystem
├── AIBT → BTNode behaviors → Actor.UseAction()
├── StatusManager → BaseStatus[] (ticked per frame)
└── ActorInventory
    ├── BaseConsumable → IItemEffect[]
    └── BaseTrinket → ItemEventBus subscriptions

CameraManager
├── GameSession.OnPlayerSpawned
├── Actor.target.GetWeightedAverageEnemyPosition()
└── SlowdownManager (SlowTrack flag)

SlowdownManager
├── Time.timeScale (Unity global)
├── SlowdownInputController (hold input)
└── SlowdownVisualEffect (post-process)

SoundManager (standalone singleton)

InputHandler (standalone singleton)
├── Unity Input System (InputActionReference)
└── ActionButtonBattle / ActionButtonHandler

TimeLockManager (static)
├── TrainingManager (Training lock)
└── FloorManager (QuickFight lock)
```

---

## Singleton Relationships

| Singleton | Type | DontDestroyOnLoad |
|-----------|------|-------------------|
| `GameSession.Instance` | MonoBehaviour | Yes |
| `SaveManager.instance` | MonoBehaviour | Yes |
| `BattleManager.instance` | MonoBehaviour | No (scene-local) |
| `GameFlowManager.instance` | MonoBehaviour | No (scene-local) |
| `UIManager.instance` | MonoBehaviour | No (scene-local) |
| `CameraManager.instance` | MonoBehaviour | No (scene-local) |
| `SlowdownManager.instance` | MonoBehaviour | No (scene-local) |
| `SoundManager.instance` | MonoBehaviour | No (scene-local) |
| `FloorManager.instance` | MonoBehaviour | No (scene-local) |
| `SkillGenerator.instance` | MonoBehaviour | No (scene-local) |
| `TrainingManager.instance` | MonoBehaviour | No (scene-local) |
| `InputHandler.instance` | MonoBehaviour | No (scene-local) |
| `ItemEventBus` | Static class | Yes (static state) |
| `TimeLockManager` | Static class | Yes (static state) |

---

## Event Dependencies

| Event | Producer | Consumer(s) |
|-------|---------|------------|
| `GameSession.OnPlayerSpawned` | `ArenaBootstrapper`, `GameFlowManager` | `CameraManager` |
| `Actor.OnBeforeTakeDamage` | `Actor.ApplyDamageInstance()` | Items (DamageMultiplier) |
| `Actor.OnAfterTakeDamage` | `Actor.ApplyDamageInstance()` | UI/audio [UNVERIFIED listeners] |
| `Actor.OnActionUsed` | `Actor.UseAction()` | AI, UI [UNVERIFIED listeners] |
| `BaseAction.OnActionEnded` | `BaseAction.EndAction()` | `Actor.OnActionEnded()` → state transition |
| `ActorRuntime.OnDeath` | `ActorRuntime.DealDamage()` | [UNVERIFIED] |
| `UIManager.OnHideUI` | `UIManager.HideUI()` | [UNVERIFIED listeners] |
| `UIManager.OnShowUI` | `UIManager.ShowUI()` | [UNVERIFIED listeners] |
| `BattleActiveState.FinalHitDealt` | `BattleActiveState.Update()` | [UNVERIFIED listeners] |
| `SlowdownManager.OnStateSlowdownStart/End` | SlowdownManager | SlowdownVisualEffect |
| `ItemEventBus.OnNewBattle` | `BattleStartState` | BaseTrinket effects |
| `ItemEventBus.OnNewFloor` | `FloorManager` | BaseTrinket effects |
| `ItemEventBus.OnDamageTaken` | `Actor.ApplyDamageInstance()` | BaseTrinket effects |

---

## Scene-to-Scene Data Flow

```
TitleScene
  └─ SaveManager (DDOL) ──────────────────────────────────────────────┐
                                                                        │
CaveScene (GameFlowManager)                                            │
  └─ GameSession (DDOL, auto-created) ◄──── SaveManager restores ─────┘
       ├── PlayerRuntime (owned by GameSession)
       ├── currentFloor / trainingsDone / timelocks
       ├── Loadout (action layout)
       └── PendingBattle / ReturnScene (arena hand-off)

Arena Scene (ArenaBootstrapper)
  └─ GameSession.PlayerRuntime ──► Actor.Bind() ──► Player body
  └─ GameSession.PendingBattle ──► BattleManager.Enter()
  └─ GameSession.ReturnScene ────► SceneManager.LoadScene() on battle end
```
