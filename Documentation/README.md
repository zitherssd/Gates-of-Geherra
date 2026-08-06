# Documentation Index

> Index of `Documentation/`. Each active doc carries a **Last verified** stamp — when you
> change code or scenes, update the affected doc's stamp to today's date and fix any drift.
>
> Last index refresh: **2026-08-03** (consolidation — removed stale `Plans/`,
> `SystemDependencyGraph.json`, and `Folder-Restructure-Plan.md`).

## Active docs

| File | Purpose | Last verified |
|------|---------|---------------|
| `AI_CONTEXT.md` | **Start here.** Primary orientation: architecture, dev rules, dangerous areas, workflows | 2026-08-03 |
| `Architecture.md` | Startup sequences, init order, dependency tree, singletons, events, scene data flow | 2026-08-03 |
| `DataArchitecture.md` | ScriptableObject types + fields (ActorDefinition, BaseAction, BattleDefinition, items, statuses) | 2026-08-03 |
| `Registry.md` | Catalog of scripts (by namespace), scenes (build index + contents), prefabs | 2026-08-03 |
| `improvements.md` | Living bug/quality report; items marked ✅ Fixed were verified 2026-08-03 | 2026-08-03 |
| `Designdoc.txt` | Game lore / design doc (not code; not code-verified) | — |
| `Systems/AI.md` | Enemy AI / behavior trees | 2026-08-03 |
| `Systems/Audio.md` | Music + SFX playback | 2026-08-03 |
| `Systems/Camera.md` | Dynamic battle camera, shake, occlusion | 2026-08-03 |
| `Systems/Combat.md` | Core combat: actions, damage, hitboxes, states | 2026-08-03 |
| `Systems/Input.md` | Touch / on-screen input routing | 2026-08-03 |
| `Systems/Items.md` | Items, inventory, ItemEventBus, crafting | 2026-08-03 |
| `Systems/Progression.md` | Floors, skills, training, run persistence | 2026-08-03 |
| `Systems/SaveSystem.md` | JSON save/load, slots, action loadout | 2026-08-03 |
| `Systems/StatusEffects.md` | Status effects (poison, block, damage multipliers) | 2026-08-03 |
| `Systems/TimeControl.md` | Slowdown / time-scale modes | 2026-08-03 |
| `Systems/UI.md` | HUD, action buttons, menus, fades | 2026-08-03 |

## Removed (2026-08-03 consolidation)

| Removed | Why | Actionable content moved to |
|---------|-----|------------------------------|
| `Plans/KnowledgeCoverage.md` | Stale knowledge snapshot (2026-06-18) | — |
| `Plans/Scene-Persistence-Implementation.md` | Marked "planned" but already shipped | `Architecture.md`, `AI_CONTEXT.md` |
| `Plans/Manual-Editor-Steps.md` | Editor wiring checklist, superseded | `improvements.md` §21.1 |
| `Plans/BattleUI-Persistent-Canvas.md` | Deferred proposal | `improvements.md` §21.2 |
| `Plans/PlayerActors-Single-Player-Refactor.md` | Partially shipped | `improvements.md` §21.3 |
| `Plans/ArenaScene-Direct-Testing.md` | Deferred proposal | `improvements.md` §21.4 |
| `SystemDependencyGraph.json` | Redundant with `Architecture.md`; nothing consumed it | `Architecture.md` |
| `Folder-Restructure-Plan.md` | Completed historical record | `AI_CONTEXT.md` (Dangerous Area 8 — `[SerializeReference]` gotcha) |
