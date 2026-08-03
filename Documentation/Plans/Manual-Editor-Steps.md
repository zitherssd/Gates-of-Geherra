# Scene Persistence — Manual Editor Steps (do these in Unity)

All code for the run-persistence + multi-scene arena flow is in place and compiling. The
remaining work is editor wiring that can't be done from code. Everything is **additive**: the
existing in-scene CaveScene flow keeps working untouched until you opt a battle into an arena.

---

## 0. Two behavioural changes to verify first

1. **Rolled stats + entered name now actually apply.** Previously `playerActor.Spawn()` ran
   `Runtime.ResetToDefinition()` *after* rolling stats and setting the entered name, silently
   discarding both. The new `GameSession.StartNewRun()` applies rolled stats and the name last
   and never re-resets, so a new run now uses rolled stats + the entered name. If you actually
   wanted fixed Definition stats, tell me and I'll revert that part.

2. **Bound player inventory shares the run's item list.** When the player body is bound
   (`Actor.Bind`), its `inventory.Items` now *is* `ActorRuntime.items` (shared, not copied), so
   items gained/used in an arena persist when you return to rest. No editor action needed — just
   be aware that the bound player's body inventory and the persistent model are the same list.

---

## 1. Player prefab (once)

- Make sure you have a **Player body prefab** (the Actor with `NavMeshAgent`, model, etc.).
- It needs an `ActorDefinition` assigned (used as a fallback; `Bind` overrides it with the run's
  runtime Definition).
- You'll assign this same prefab in two places below.

## 2. Rest scene (CaveScene today, or a dedicated RestScene)

On the `GameFlowManager` object:
- **playerPrefab** -> assign the Player prefab.
- **playerSpawnPoint** -> assign a Transform marking where the player appears (optional; if the
  scene still has a pre-placed `playerActor`, that's used and no spawning happens).
- Leaving **playerActor** empty makes it spawn from the prefab at `playerSpawnPoint`.

> CaveScene with its existing pre-placed player keeps working with no changes.

## 3. Arena scenes (one per arena — the new flow)

For each arena scene:

1. **Create the scene** and add it to **Build Settings**.
2. **Register it in `ArenaCatalog`** (code): add an `Arena` enum value in
   [Battle/Manager/Arena.cs](Battle/Manager/Arena.cs) and map it to the scene name in
   `ArenaCatalog.SceneName`. Current mapping:
   - `Arena.Cave -> "CaveScene"`, `Arena.Debug -> "DebugScene"`, `Arena.Sandbox -> "SandboxScene"`.
   Tell me the new arena names and I'll add the enum + mapping for you.
3. **Add the battle infrastructure** the battle code expects (these are per-scene singletons, not
   DontDestroyOnLoad): `BattleManager`, `UIManager`, `CameraManager`, `SoundManager`, plus
   `SlowdownManager` / `TooltipUI` / `SkillGenerator` if your battles use them.
   - On `BattleManager`: assign **enemyPrefab**; leave **PlayerActors** EMPTY (filled at runtime).
4. **Add an `ArenaBootstrapper`** component (`Game/ArenaBootstrapper.cs`):
   - **playerPrefab** -> the Player prefab.
   - **debugBattle** -> optional, only for entering this scene directly (see section 6).
5. **Add one or more `SpawnGroup`s** (`Battle/Manager/SpawnGroup.cs`) — empty GameObjects with the
   component:
   - **id** -> a `SpawnGroupId` (matches the BattleDefinition's `spawnGroup`).
   - **playerSpawn** -> Transform where the player starts.
   - **enemySpawns** -> one Transform per enemy slot (reused cyclically if fewer than enemies).
   - **cameraAnchor** -> optional camera framing point (falls back to legacy if unset).

## 4. BattleDefinition assets

The two removed fields (`enemyStartPosition`, `playerStartPosition`) will simply drop off the
assets on reserialize — no action needed beyond a re-save. For each Battle asset set:
- **arena** -> `None` for in-scene battles (CaveScene); the target `Arena` for arena battles.
- **spawnGroup** -> which `SpawnGroupId` to use in that arena.
- **Level** -> keep for legacy in-scene battles; ignored once a `SpawnGroup` is found.
- **enemyActors** / reward pools -> unchanged.

## 5. Triggering an arena battle (no code change)

`FloorManager.ProgressToNextFloor()` / `QuickFight()` already call
`GameFlowManager.EnterBattle(battle, ...)`. With the new routing:
- If `battle.arena` maps to a **different** scene than the active one -> it sets
  `GameSession.PendingBattle` + `ReturnScene` and loads the arena scene; the arena's
  `ArenaBootstrapper` spawns/binds the player and starts the fight.
- Otherwise -> the legacy in-scene battle runs exactly as before.
- On **victory** -> returns to `ReturnScene` (where you came from). On **death** -> `TitleScene`
  (unchanged). Note: `onBattleEnd` callbacks are **not** carried across a scene load (a warning
  is logged); encode post-arena logic on `GameSession` instead if you need it.

## 6. Entering play / testing

- **Normal:** start from `TitleScene` so a run (and `GameSession.PlayerRuntime`) exists, then go
  rest -> arena -> rest.
- **Direct arena testing:** entering an arena scene directly works for combat, but
  `GameSession.PlayerRuntime` will be null (no run started), so the player body falls back to its
  prefab's Definition stats rather than run stats. To test with real run stats, start a run first.
  (A proper "debug run" guard is captured in `ArenaScene-Direct-Testing.md`.)

---

## What I changed in code (for reference)

- **New:** `Game/GameSession.cs` (persistent run data), `Game/ArenaBootstrapper.cs`,
  `Battle/Manager/Arena.cs` (`Arena` + `ArenaCatalog`), `Battle/Manager/SpawnGroup.cs`
  (`SpawnGroupId` + `SpawnGroup`).
- **Changed:** `Actor.Bind` + shared-inventory `ActorInventory.RebindTo`; `GameFlowManager`
  (spawn/bind, run-data delegation, `EnterBattle` arena routing, collapsed `SetMode`);
  `BattleManager` (`Player` resolver, `SpawnGroup`-based spawning with legacy fallback, dead-code
  removed); `BattleDefinition` (arena/spawnGroup added, start-position fields removed);
  `BattleEndState` (player via `BattleManager.Player`, arena-aware return); `FloorManager`
  (`currentFloor` on `GameSession`); `CameraManager` (player via `OnPlayerSpawned`, null-guarded).
