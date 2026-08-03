# Plan: Collapse PlayerActors list → single Player (do later)

> Deferred. Captured during the scene-persistence design session. Not yet implemented.
> Game is **strictly single-player**; the list is legacy.

## Goal
`BattleManager.PlayerActors` is a `List<Actor>` for legacy reasons. Since the game is
single-player, collapse it to a single `Player` reference to remove `[0]` indexing and
`Concat` gymnastics. `EnemyActors` stays a list.

## Decision
Defer. Keep the list shape for now to avoid churn. `GameSession` holds exactly **one**
player `ActorRuntime`; the list is only a battle-time convenience and should not be treated
as the source of truth.

## Checklist (when we do it)
- [ ] Add `public Actor Player { get; }` to `BattleManager` (single).
- [ ] Replace reads of `PlayerActors[0]`:
  - Utility/CameraManager.cs (~L51-52): `PlayerActors[0].transform`, `PlayerActors[0]`.
  - Utility/CameraManager.cs (~L133): `PlayerActors[0].target.target`.
  - Utility/CameraOcclusionManager.cs (~L102): `PlayerActors.Concat(EnemyActors)` →
    iterate `EnemyActors` + the single `Player`.
- [ ] Update `BattleManager.Enter` / `SpawnEnemies` / refresh logic to set `Player` instead
      of (or alongside) the list.
- [ ] Once all readers use `Player`, remove `PlayerActors` (or keep a single-element
      compatibility shim if something external still needs a list).

## Notes
- Do this **after** GameSession + per-scene spawning lands, since spawning is where `Player`
  gets assigned (via `OnPlayerSpawned`).
- Co-op is explicitly out of scope; do not design the list to "keep the door open."

## Touch points (known today)
- Battle/Manager/BattleManager.cs — field + spawn/refresh
- Utility/CameraManager.cs — multiple `PlayerActors[0]`
- Utility/CameraOcclusionManager.cs — `PlayerActors.Concat(EnemyActors)`
