# Knowledge Coverage Report

**Generated**: 2026-06-18  
**Method**: Static code analysis + Unity MCP scene inspection (CaveScene hierarchy) + Repository memory

---

## Systems Discovered

| System | Confidence | Status |
|--------|-----------|--------|
| Combat (actions, hitbox, damage) | 95% | Fully documented — extensive code review |
| AI Behavior Tree | 90% | All BT rulesets and behavior/condition scripts reviewed |
| Actor Runtime / Definition / State Machine | 95% | Core classes fully read |
| Battle Lifecycle (BattleStateMachine + States) | 95% | All 3 states fully read |
| Game Session (persistence, scene hand-off) | 95% | Fully reviewed |
| Save System | 90% | All scripts reviewed; field-name bug documented |
| Progression (floors, skills, training) | 85% | FloorManager and SkillGenerator fully read; TrainingManager.AwardRandomStat() body not confirmed |
| UI (action buttons, drag-drop, menus) | 80% | UIManager structure clear; some secondary UI scripts not reviewed |
| Item System (consumables, trinkets, crafting) | 85% | Core classes reviewed; item assets not individually inspected |
| Status Effect System | 55% | Class structure known; tick/duration internals not reviewed |
| Camera System | 85% | Full algorithm reviewed; SlowTrack behavior partially inferred |
| Time Control (Slowdown) | 90% | Full SlowdownManager reviewed |
| Audio System | 40% | Only SoundManager public API known; clip library unknown |
| Input System | 75% | InputHandler and ActionButtonBattle understood; gamepad support unknown |
| Scene Architecture | 90% | All 5 scenes identified; CaveScene fully inspected via MCP; TitleScene/Sandbox inferred |

---

## Systems Partially Understood

### Status Effect System
- **Known**: Class hierarchy (BaseStatus, PoisonStatus, BlockStatus, DamageMultiplierStatus, DoubleDamageStatus, Stagger), `StatusManager.Tick()` called per frame, `AddStatusEffect` applies statuses from actions.
- **Unknown**: Duration tracking mechanism, stacking behavior, how status interacts with `DamageInstance.Calculate()`, whether `BaseStatus` is an SO or plain class.
- **Confidence**: 55%

### Training Manager
- **Known**: `TriggerTraining()` starts a timer, increment `trainingsDoneThisFloor`, timer scales 10–15s.
- **Unknown**: `AwardRandomStat()` implementation — which stat is awarded, how (random pick? weighted?), UI feedback.
- **Confidence**: 65%

### UI — Secondary Scripts
- **Known**: `UIManager` central controller, action button architecture, drag-drop containers, skill selection cards.
- **Unknown**: `ActionUIManager.cs`, `TargetingManager.cs`, `ActionInventoryHandler.cs` — their exact roles not confirmed.
- **Confidence**: 70%

### CrawlerManager
- **Known**: `CrawlerManager` is a MonoBehaviour on the `CrawlerManager` child of `Managers`. The class body is empty (stub).
- **Unknown**: Intended purpose — possibly a future dungeon-crawler map system.
- **Confidence**: 20%

### Audio
- **Known**: `SoundManager.PlayMusic()`, `PlayMusicRest()`, `PlaySE(name)`, `FadeOutMusic()` exist.
- **Unknown**: Audio clip library, AudioMixer setup, per-actor sounds, how music tracks are assigned.
- **Confidence**: 40%

---

## Unknown Areas

