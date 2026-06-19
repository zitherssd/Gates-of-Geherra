# Gates of Gehera — Project Overview

## Genre

Roguelite beat-em-up / dungeon-crawler.  
Real-time action combat with a floor-based progression structure and persistent run state between scenes.

---

## Core Gameplay Loop

1. **Rest Area** — Player starts (or returns after battle) in the rest area inside CaveScene.
   - Can view stats (`StatPanelUI`), manage skills (`Skills UI`), view items, or trigger training.
   - Buttons: Descend (next floor boss), Explore (random fight), Train, Skills, Items.

2. **Battle** — A `BattleDefinition` ScriptableObject defines enemies, arena, spawn positions, and reward pools.
   - Player and enemies are placed in a 3D arena.
   - Real-time combat using action buttons (drag-drop UI onto four action containers).
   - Combat ends when all enemies OR the player are dead.

3. **Post-Battle Reward** — `BattleEndState` awards:
   - Optional item from a `RewardItemsPool` (random pick, probability-gated).
   - Skill selection: 3 random `BaseAction` ScriptableObjects drawn from the `RewardPool`; player picks one to add to their action library.
   - Auto-save to current slot.

4. **Return to Rest** — Scene either returns to `CaveScene` rest mode or loads a `ReturnScene` (dedicated arena scene flow).

5. **Death** — Save deleted; player is returned to `TitleScene`.

---

## Win Conditions

- **Per-battle win**: All `EnemyActors` reach `DeathState`.
- **Run win**: [UNVERIFIED] — no explicit "run complete" condition found in code. The `FloorManager` has 5 story battles defined (1st–5th Floor), but no victory screen or run-end state was identified.

---

## Loss Conditions

- All `PlayerActors` reach `DeathState`.
- On loss: current save slot is deleted (`SaveManager.DeleteSave`), 2-second slowdown plays, then `TitleScene` loads.

---

## Progression Systems

### Floor Progression
- `FloorManager.currentFloor` (delegated to `GameSession.currentFloor`).
- `ProgressToNextFloor()` increments floor, triggers a story battle from `FloorManager.StoryBattles` list (5 floors defined).
- `QuickFight()` picks a random battle from `FloorManager.RandomBattles` (8 defined).

### Skill Acquisition
- After each battle win, `SkillGenerator` draws 3 random skills from `Resources/Actions/Droptable/`.
- Player selects one; it is added to `ActorRuntime.actions`.
- Known droptable skills: CageKick, Charge, EnhancedPunch, Fire Jutsu, Fire Pillar, Fireball, Focus, Kick_Forward, Kick_Sweep, Shuriken_Big, Shuriken_Small, Teleport_Omae.

### Training
- `TrainingManager.TriggerTraining()` starts a real-time timer (10–15 seconds scaling with usage).
- On timer completion: `AwardRandomStat()` is called (implementation in `TrainingManager`).
- Training is time-locked; can only train once per timer cycle.

### Stats
- 4 primary stats: **Strength** (maxPosture bonus), **Agility** (maxStamina bonus ×2), **Mind** (maxBuildup bonus), **Spirit** (purpose [UNVERIFIED]).
- Stats are rolled 3d6 on new game (`GameSession.RollStats`).
- Stats are saved/loaded via `SaveData`.

---

## Meta Systems

### Save System
- JSON file per slot at `Application.persistentDataPath/save_slot_{n}.json`.
- WebGL fallback: `PlayerPrefs`.
- Saves: floor, trainings done, timelocks, HP bars, stats, stamina, posture, buildup, items, action layout.

### Timelocks
- `TimeLock` — real-world time-gated actions stored on `GameSession.timelocks`.
- Used for Training and QuickFight cooldowns.
- Persisted in save data.

### Loadout Persistence
- `ActionSlotSaveData` records which action is in which UI container slot.
- Persisted via `GameSession.Loadout` so layout survives scene transitions without disk I/O.

---

## Player Goals

- Fight through 5 story floors.
- Accumulate skills from post-battle rewards to build a viable combat kit.
- Train between battles to improve stats.
- Survive — death is permanent (save deleted).
