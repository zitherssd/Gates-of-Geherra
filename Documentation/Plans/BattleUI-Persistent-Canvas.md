# Plan: Persistent BattleUI + Audio (do later)

> Deferred. Captured during the scene-persistence design session. Not yet implemented.

## Goal
BattleUI and music should **persist across scenes** (Title → Rest → Arenas) instead of
being copied into every ArenaScene.

## Why not "copy BattleUI into each ArenaScene"
That's the N-copies trap: every UI tweak means editing every arena scene, and they drift
out of sync. One source of truth is cheaper and safer.

## Decision
Make the global presentation/services **persistent singletons created once in TitleScene**
(`DontDestroyOnLoad`):
- `UIManager` + its BattleUI canvas
- `SoundManager` (music continuity across rest ↔ arena)
- `InputHandler`
- `SlowdownManager`

Each scene re-binds these to the current player via the `OnPlayerSpawned` event (see
GameSession / bootstrapper plan), rather than the UI/camera finding the player itself.

## Checklist (when we do it)
- [ ] Create the services once in TitleScene; guard against duplicates on re-entry
      (`if (instance != null) { Destroy(gameObject); return; }`).
- [ ] `DontDestroyOnLoad` the UI canvas root + services.
- [ ] Subscribe `UIManager` to `OnPlayerSpawned` to rebind health bars / action buttons
      to the freshly spawned player body each scene.
- [ ] Audit `UIManager` for **hard scene references cached once** and move them to a
      `sceneLoaded` re-resolve:
  - `OriginPoint = GameObject.FindGameObjectWithTag("OriginPoint")` — UI/UIManager.cs (~L70).
  - Any `GameObject.Find("LeftContainer"/"RightContainer"/"PlacementGuide")` in
    Battle/Actions/ActionButtonBattle.cs.
  - Joystick refs in UI/JoystickManager.cs (`GameObject.Find("VirtualJoystickBase"/"VirtualKnob")`).
- [ ] If the BattleUI canvas is `Screen Space - Camera`, re-assign its render camera on
      each scene load (the camera respawns per scene).
- [ ] Decide what hides in non-battle scenes (Title/Rest): keep the canvas alive but toggle
      panels, driven by game state.

## Gotchas
- Persistent canvas + per-scene camera = stale `worldCamera` ref → UI stops rendering /
  raycasts break. Re-resolve on `sceneLoaded`.
- Event subscriptions to the **old** player body must be cleared when the new one spawns
  (old subscribers die with the old scene). Rebind via `OnPlayerSpawned`.
- Music should NOT restart on scene load if the same track should continue — only change
  track on explicit mode change (rest theme vs battle theme).

## Related
- ArenaScene-Direct-Testing.md (services must lazy-init when an arena is entered directly)
- PlayerActors-Single-Player-Refactor.md