| Area | Notes |
|------|-------|
| **TitleScene hierarchy** | Not inspected via MCP (scene load failed via additive path). Contents inferred from `MainMenuController`. |
| **SandboxScene hierarchy** | Not inspected. Used for free testing; contents unknown. |
| **DebugScene** | Disabled in build. Not inspected. |
| **ArenaScenes/Crossing hierarchy** | Not inspected via MCP. Structure inferred from `ArenaBootstrapper` and `SpawnGroup`. |
| **Actor Components (Audio sub-folder)** | `Scripts/Battle/Components/Audio/` contents not listed. |
| **`AiRuleset.Ninja`** | Assigned in enum but no behavior list in AIBT — completely inert AI. |
| **`ActorBehaviorTree.cs`** | Script exists at `Scripts/Battle/Actor/ActorBehaviorTree.cs` but was not read. May be an alternate/legacy BT. |
| **`ActorUIController.cs`** | Script exists but not read — may handle HP bar and UI updates for individual actors. |
| **`HitWindowManager.cs` / `HitWindow.cs`** | In `Scripts/Battle/Actions/HitWindows/`. Not reviewed — may be a more advanced hit timing system. |
| **`EffectsRepository.cs`** | In `Scripts/Battle/Components/Effects/`. Not reviewed. |
| **`Items/UI/` sub-folder** | `Scripts/Battle/Items/UI/` — likely tooltip and inventory UI scripts; not reviewed. |
| **Arena enum values** | `Arena.None` and `Arena.Crossing` confirmed. Other values unknown. |
| **`SpawnGroupId` enum values** | `SpawnGroupId.Default` confirmed. Others unknown. |
| **`Rarity` enum values** | `BaseAction.Rarity` field exists but enum definition not reviewed. |
| **`TAG` enum values** | Some tags documented in repo memory (PROJECTILE, KNOCKBACK_*, etc.); full list not confirmed. |
| **`NewFolder1/` scripts** | `Scripts/NewFolder1/` — directory listed but contents not explored. |
| **Prefabs/Effects/ directory** | Likely particle prefabs for `PlayParticleEffect`; not listed. |
| **`DragMove.cs` implementation** | Referenced but body not read. |
| **`TooltipUI`** | Component seen in ROOT UI hierarchy; script not read. |

---

## Missing References

| Missing | Impact |
|---------|--------|
| `TimeLockManager` script location | Unclear if it's in Utility/ or Game/; reference works but file not confirmed |
| `BindPlayerBody()` method body | Called in `GameFlowManager.Start()` but not in the reviewed portion — complete binding logic unclear |
| `ActorSaveData.LoadInto()` full body | Second half of the method (past the HP bars block) not reviewed |
| `GameFlowManager.EnterBattle()` | Referenced but the method body was not in the reviewed file portion |
| `SaveManager.LoadGame()` and `SlotExists()` | Called but not reviewed; WebGL/PC branching assumed |
| `ActionDatabase.Initialize()` | Called in SaveManager.Start() but implementation not read |

---

## Recommended Future Investigation

### High Priority (Before Making Changes)

1. **Read `HitWindowManager.cs` and `HitWindow.cs`** — may represent an advanced timing system not yet integrated with the action system. Could affect multi-hit and frame-data plans.

2. **Read `ActorUIController.cs`** — likely manages the per-actor HP bar UI; important for any UI refactor.

3. **Read `StatusManager.cs` tick implementation** — needed before adding new status effects or modifying the damage pipeline.

4. **Read `GameFlowManager.EnterBattle()`** — the scene-transition split (in-scene vs arena) is central to all battle entry.

5. **Inspect TitleScene via MCP** — confirm SaveManager placement and whether a second UIManager exists there.

### Medium Priority

6. **Read `ActorBehaviorTree.cs`** — confirm if this is a live system or a legacy file that can be deleted.

7. **Read `EffectsRepository.cs`** — may contain a registry of all available effects used by tooling or runtime.

8. **Inspect `Assets/Scripts/NewFolder1/`** — unidentified script folder.

9. **Inspect ArenaScenes/Crossing via MCP** — confirm SpawnGroup setup, BattleManager placement, ArenaBootstrapper configuration.

10. **Read `Items/UI/` scripts** — confirm item UI flow for equipping and using items.

### Low Priority

11. **Verify `CrawlerManager`** intent — stub class or future dungeon map system.
12. **Confirm `Ninja` AiRuleset** is intentionally empty or fill it in.
13. **Check `RARITY` enum** values for action rarity display.
14. **Confirm `Spirit` stat effect** — no code path using Spirit was found in reviewed scripts.

---

## Confidence Summary

| Area | Confidence |
|------|-----------|
| Core combat architecture | 95% |
| Scene structure and flow | 88% |
| Data architecture (SOs) | 90% |
| Persistence and save system | 88% |
| AI system | 90% |
| Progression system | 82% |
| Item system | 80% |
| UI system | 75% |
| Status effects | 55% |
| Audio system | 40% |
| Unknown scripts / new folders | 10% |
| **Overall project knowledge** | **79%** |
