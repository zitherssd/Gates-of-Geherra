# Camera System

## Purpose

Provides a dynamic 3D camera that tracks the player and enemies, adapts framing to combat spread, and handles visual effects (shake, slow-track, occlusion).

---

## Responsibilities

- Dynamically position the camera between the player and the weighted enemy centroid.
- Scale distance (`UpDistance`, `BackDistance`) based on enemy spread.
- Support camera shake on hit.
- Enable SlowTrack mode (camera locks on death hit).
- Make occluding objects transparent.
- Subscribe to `GameSession.OnPlayerSpawned` to track the player across scene transitions.

---

## Main Scripts

| Script | Role |
|--------|------|
| `CameraManager.cs` | Main camera controller; singleton |
| `CameraOcclusionManager.cs` | Makes objects between camera and player transparent |
| `DirectionalLightController.cs` | Rotates directional light dynamically |

---

## Camera Position Algorithm

```
Each frame:
1. weightedEnemyPosition = TargetingSystem.GetWeightedAverageEnemyPosition()
2. midpoint = (weightedEnemyPosition + playerPosition) / 2
3. directionVector = (weightedEnemyPosition - playerPosition) / 2  [projected to XZ plane]
4. if !Override:
     spread = LargestDirectionFromEnemies().magnitude (clamped 1–30)
     UpDistance = LinearMap(spread, 1, 30, 1.7, 5.0)
     BackDistance = LinearMap(spread, 1, 30, 3.3, 11.0)
5. Flip directionVector to align with camera.right
6. rightPerp = Cross(directionVector, Vector3.up)
7. targetPos = midpoint + (0, UpDistance, 0) + (-rightPerp * BackDistance)
8. camera.position = Lerp(current, target, ...) [smooth tracking]
```

---

## Camera Modes

| Mode | Trigger | Behavior |
|------|---------|----------|
| Normal | Default | Weighted midpoint tracking |
| SlowTrack | `SlowTrack = true` (on death hit) | [UNVERIFIED exact behavior — tracking locked/slowed] |
| Override | `Override = true` | Manual `UpDistance`/`BackDistance` values used |
| Shake | `CameraShake()` / similar | Random offset added, decays over time |

---

## Data Sources

- `Actor.target.GetWeightedAverageEnemyPosition()` — enemy centroid
- `Actor.target.LargestDirectionFromEnemies()` — spread magnitude
- `GameSession.OnPlayerSpawned` — player body reference after arena spawn

---

## Events

None emitted. Reads from GameSession event.

---

## External Dependencies

- `GameSession.OnPlayerSpawned` — subscribes/unsubscribes in OnEnable/OnDisable
- `BattleManager.PlayerActors[0]` — fallback player reference on Start
- `TargetingSystem` (via player Actor) — enemy position queries

---

## Extension Points

- Adjust zoom range: modify `LinearMap` range constants in `CameraManager.Update()`.
- Add new camera mode: add condition in `Update()`, set flag like `SlowTrack` or `Override`.

---

## Risks

- **`Override = true` uses Inspector values only** — `UpDistance` and `BackDistance` become static; suitable for debugging but not production.
- **SlowTrack is never reset to false automatically** — after a death hit, `SlowTrack = true` persists until manually reset by battle end flow (or next `ResetForNewBattle()`).
- **Camera subscribes globally via OnPlayerSpawned** — if `GameSession.Instance` is auto-created before the scene finishes loading, OnEnable may subscribe prematurely.
