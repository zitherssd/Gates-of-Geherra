# Plan: Direct ArenaScene testing (do later)

> Deferred. Captured during the scene-persistence design session. Not yet implemented.

## Goal
Be able to hit **Play directly on an ArenaScene** in the editor without first going through
TitleScene, so we can iterate on a single arena fast.

## The problem
With the new design, the persistent objects (`GameSession` + the global services) are
created **once in TitleScene**. If you enter an arena scene directly, none of them exist,
so spawning/binding the player and starting a battle has nothing to read from.

## Decision
Add a **bootstrap guard**: if a scene boots and `GameSession` doesn't exist yet, create a
**default debug run** so the scene is fully playable on its own. Must be obviously
debug-only and never run in the real flow.

## Checklist (when we do it)
- [ ] In the scene bootstrapper, first line: `if (GameSession.Instance == null) StartDebugRun();`
- [ ] `StartDebugRun()`:
  - [ ] Create `GameSession` and call `StartNewRun()` (default MC `ActorDefinition` from
        Resources, fixed or rolled stats).
  - [ ] Lazy-init any missing services (UIManager/Sound/Input/Slowdown) — same "create if
        null" guard they use in TitleScene.
  - [ ] For arenas: assign a default `BattleDefinition` (debug enemy pool + a spawn-group id)
        as the pending battle.
- [ ] Log a clear warning: `"[DEBUG] No GameSession found — starting default debug run."`
- [ ] Consider a `DebugRunConfig` ScriptableObject (which MC, which stats, which battle) so
      each arena can point at its own test setup.

## Gotchas
- Guard must be a no-op in the real flow (GameSession already exists → skip entirely).
- Keep debug enemy pools / definitions out of shipping content folders, or clearly tagged.
- If services are also created lazily here, make sure the TitleScene path still wins
  (don't double-create when launched normally).

## Related
- BattleUI-Persistent-Canvas.md (services lazy-init)
- GameSession + bootstrapper design (main session)
