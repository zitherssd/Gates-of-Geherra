# Gates of Gehera — Runtime Architecture

## Bootstrap Scene

**TitleScene** (Build Index 0) is the game entry point.

`SaveManager` and `GameSession` are the only two persistent objects that survive across all scene loads.

---

## Startup Sequence

### TitleScene Start

```
Application Launch
  → TitleScene loads
  → SaveManager.Awake()
      → DontDestroyOnLoad(this)
      → instance = this
  → SaveManager.Start()
      → actionDatabase.Initialize()          // builds GUID→Action lookup
  → MainMenuController.Start()
      → LeanTween title animation
      → Show Start/Sandbox buttons
```

### New Game: TitleScene → CaveScene

```
Player taps "Start" → enters name
  → MainMenuController.StartNewGameForReal()
    → SaveManager.newGamePlayerName = inputField.text
    → SceneManager.LoadScene("CaveScene")

CaveScene Loads
  → All scene managers Awake() (UIManager, BattleManager, SlowdownManager, etc.)
  → GameFlowManager.Start()
    → GameSession.Instance  [auto-created if not present; GetOrCreate pattern]
    → session.PlayerRuntime == null AND no save exists:
        session.StartNewRun(playerName)
          → Resources.Load<ActorDefinition>("Actors/MC")
          → ActorRuntime(mc) → ResetToDefinition() → clone actions/items
          → RollStats() [3d6 each for STR, AGI, MND, SPT]
    → SetupPlayerBody()
        → EnsurePlayerBody() → pre-placed PlayerBattler or spawn from playerPrefab
        → Actor.Bind(session.PlayerRuntime) [player body adopts persisted runtime]
        → BindPlayerBody() → [UNVERIFIED — registers player with managers]
    → SetMode(GameMode.RestArea)
        → RestAreaManager.Enter()
        → UIManager.Fade(false) → show UI
    → UIManager.InitializePlayerActionButtonPrefabs(runtime.actions, loadout)
    → UIManager.DisableBattleSkills()
```

### Load Existing Save: TitleScene → CaveScene

```
GameFlowManager.Start()
  → session.PlayerRuntime == null AND SaveManager.SlotExists(slot):
      save = SaveManager.LoadFromSlot(slot)
          → RestoreFloor/TrainingData to FloorManager + GameFlowManager
      EnsurePlayerBody()
      save.LoadActor(playerActor)
          → ActorSaveData.LoadInto(save, actor)
          → ActionDatabase resolves saved GUIDs → restore Runtime.actions
      session.AdoptPlayerRuntime(actor.Runtime)
  → [same rest-mode setup as new game]
```

### Entering a Battle (In-Scene / Legacy)

```
[User clicks Descend or Explore]
  → FloorManager → GameFlowManager.EnterBattle(battleDefinition, onBattleEnd)
    → if battleDefinition.arena == Arena.None:
        BattleManager.Enter(battleDef, onBattleEnd)
          → position player at SpawnGroup or Level_ marker
          → Actor.Runtime.Refresh() [reset posture/stamina/buildup, NOT stats]
          → SpawnEnemies() [Instantiate enemyPrefab, SetDefinition, Init, Spawn]
          → UIManager.Fade(false)
          → CameraManager.ResetForNewBattle(anchor)
          → WaitForSeconds(2f) → SoundManager.PlayMusic(null)
          → BattleStateMachine.Initialize(startState)
            → BattleStartState.Enter()
                → enemies.TransitionTo<IdleState>()
                → UIManager.EnableUI() + EnableBattleSkills()
                → ItemEventBus.Raise(OnNewBattle)
                → TransitionTo(activeState)
```

### Entering a Battle (Arena Scene)

```
GameFlowManager.EnterBattle(battleDef, onBattleEnd)
  → battleDef.arena != Arena.None:
      GameSession.PendingBattle = battleDef
      GameSession.ReturnScene = current scene name
      GameSession.Loadout = UIManager.GetCurrentLoadout()  [persist layout]
      SceneManager.LoadScene(arenaSceneName)

Arena Scene Loads
  → ArenaBootstrapper.Start()
    → session.PendingBattle (or debugBattle)
    → SpawnAndBindPlayer(spawnGroup)
        → Instantiate(playerPrefab, spawnPos, spawnRot)
        → player.Bind(session.PlayerRuntime)
        → session.NotifyPlayerSpawned(player) → CameraManager.SetPlayer(player)
    → BattleManager.PlayerActors = [player]
    → UIManager.InitializePlayerActionButtonPrefabs(runtime.actions, session.Loadout)
    → UIManager.DisableBattleSkills()
    → session.PendingBattle = null
    → BattleManager.Enter(battle, null)
```

