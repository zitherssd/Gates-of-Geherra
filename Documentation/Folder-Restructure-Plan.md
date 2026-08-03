# Folder Restructure Plan — Gates of Gehera

> Committed **before** any restructuring begins so we have a clear, versioned outline of objectives.
> Status: **IN PROGRESS** — updated as phases complete.

---

## 1. Overview

The project's folder layout has grown organically and needs reorganizing. This plan fixes, in seven independently-verifiable phases:

| # | Phase | Risk | Code changes |
|---|-------|------|--------------|
| 0 | Commit this plan to the repo | none | none |
| 1 | Asset cleanup (delete junk, add `Settings/` + `ThirdParty/`, split `Sounds/`/`Textures/`/`Animations`) | low | none |
| 2 | `Resources/` restructure + update `Resources.Load` callers | low | 6 files |
| 3 | `Scripts/` restructure + namespace updates | high | ~180 files |
| 4 | Add asmdefs (`GoG.Runtime` + `GoG.Editor`) | medium | 2 new files |
| 5 | Documentation sync | low | 3 docs |
| 6 | Verification (compile, scenes, playtest, builds) | — | none |

---

## 2. Current Problems

### Assets root (`Assets/`)
- 7 loose render/config assets (`URP.asset`, `New Lighting Settings.lighting`, `UniversalRenderPipelineGlobalSettings.asset`, renderer asset, etc.) — no `Settings/` folder.
- Poorly-named leftover `Untitled 3.fbx` (to be **deleted**).
- `Designdoc.txt` (docs mixed with assets).
- Third-party assets (`LeanTween`, `TextMesh Pro`, `Low Poly Stones`, `Samples`, `Adaptive Performance`) mixed at root with first-party folders.
- `Unity.VisualScripting.Generated/` — auto-regenerated, delete.

### `Assets/Sounds/`
Flat mess: music (mp3) + SFX (ogg/wav) + album-art jpgs + `Folder.jpg` + `Fri Sep 22 2023.mp3`. No `Music/` vs `SFX/` split.

### `Assets/Textures/`
60+ flat files mixing `.psd` sources, downloaded jpgs, materials, sprite sheets, `Untitled*.png`, `ChatGPT Image …`, `GoG Design Document-17.png` (doc screenshot).

### `Assets/Animations/`
Flat; `.anim` clips + `.controller`s mixed; `Fire (1).controller` (Unity duplicate-import artifact); unexplained `New/` folder.

### `Assets/Scripts/`
- `NewFolder1/` — **empty**.
- `Docs/` — markdown living inside `Assets` while real docs live at repo-root `Documentation/`.
- `Utility/` — 30+ unrelated scripts (cameras, popups, sound, input, slow-mo, HP bars) = dumping ground.
- Deep nesting: `Battle/Actions/Actions/Effects/`.
- Confusing names: `Battle/Idle/` actually holds Timelock/Cooldown code; `Pattern/` holds core state-machine patterns; `TitleScene/` is a scene-named folder.
- **Duplicate status systems**: `Battle/Status/` (namespace `Assets.Scripts.Battle.Status`) AND `Battle/Components/Status/` (namespace `Assets.Scripts.Battle.Components.Status`) — **both** define `StatusManager` + `PoisonStatus`.

### `Assets/Resources/`
Loose assets at root (`Action Database.asset`, `PoisonStatus.asset`, `DamageMultiplierStatus.asset`, `HpPopup.prefab`, `PosturePopup.prefab`) next to subfolders; `Actors/` and `Actions/` mix loose files + subfolders inconsistently.

### Misc
- `GameObject.prefab` (generic name).
- Stale gitignored `All.csproj`/`Battle.csproj`/`NewAssembly.csproj` at root.
- No asmdefs — everything compiles into `Assembly-CSharp`. Stale `GoG.*.csproj` at root are leftovers from a prior asmdef experiment (gitignored).

---

## 3. Constraints (discovered)

1. **Namespace == folder path** convention (`Assets.Scripts.*`). Restructuring scripts ⇒ namespace + `using` updates across ~180 files.
2. **Moving a file WITH its `.meta` preserves its GUID** ⇒ all scene/prefab/ScriptableObject references survive moves. **Deleting breaks references.**
3. `Resources.Load/LoadAll` paths live in only 6 files:
   - `ActionButtonHandler.cs` — `Sprites/ButtonBorder*`
   - `EffectsRepository.cs` — `HpPopup`, `PosturePopup`
   - `SkillGenerator.cs` — `Actions/Droptable`
   - `GameSession.cs` — `PlayerTemplatePath` (verify exact value)
   - `SandboxDebugMenuController.cs` — `Battles`, `Actions/Droptable`
   - `ActionDatabase.cs` — `Actions`
