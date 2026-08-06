# Progression System

> **Last verified:** 2026-08-03

## Purpose

Manages the player's advancement through floors, skill accumulation, stat growth, and run persistence.

---

## Responsibilities

- Track current floor and advance through 5 story battles.
- Provide random ("explore") battles between floor milestones.
- Award skills post-battle from a droptable.
- Provide a real-time Training mechanic that awards stat points.
- Persist all run state across scene transitions.

---

## Main Scripts

| Script | Role |
|--------|------|
| `FloorManager.cs` | Floor counter, story/random battle dispatch |
| `SkillGenerator.cs` | Post-battle skill draw and UI card presentation |
| `TrainingManager.cs` | Training timer, stat award, time-lock gating |
| `GameSession.cs` | Owns all persistent run-state; DontDestroyOnLoad |
| `GameFlowManager.cs` | Delegates `trainingsDone`, `timelocks` to GameSession; orchestrates mode transitions |

> **No dedicated `Progression` folder** — run progression lives across `Game/GameSession.cs`, `Save/SaveData.cs`, and `Battle/Manager/BattleManager.cs` (see `Architecture.md`).

---

## Data Sources

- `FloorManager.StoryBattles : List<BattleDefinition>` — 5 ordered battles (1st–5th floor)
- `FloorManager.RandomBattles : List<BattleDefinition>` — 8 random battles
- `SkillGenerator.resourcePath = "Actions/Droptable"` — `Resources.LoadAll<BaseAction>()` on Start
- `BattleDefinition.RewardPool` — optional curated reward pool per battle
- `GameSession.currentFloor`, `trainingsDone`, `trainingsDoneThisFloor`, `timelocks`

---

## Events

| Event | Source | Effect |
|-------|--------|--------|
| `ItemEventBus.OnNewFloor` | `FloorManager.ProgressToNextFloor()` | Trinkets with OnNewFloor trigger fire |
| `ItemEventBus.OnNewBattle` | `BattleStartState.Enter()` | Trinkets with OnNewBattle trigger fire |
| `SkillGenerator` callback | `onSelectionComplete` lambda | Triggers save, re-enables battle skills, returns to rest |

---

## Floor Progression Flow

```
Player clicks "Descend"
  → FloorManager.ProgressToNextFloor()
    → ItemEventBus.Raise(OnNewFloor)
    → currentFloor++ 
    → Fade out
    → GameFlowManager.EnterBattle(StoryBattles[floor-1])
      → if BattleDefinition.arena != None: load arena scene
      → else: BattleManager.Enter() in current scene
  
Battle ends (victory)
  → BattleEndState.Enter()
    → Award items (probability check against RewardItemsPools)
    → if RewardPool.Actions.Count > 2:
        SkillGenerator.DrawSkillsFromSelection([3 random], callback)
          → Player selects one → added to ActorRuntime.actions
          → callback: save, disable battle skills, return to rest
    → else: save, return to rest immediately
```

---

## Training Flow

```
Player clicks "Train"
  → TrainingManager.TriggerTraining()
    → increment trainingsDone
    → calculate wait (10s + trainingsDoneThisFloor seconds, max 15s)
    → TimeLockManager.Add("Training", timespan)
    → increment trainingsDoneThisFloor

Each frame in TrainingManager.Update():
  → check TimeLockManager.Get("Training")
  → if done: AwardRandomStat(), TimeLockManager.Remove("Training")
  → if lock present: disable TrainButton
```

---

## External Dependencies

- `BattleManager` — receives battle to start
- `UIManager` — fade, show/hide UI
- `SoundManager` — music cues on floor advance
- `SaveManager` — auto-save after battle and skill selection
- `TimeLockManager` — real-time timer enforcement

---

## Extension Points

- Add a new floor: extend `FloorManager.StoryBattles` list with a new `BattleDefinition`.
- Add random encounters: extend `FloorManager.RandomBattles`.
- Add more droptable skills: place new `BaseAction` assets in `Resources/Actions/Droptable/`.
- Change training rewards: edit `TrainingManager.AwardRandomStat()`.
- Change training timer scaling: edit the `switch(trainingsDone)` block.

---

## Risks

- **5-floor hard limit**: `StoryBattles` array is indexed as `[floor-1]` — index out of range if `currentFloor > StoryBattles.Count`.
- **No run-complete state**: After the 5th floor, no victory condition or post-game flow is implemented.
- **Training award verified 2026-08-03**: `AwardRandomStat()` rolls `Random.Range(0, 4)` and grants +1 to STR/AGI/MND/SPT (case 3 = SPT).
- **Droptable loaded once at Start**: `allActions` is populated in `SkillGenerator.Start()`; changes to `Resources/Actions/Droptable/` at runtime have no effect.
