# Audio System

## Purpose

Plays and manages background music and sound effects throughout the game.

---

## Responsibilities

- Play music tracks (combat and rest variants).
- Fade out music on scene transitions.
- Play one-shot sound effects by name.
- Integrate with scene transitions and game events.

---

## Main Scripts

| Script | Role |
|--------|------|
| `SoundManager.cs` | Singleton; exposes `PlayMusic`, `PlayMusicRest`, `PlaySE`, `FadeOutMusic` |
| `AudioManager.cs` | Per-actor audio; handles actor-specific sounds (hit sounds, etc.) — class, not MonoBehaviour |
| `Battle/Components/Audio/` | [UNVERIFIED — contains actor audio component(s)] |

---

## Data Sources

[UNVERIFIED — audio clip assignments not reviewed; likely serialized fields on SoundManager]

---

## Events

None emitted by `SoundManager`. Called directly.

---

## Usage Locations

| Call | Location |
|------|----------|
| `SoundManager.PlayMusicRest()` | `RestAreaManager.Enter()` |
| `SoundManager.PlayMusic(null)` | `BattleManager.Enter()` (stops music before battle) |
| `SoundManager.PlaySE("Gong")` | `FloorManager.ProgressToNextFloor()`, `QuickFight()` |
| `SoundManager.FadeOutMusic()` | `BattleEndState.Enter()` |

---

## External Dependencies

- `SoundManager.instance` — singleton referenced directly by managers

---

## Risks

- **"Gong" sound effect hardcoded as string** — typos or missing clips fail silently.
- **SoundManager details [UNVERIFIED]**: Clip library, volume control, and layering logic were not reviewed.