### Battle End (Victory)

```
BattleActiveState.Update()
  → all enemies dead:
      SlowdownManager.TriggerTemporarySlowdown(2f)
      UIManager.DisableUI()
      CameraManager.SlowTrack = true
      exitStep = true
  → next frame, all enemies in DeathState:
      TransitionTo(endState)

BattleEndState.Enter()
  → SlowdownManager.ExitStateSlowdown()
  → SoundManager.FadeOutMusic()
  → UIManager.Fade(true, callback)
    [callback]:
    → UIManager.HideUI()
    → Award items from RewardItemsPools (random, probability-gated)
    → if RewardPool.Actions.Count > 2:
        SkillGenerator.DrawSkillsFromSelection([3 random], callback)
          [selection callback]:
          → action added to PlayerActors[0].Runtime.actions
          → UIManager.InitializePlayerActionButtonPrefabs(actions)
          → destroy cards
          → UIManager.DisableBattleSkills()
          → SaveManager.SaveToSlot(slot)
          → BattleManager.TriggerBattleEnd()
          → ReturnAfterBattle()
              → GameSession.Loadout = UIManager.GetCurrentLoadout()
              → if ReturnScene set: SceneManager.LoadScene(ReturnScene)
              → else: GameFlowManager.SetMode(RestArea)

BattleEnd (Death)
  → BattleActiveState detects all players dead:
      SlowdownManager.TriggerTemporarySlowdown(2f)
      exitStep = true
  → all players in DeathState:
      SaveManager.DeleteSave(slot)
      WaitForSeconds(2f) → SceneManager.LoadScene("TitleScene")
```

---

## Initialization Order

| Order | Object | Lifecycle Hook |
|-------|--------|---------------|
| 1 | `SaveManager` | `Awake()` → DontDestroyOnLoad |
| 2 | `GameSession` | `Awake()` → DontDestroyOnLoad (or auto-created on first access) |
| 3 | `BattleManager` | `Awake()` → BattleStateMachine created |
| 4 | `UIManager` | `Awake()` → instance set |
| 5 | `CameraManager` | `Awake()` → instance; `OnEnable()` → subscribe OnPlayerSpawned |
| 6 | `SlowdownManager` | `Awake()` → instance |
| 7 | `InputHandler` | `Awake()` → instance; `[DefaultExecutionOrder(-1)]` → before most scripts |
| 8 | `Actor (Player)` | `Awake()` → sub-systems created; `Start()` → AI, EnsureRuntime, InactiveState |
| 9 | `SaveManager.Start()` | `actionDatabase.Initialize()` — must happen before save load |
| 10 | `GameFlowManager.Start()` | Player spawn, save load, rest mode setup |

---

## Persistent Objects (DontDestroyOnLoad)

| Object | Why Persistent |
|--------|---------------|
| `GameSession` | Run state — PlayerRuntime, floor, timelocks, loadout |
| `SaveManager` | Disk I/O + ActionDatabase access between scenes |
| `ItemEventBus` (static) | Global item trigger subscriptions |
| `TimeLockManager` (static) | Real-time training/quickfight locks |

---

## State Machines

### BattleStateMachine

```
[Initial] → BattleStartState.Enter()
    → → BattleActiveState
        → (on enemy death): BattleEndState
        → (on player death): TitleScene (not via state machine — direct SceneManager)
```

### ActorStateMachine (per Actor)

```
Start → InactiveState
BattleStart → IdleState
UseAction → ActingState
  ActingState.OnEnd → IdleState
  ActingState.EndAction(Interrupted) → IdleState
ApplyDamageInstance (posture ≤ 50%) → StaggerState
  StaggerState (duration expires) → [GettingUpState or IdleState — UNVERIFIED]
ApplyDamageInstance (HP = 0) → StaggerState (then DeathState)
Block.PerformSpecific → BlockState
Jump → AirNeutralState
  Land → LandingState → IdleState
MoveAction → MoveState → IdleState
```

---

## Key Execution Flow (Single Frame)

```
Update():
  InputHandler (ExecutionOrder -1) — gesture detection
  Actor.Update():
    → StaminaRegen, PostureRegen, UpdateActionCooldowns
    → statusManager.Tick()
    → ai.Update() [BT evaluation for enemies]
    → effects.UpdatePolygon()
    → if controllable: target.Update()
  ActorStateMachine.Update() → CurrentState.Update()
  BattleManager.Update() → battleStateMachine.Update() → BattleActiveState.Update()
  CameraManager.Update() → compute and lerp camera position
  SlowdownManager (coroutines) → timeScale transitions
  
FixedUpdate():
  Actor.FixedUpdate() → movement.agent.nextPosition = transform.position
```
