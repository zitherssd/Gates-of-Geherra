# Gates of Gehera — Project Guidelines

## Documentation

Before making code changes, consult the relevant documentation in `Documentation/` for context.
**`Documentation/README.md`** is the index — it lists every doc with its "last verified" date
and what was removed during the 2026-08-03 consolidation:

- **`AI_CONTEXT.md`** — Start here. Primary orientation guide: architecture, development rules, dangerous areas, common workflows.
- **`Architecture.md`** — Startup sequences, initialization order, system dependency tree, singleton relationships, scene-to-scene data flow.
- **`DataArchitecture.md`** — All ScriptableObject types with field details (ActorDefinition, BaseAction, BattleDefinition, items, statuses).
- **`Registry.md`** — Catalog of scripts (by namespace), scenes (build index + contents), and prefabs.
- **`improvements.md`** — Known bugs, code quality issues, and improvement suggestions organized by priority.
- **`Documentation/Systems/*.md`** — Deep dives into individual systems: Combat, AI, Audio, Camera, Input, Items, Progression, SaveSystem, StatusEffects, TimeControl, UI.

## Skills

The agent has access to gamedev skill files under `.agents/skills/`. **Always consult `find-skills` first before making code changes — it will route to the right domain skill.**