4. **Status system**: LIVE = `Assets.Scripts.Battle.Components.Status` (referenced by `Actor.cs`, `Block.cs`, `AddStatusEffect.cs`, `ActorUIController.cs`, and the `Resources/*.asset` status assets by GUID `f54c0181…` / `e0a38ea2…`). DEAD = `Assets.Scripts.Battle.Status` (unreferenced in code; its `StatusManager` is a MonoBehaviour, meta guid `45d981b8…` — verify no scene/prefab uses it before deleting).
5. `.meta` files are not grep-searchable — use direct reads.

---

## 4. Target Structure (Scripts)

```
Assets/Scripts/
├── GoG.Runtime.asmdef            # covers all below except Editor
├── Core/                         # Assets.Scripts.Core
│   ├── StateMachine.cs, IState.cs, IAttack.cs      (from Pattern/)
│   ├── StaticHelpers.cs, ActionSlot.cs, Intersections.cs  (from Utility/)
│   └── RhombusMeshCollider.cs   (from Physics/)
├── Game/                         # Assets.Scripts.Game (unchanged)
├── Crawler/                      # Assets.Scripts.Crawler (unchanged)
├── Battle/
│   ├── Actor/ (+ States/, AI/, Systems/)   (unchanged — well organized)
│   ├── Actions/
│   │   ├── BaseAction, BaseSkill, DamageInstance, MoveData, ActionEndReason,
│   │   │   WorldActionTrigger, RewardPool, HitWindows/
│   │   ├── Skills/               # from Actions/Actions/ (concrete actions)
│   │   └── Effects/              # from Actions/Actions/Effects/ (IEffect + impls)
│   ├── Manager/ (+ States/)      (unchanged)
│   ├── Items/ (+ UI/)            (unchanged)
│   ├── Status/                   # consolidated from Components/Status/ (live)
│   ├── Components/               # Audio/, Effects/
│   └── Timelock/                 # from Idle/ (TimelockManager, Timelock, CooldownDisplay)
├── Save/                         # Assets.Scripts.Save (unchanged)
├── UI/
│   ├── UIManager, StatPanelUI, JoystickManager, TargetingManager,
│   │   ActionCardHandler, ActionInventoryHandler, ActionUIManager, DropSlot,
│   │   ActionButtonBattle, ActionButtonInventory, ActionButtonHandler (from Battle/Actions/)
│   └── Title/                    # from TitleScene/ (MainMenuController, MainMenuSlot)
├── Utility/                      # single namespace Assets.Scripts.Utility (documented exception)
│   ├── Camera/   Feedback/  Input/  Audio/  Misc/
└── Editor/                       # GoG.Editor.asmdef (Editor-only)
    └── ActorDefinitionEditor, BaseActionEditor, SlowdownVisualEffectEditor, StateMachineDrawer
```

---

## 5. Phases

### Phase 0 — Commit plan .md to repo
1. Create `Documentation/Folder-Restructure-Plan.md` (this file).
2. `git add` + commit the plan doc alone (`docs: add folder restructure plan`).
3. Revisions during execution update this file + re-commit.

