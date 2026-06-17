# Scene Persistence — Implementation Plan (master)

> Status: PLANNED, not started. No code until green-lit.
> Companion deferral docs in this folder:
> - BattleUI-Persistent-Canvas.md
> - ArenaScene-Direct-Testing.md
> - PlayerActors-Single-Player-Refactor.md

## Decisions (locked)
- **Persist DATA, respawn BODIES.** Player MODEL (`ActorRuntime`, no scene deps) persists in
  RAM; player BODY (Actor prefab w/ NavMeshAgent) spawns per scene and binds to the model.
- **GameSession** (NEW, `DontDestroyOnLoad`, data only) = single source of truth for a run.
- **GameFlowManager does NOT persist** → becomes the RestScene bootstrapper. No persistent
  orchestrator; transitions = write GameSession + `SceneManager.LoadScene`, triggered locally.
- **Player spawns from one prefab** at a per-scene `PlayerSpawnPoint` (inside a SpawnGroup).
- **Spawn locations live in the scene** as `SpawnGroup`s; `BattleDefinition` only selects which.
- **Save = explicit save points only** (new game, floor cleared, training). No disk on scene load.
- **Spawn-group key = `enum SpawnGroupId`**; arena = `enum Arena`.
- **OnPlayerSpawned** event = scene-local for now (Camera subscribes). Promote to a persistent
  hub only when UI becomes persistent (deferred doc).

## New / changed types
- `GameSession` (new): player `ActorRuntime`, currentFloor, trainingsDone(ThisFloor),
  timelocks, currentSaveSlot, `PendingBattle` (BattleDefinition). Methods: `StartNewRun()`,
  `ContinueRun(slot)`, `ToSaveData()/LoadFrom(save)`. Data only — **no scene references**.
- `Arena` (enum) + `ArenaCatalog` (enum → scene name, one place).
- `SpawnGroupId` (enum).
- `SpawnGroup` (MonoBehaviour, scene): `SpawnGroupId id; Transform playerSpawn; Transform[] enemySpawns;`
- `Actor.Bind(ActorRuntime runtime)`: attach body to persistent model; clear/rewire `OnDeath`
  and per-body event subs.
- `BattleDefinition`: ADD `Arena arena`, `SpawnGroupId spawnGroup`; REMOVE `playerStartPosition`
  (Transform — always null on an SO asset) and `enemyStartPosition`; keep `enemyActors` + rewards.

---

## Ordered build sequence (each step keeps the project compiling/testable)

### Step 1 — Introduce GameSession as a data holder (additive)
- Create `GameSession` (DDOL) with run data + `StartNewRun()` / `ContinueRun(slot)`
  (lift the duplicated MC-template + RollNewstats logic out of `GameFlowManager.Start()`).
- `GameFlowManager` delegates run-data reads/writes to `GameSession` (player still
  inspector-assigned + hydrated in place for now).
- Move `trainingsDone`, `trainingsDoneThisFloor`, `timelocks` onto `GameSession`;
  `TrainingManager` reads them there.
- **Test:** RestScene behaves exactly as before. Fixes point 9 (dup player creation).

### Step 2 — Spawn the player from a prefab in RestScene (single scene first)
- Add `Actor.Bind(runtime)` + `OnPlayerSpawned` (scene-local event source).
- Add `PlayerSpawnPoint`/`SpawnGroup` marker to RestScene; bootstrapper spawns the Player
  prefab there and binds to `GameSession` player runtime.
- Repoint `RestAreaManager` and `CameraManager` from `GameFlowManager.instance.playerActor`
  to the spawned ref via `OnPlayerSpawned`.
- **Test:** RestScene with a spawned+bound player; camera follows; stats correct.

### Step 3 — SpawnGroups + enums + BattleDefinition migration
- Add `Arena` + `SpawnGroupId` enums, `ArenaCatalog`.
- Migrate `BattleDefinition` (add arena + spawnGroup, remove the two start-position fields).
  Update existing Battle assets in-editor.
- Update enemy spawning to read a chosen `SpawnGroup` (replaces `GetChild(i+1)` and the
  "unholy line"). Player + enemies share the spawn+bind path.
- **Test:** A battle in the current scene spawns everyone at a SpawnGroup.

### Step 4 — Multi-scene flow (the actual goal)
- `ArenaBootstrapper`: on load, read `GameSession.PendingBattle`, find the `SpawnGroup`,
  spawn player + enemies, start `BattleStateMachine`.
- Transition out of rest: set `GameSession.PendingBattle`, `LoadScene(ArenaCatalog[arena])`.
- `BattleEndState` → `LoadScene(rest)` (already loads scenes; just route through catalog).
- **Test:** Rest → Arena → Rest loop; player stats/inventory persist across loads with no
  disk round-trip.

### Step 5 — Cleanup
- Delete dead `BattleManager.SetupBattleWithEnemies()` + commented UIManager lines (point 4).
- Collapse the `GameFlowManager.SetMode` switch (scene loads replace mode toggling).
- Confirm `CameraManager` no longer assumes `PlayerActors[0]` at wake (uses OnPlayerSpawned).

---

## Risks / gotchas
- **NavMeshAgent**: spawning per scene binds to that scene's baked navmesh — the reason bodies
  must NOT be DDOL.
- **Event lifetime**: `Bind()` must clear old `OnDeath`/subscribers (old body dies with old scene).
- **GameSession stays body-free**: it raises/forwards notifications but never stores a scene ref.
- **Cloned action SOs** inside `ActorRuntime` persist fine in RAM (no save/load reconstruct between scenes).
- **Direct arena testing** breaks until the debug-run guard lands (deferred doc).
