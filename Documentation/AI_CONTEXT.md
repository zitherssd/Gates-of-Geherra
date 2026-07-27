# Gates of Gehera — AI Context

> This file is the primary orientation guide for AI agents working on this project.  
> Read this before making any changes. For deeper dives, follow the links to the dedicated system docs.

---

## Project Summary

**Gates of Gehera** is a real-time action roguelite. Players fight through 5 story floors, each culminating in a 1vN arena battle. Between fights, they rest, train stats, and acquire new skills from a post-battle droptable.

- **Genre**: Roguelite beat-em-up / dungeon crawler
- **Platform Target**: Mobile (touch controls) + WebGL
- **Engine**: Unity (URP, Unity Input System, NavMesh, LeanTween)
- **Current State**: In active development — several systems are partially implemented or stubbed

---

## Gameplay Overview

### Core Gameplay Loop

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

### Win/Loss Conditions

- **Per-battle win**: All `EnemyActors` reach `DeathState`.
- **Run win**: [UNVERIFIED] — no explicit "run complete" condition found in code. 5 story battles defined (1st–5th Floor), but no victory screen or run-end state.
- **Loss**: All `PlayerActors` reach `DeathState`. Save deleted, 2-second slowdown plays, then `TitleScene` loads.

### Player Goals

- Fight through 5 story floors.
- Accumulate skills from post-battle rewards to build a viable combat kit.
- Train between battles to improve stats.
- Survive — death is permanent (save deleted).

### Progression Systems

| System | Key Script | Mechanism |
|--------|-----------|-----------|
| **Floor Progression** | `FloorManager` | `ProgressToNextFloor()` increments floor, triggers story battle from `StoryBattles[floor-1]` (5 floors). `QuickFight()` picks random battle from `RandomBattles` (8 defined). |
| **Skill Acquisition** | `SkillGenerator` | Draws 3 random skills from `Resources/Actions/Droptable/` post-battle. Player picks one → added to `ActorRuntime.actions`. |
| **Training** | `TrainingManager` | `TriggerTraining()` starts real-time timer (10–15s scaling with usage). On completion: `AwardRandomStat()`. Time-locked via `TimeLockManager`. |
| **Stats** | `GameSession.RollStats()` | 4 stats: **STR** (maxPosture bonus), **AGI** (maxStamina×2), **MND** (maxBuildup bonus), **SPT** (purpose [UNVERIFIED]). Rolled 3d6 on new game. |

### Meta Systems

| System | Mechanism |
|--------|-----------|
| **Save** | JSON at `persistentDataPath/save_slot_{n}.json` (WebGL: PlayerPrefs). Saves floor, trainings, timelocks, HP bars, stats, stamina, posture, buildup, items, action layout. |
| **Timelocks** | Real-world time-gated actions (`GameSession.timelocks`). Used for Training and QuickFight cooldowns. Persisted in save data. |
| **Loadout** | `ActionSlotSaveData` records action→UI-container mapping. Persisted via `GameSession.Loadout` so layout survives scene transitions without disk I/O. |

---

## Architecture Summary

