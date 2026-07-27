# Gates of Gehera — Project Guidelines

## Documentation

Before making code changes, consult the relevant documentation in `Documentation/` for context:

- **`AI_CONTEXT.md`** — Start here. Primary orientation guide: architecture, development rules, dangerous areas, common workflows.
- **`Architecture.md`** — Startup sequences, initialization order, system dependency tree, singleton relationships, scene-to-scene data flow.
- **`DataArchitecture.md`** — All ScriptableObject types with field details (ActorDefinition, BaseAction, BattleDefinition, items, statuses).
- **`Registry.md`** — Catalog of scripts (by namespace), scenes (build index + contents), and prefabs.
- **`improvements.md`** — Known bugs, code quality issues, and improvement suggestions organized by priority.
- **`SystemDependencyGraph.json`** — Machine-readable dependency data.
- **`Documentation/Systems/*.md`** — Deep dives into individual systems: Combat, AI, Audio, Camera, Input, Items, Progression, SaveSystem, StatusEffects, TimeControl, UI.