### Phase 1 — Asset cleanup (no code changes)
1. Delete junk: `Scripts/NewFolder1/` (empty); album-art jpgs + `Folder.jpg`; `Fri Sep 22 2023.mp3`; `Textures/ChatGPT Image …`; `Textures/Untitled*.png`; `Textures/GoG Design Document-17.png`; `Animations/Fire (1).controller`; stale root `All.csproj`/`Battle.csproj`/`NewAssembly.csproj`.
2. Loose root render config → `Assets/Settings/`: `URP.asset`, `New Universal Render Pipeline Asset_Renderer.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `New Lighting Settings.lighting`, `New 2D Renderer Data.asset`.
3. Third-party → `Assets/ThirdParty/` (move WITH metas): `LeanTween`, `Low Poly Stones`, `TextMesh Pro`, `Samples`, `Adaptive Performance`; delete `Unity.VisualScripting.Generated/`.
4. DELETE `Untitled 3.fbx`; `Designdoc.txt` → `Documentation/`.
5. `Sounds/` → `Music/` + `SFX/` (mp3→Music, ogg/wav→SFX; drop jpgs).
6. `Textures/` → `UI/`, `Environment/`, `Effects/`, `Character/`, `Source/` (.psd). Move loose `.mat`s (`Sandbx.mat`, `Skybox.mat`) to Materials.
7. `Animations/` → `Clips/` + `Controllers/`.
8. `Prefabs/` → subfolders (`UI/`, `Actor/`, `Props/`, `Effects/`); rename generic `GameObject.prefab`.

### Phase 2 — Resources restructure (small code changes)
1. `Resources/` root loose assets → `Resources/Statuses/` (`PoisonStatus`, `DamageMultiplierStatus`), `Resources/Prefabs/` (`HpPopup`, `PosturePopup`). UPDATE `EffectsRepository.cs` paths → `"Prefabs/HpPopup"`, `"Prefabs/PosturePopup"`.
2. `Resources/Actors/`: keep player (`MC.asset`) at root; move enemies into `Actors/Enemies/`. Verify `GameSession.PlayerTemplatePath` still resolves.
3. `Resources/Actions/`: move loose `AI Move.asset`/`Player Move.asset`/`Jump.asset` → `Movement/`; keep `Attacks/`, `Defensive/`, `Droptable/`. Verify `ActionDatabase.cs`, `SkillGenerator.cs`, `SandboxDebugMenuController.cs` paths.
4. Tidy `Resources/Items/`, `Resources/Battles/`, `Resources/Crawler/Droptables/` only if needed.
5. Run error check + re-grep all `Resources.Load*` after moves.

### Phase 3 — Scripts restructure + namespaces
1. Delete legacy `Assets/Scripts/Battle/Status/` AFTER grep-checking scenes/prefabs for guid `45d981b8f256e894bbe666164e1b44b0`. Move live `Components/Status/` → `Battle/Status/`, namespace `Assets.Scripts.Battle.Status`; update usings in `Actor.cs`, `Block.cs`, `AddStatusEffect.cs`, `ActorUIController.cs` (+ any others via find-replace).
2. Collapse nesting: `Battle/Actions/Actions/` → `Battle/Actions/Skills/`; `Battle/Actions/Actions/Effects/` → `Battle/Actions/Effects/`. Namespaces `Assets.Scripts.Battle.Actions.Actions[.Effects]` → `Assets.Scripts.Battle.Actions.Skills[.Effects]`; find-replace all usings.
3. `Battle/Idle/` → `Battle/Timelock/`; namespace `Assets.Scripts.Battle.Idle` → `Assets.Scripts.Battle.Timelock`.
4. `Pattern/` → `Core/`; namespace `Assets.Scripts.Pattern` → `Assets.Scripts.Core`. Pull `StaticHelpers`, `ActionSlot`, `Intersections` from Utility (→ `Assets.Scripts.Core`); `Physics/RhombusMeshCollider.cs` → Core.
5. `TitleScene/` → `UI/Title/`; namespace → `Assets.Scripts.UI.Title`.
6. `Scripts/Docs/` → move .md files to `Documentation/`.
7. `Utility/`: split into `Utility/Camera/`, `Utility/Feedback/`, `Utility/Input/`, `Utility/Audio/`, `Utility/Misc/`. **Keep single namespace `Assets.Scripts.Utility`** (documented exception) to avoid massive using-churn.
8. Move `ActionButtonBattle.cs`, `ActionButtonInventory.cs`, `ActionButtonHandler.cs` from `Battle/Actions/` → `Scripts/UI/`.
9. After each sub-step: error check + grep for stale namespace references.

### Phase 4 — asmdefs
1. `Assets/Scripts/GoG.Runtime.asmdef` — all runtime scripts. `autoReferenced: true`.
2. `Assets/Scripts/Editor/GoG.Editor.asmdef` — `includePlatforms: ["Editor"]`, references GoG.Runtime. Editor folder auto-excluded from parent asmdef.
3. Do NOT split further (Game↔Battle↔UI coupling heavy; circular-ref risk).
4. Verify Unity regenerates `GoG.Runtime.csproj`/`GoG.Editor.csproj`; no package/plugin hard-references Assembly-CSharp (LeanTween, MackySoft have own asmdefs).

### Phase 5 — Documentation sync
1. `Documentation/Registry.md` — update namespace map + folder paths.
2. `Documentation/AI_CONTEXT.md` — update folder-path references.
3. `.github/copilot-instructions.md` — update folder references if any.
4. Update repo memory notes if paths changed materially.

### Phase 6 — Verification
1. Compile clean in Unity (no errors).
2. Open TitleScene + CaveScene + an ArenaScene (Crossing): no missing scripts, no missing SO references, statuses attach, HP popups spawn.
3. Playtest a battle via `ArenaBootstrapper.debugBattle`; verify enemy AI, actions, statuses, item drops.
4. Verify Build Settings valid; player builds (WebGL + mobile).
5. Spot-check save/load round-trip.

---

## 6. Decisions (locked)

- Scope = full restructure: asset cleanup + Resources + Scripts + asmdefs.
- Junk deletion approved — always move with `.meta`; delete only confirmed-dead.
- Namespaces: **Option A** — keep `Assets.Scripts.*` prefix; rename only moved path segments via deterministic find-replace. (Rejected `GoG.*` as excessive churn.)
- Utility subfolders keep a single `Assets.Scripts.Utility` namespace (documented exception).
- asmdefs: **Option A** — minimal 2 (`GoG.Runtime` + `GoG.Editor`) to avoid circular refs.
- Legacy `Battle/Status/` deleted only after scene/prefab GUID check.
- `Untitled 3.fbx`: DELETE.

---

## 7. Verification / Rollback

- Each phase ends with a verification gate before moving on.
- Rollback: `git` revert of the affected phase. Asset moves are safe (GUIDs preserved); deleted junk was confirmed-dead; if a deletion turns out to be needed, restore from git history.