### The Two-Layer Actor Model
Every combatant has:
1. **`ActorDefinition`** (ScriptableObject) — static template (name, base stats, default actions, AI ruleset)
2. **`ActorRuntime`** (plain C# class) — live mutable state (current HP, stamina, posture, buildup, actions list, items)
3. **`Actor`** (MonoBehaviour) — the scene body that bridges runtime to physics, animation, and systems

The player's runtime lives on **`GameSession`** (DontDestroyOnLoad) and survives scene loads. The player body is re-spawned each scene and bound to the runtime via `Actor.Bind()`. **Enemy bodies own their own runtime** and are discarded when the battle ends.

### The Action/Effect System
All combat behaviors are **ScriptableObjects** (`BaseAction` subclasses). The most flexible is `GenericSkill`, which composes effects from the `IEffect` interface using `[SerializeReference, SubclassSelector]` lists. To add a new ability behavior: implement `IEffect`, then configure it in the Inspector.

### The StateMachine Pattern
Both actors and battles use a state-machine pattern (`IState` interface, `StateMachine` base). Actor states handle the moment-to-moment animation/physics; battle states (`BattleStartState`, `BattleActiveState`, `BattleEndState`) handle the match lifecycle.

### Scene Architecture
- **CaveScene**: The "hub" — rest area + optional legacy in-scene battles. Most singleton managers live here.
- **Arena Scenes** (e.g., Crossing): Dedicated battle arenas bootstrapped by `ArenaBootstrapper`. Player data arrives via `GameSession`, no disk round-trip needed.
- **GameSession + SaveManager** are DontDestroyOnLoad. Everything else is scene-local.

---

## Development Rules (Observed Conventions)

1. **Singletons via `static instance`** — almost all managers use `public static X instance`. Assign in `Awake()`.
2. **DontDestroyOnLoad is explicit** — only `GameSession` and `SaveManager` survive scenes. Do not accidentally add DDOL to scene-local managers.
3. **ScriptableObjects for data** — `ActorDefinition`, `BaseAction`, `BattleDefinition`, `BaseItem`, `BaseStatus` are all SOs. Do not store mutable runtime state on SOs.
4. **`[SerializeReference, SubclassSelector]`** — used for polymorphic effect lists on `GenericSkill`, `BaseConsumable`, `BaseTrinket`. Adding a new effect class makes it automatically available in the Inspector.
5. **`Resources.Load` for runtime assets** — actor definitions, actions, droptable, items loaded via `Resources.Load/LoadAll`. Assets must be inside `Assets/Resources/`.
6. **GUIDs for save references** — every `BaseAction` and `ActorDefinition` has a `guid` field. This is the canonical save key. Always populate GUIDs on new assets.
7. **`Actor.Bind()` vs `Actor.Spawn()`** — `Spawn()` resets runtime to definition (enemies). `Bind()` adopts an existing runtime without reset (player across scene loads). Never call `Spawn()` on the player.
8. **`ItemEventBus` is static** — trinket effects persist in the static dictionary until explicitly `Unequip()`ed. Always call `Unequip()` when removing items or destroying actor bodies.
9. **Multiple HP bars = boss phases** — add multiple `HpBar` entries to `ActorDefinition.hpBars`. The runtime processes them last-to-first.
10. **Animation events drive action timing** — `EnterWindup`, `OnHit`, `EnterRecovery`, `OnEnd` must be keyframed in the Animator on the correct frames. Missing events cause actions to hang indefinitely.

---

## Important Systems (Read Before Modifying)

| System | Docs | Why Important |
|--------|------|--------------|
| Combat | [Systems/Combat.md](Systems/Combat.md) | Core gameplay; wrong changes break all attacks |
| Action/Effect | [repo memory: action-system-architecture.md] | Complex polymorphic system; effects interact |
| Architecture | [Architecture.md](Architecture.md) | Startup order, system deps; wrong init order causes null refs |
| Save System | [Systems/SaveSystem.md](Systems/SaveSystem.md) | Stat field naming bug; no migration system |
| Actor Bind/Spawn | `Actor.cs` | Player must use Bind(); enemies use Spawn() |
| GameSession | `GameSession.cs` | Single source of truth for all run state |
| ItemEventBus | `ItemEventBus.cs` | Static; leaked subscriptions cause ghost effects |

---

## Dangerous Areas

### 1. Actor in Non-Battle Scenes
`Actor.Update()` calls `BattleManager.instance.enabled` — if an `Actor` exists in a scene without a `BattleManager`, this throws immediately. Always check before adding actors to new scenes.

### 2. Save Schema Has No Versioning
Adding fields to `SaveData` or `ActorSaveData` is safe. **Renaming or removing fields silently breaks existing saves.** The stat field bug (`CON` stores Agility data, `AGI` stores Mind data) must be preserved or all saves break.

### 3. UIManager is Scene-Local
`UIManager.instance` is reassigned every scene. Arena scenes must have their own `UIManager`. References cached in scripts must not be stored across scene loads.

### 4. `ItemEventBus` Static Subscriptions Leak
If a `BaseTrinket` is equipped and the actor body is destroyed without calling `Unequip()`, the dead actor remains subscribed. The next `ItemEventBus.Raise()` will call `effect.Eval(deadOwner)` — potential null reference.

### 5. Action Animation Events Are Fragile
If `OnEnd()` is not called in the Animator clip (e.g., clip is interrupted, deleted, or event is missing), the action never calls `EndAction()`, `_currentAction` stays set, and the actor is permanently stuck in `ActingState`.

### 6. Floor Index Out of Bounds
`FloorManager.ProgressToNextFloor()` accesses `StoryBattles[currentFloor - 1]`. If the player somehow reaches floor 6 (e.g. via bug or missing bounds check), this throws `ArgumentOutOfRangeException`.

### 7. Skill ScriptableObjects Are Cloned but Mutable
`ActorRuntime` clones actions from `ActorDefinition.baseActions` via `Instantiate(skill)`. But the clones are runtime instances — fields modified on the clone do not affect the source asset. However, `SkillGenerator` post-battle rewards are direct `ScriptableObject.Instantiate()` copies — same protection applies.

---

## Common Workflows

### Add a New Enemy Type
1. Create a new `ActorDefinition` SO at `Resources/Actors/`.
2. Set name, HP bars, stats, `Controllable = false`, assign `AIRuleset`.
3. Assign starting actions from existing action assets.
4. Add the definition to a `BattleDefinition.enemyActors` list.

### Add a New Skill to the Droptable
1. Create a `BaseAction` subclass asset (e.g., `GenericSkill`) in `Resources/Actions/Droptable/`.
2. Assign a unique `guid` string.
3. Configure effects using `[SerializeReference, SubclassSelector]` in the Inspector.
4. The skill is automatically included in post-battle draws (loaded via `Resources.LoadAll`).

### Add a New Item (Trinket)
1. Create a `BaseTrinket` SO at `Resources/Items/`.
2. Set `Triggers` (which game events activate it) and `Effects` (what it does).
3. Add it to a `BattleDefinition.RewardItemsPools` or `ActorDefinition.startingItems`.

### Add a New Effect Type
1. Create a class implementing `IEffect` (and optionally `IEndableEffect`, `IUpdateableEffect`).
2. No registration needed — `[SerializeReference, SubclassSelector]` auto-discovers it.
3. Use it in `GenericSkill.OnStartEffects`, `GenericSkill.hitWindows[].windowEffects`, `OnEndEffects`, or `OnUpdateEffects`. (Note: `OnHitEffects` is deprecated — use HitWindows instead.)

### Add a New Battle
1. Create a `BattleDefinition` SO at `Resources/Battles/`.
2. Set `enemyActors`, `arena` (use `Arena.None` for in-scene), `Level`, `RewardPool`, `RewardItemsPools`.
3. Add to `FloorManager.StoryBattles` or `RandomBattles` in the scene.

### Add a New Arena Scene
1. Duplicate `Assets/Scenes/ArenaScenes/Crossing.unity`.
2. Ensure `ArenaBootstrapper`, `BattleManager`, `SpawnGroup`, Camera, Light are present.
3. Add to Build Settings.
4. Add `Arena.NewArena` to the `Arena` enum and map it in `GameFlowManager.EnterBattle()`.

---

## AI Instructions

### Before Making Code Changes
- Check `Docs/Architecture.md` for initialization order and system dependency graph.
- Check repo memory `action-system-architecture.md` for deep action/effect system details.
- Never store scene references in `GameSession` or `SaveManager`.

### When Adding ScriptableObjects
- Always populate the `guid` field with a unique string (use `System.Guid.NewGuid().ToString()`).
- Place under `Assets/Resources/` if they need to be loaded at runtime via `Resources.Load`.

### When Modifying Save Data
- Only add new fields — never rename or remove.
- Be aware of the `CON/AGI` field naming inversion in `ActorSaveData`.
- Test load of old saves after schema changes.

### When Adding New Actor States
- Register in `ActorStateMachine.Awake()`.
- Implement `IState` with `Enter()`, `Exit()`, `Update()`.
- Do not call `TransitionTo<X>()` from within a state's own `Enter()` without a guard — it causes re-entrancy.

### When Modifying Battle Flow
- `BattleEndState.Enter()` is the critical money path — it handles save, reward, and scene transition. Be extremely careful with async/coroutine ordering here.
- All coroutines use `battleManager.StartCoroutine()` (not `this`) because states are not MonoBehaviours.

### Testing New Features
- Use `ArenaBootstrapper.debugBattle` to test a specific battle in a dedicated arena scene without running through floors.
- The `SandboxScene` is available for free-form testing.
- `DebugScene` is disabled in build settings.
