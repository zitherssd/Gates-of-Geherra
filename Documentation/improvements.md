# Gates of Gehera — Codebase Improvement Report

> **Date:** 2026-07-27
> **Scope:** Full codebase review against game-development best practices (skill domains)
> **Priority levels:** 🔴 Critical | 🟠 High | 📌 Medium | 💡 Low/Suggestion

---

## Table of Contents

1. [🔴 Critical Bugs](#1-critical-bugs)
2. [🟠 High-Priority Issues](#2-high-priority-issues)
3. [Performance & GC Pressure](#3-performance--gc-pressure)
4. [📌 Code Quality & Maintainability](#4-code-quality--maintainability)
5. [Skill Domain: Unity C# Scripting](#5-skill-domain-unity-c-scripting)
6. [Skill Domain: Unity Physics](#6-skill-domain-unity-physics)
7. [Skill Domain: Unity ScriptableObjects](#7-skill-domain-unity-scriptableobjects)
8. [Skill Domain: Game AI](#8-skill-domain-game-ai)
9. [Skill Domain: Game Feel](#9-skill-domain-game-feel)
10. [Skill Domain: Game UI/UX](#10-skill-domain-game-uiux)
11. [Skill Domain: Save Systems](#11-skill-domain-save-systems)
12. [Skill Domain: Camera Systems](#12-skill-domain-camera-systems)
13. [Skill Domain: Audio Design](#13-skill-domain-audio-design)
14. [Skill Domain: Input Systems](#14-skill-domain-input-systems)
15. [Skill Domain: Unity Animation](#15-skill-domain-unity-animation)
16. [Skill Domain: Performance Optimization](#16-skill-domain-performance-optimization)
17. [Skill Domain: Physics Tuning](#17-skill-domain-physics-tuning)
18. [Skill Domain: Procedural Gen / Roguelike / RPG](#18-skill-domain-procedural-gen--roguelike--rpg)
19. [Dead Code & Stubs](#19-dead-code--stubs)

---

## 1. 🔴 Critical Bugs

### 1.1 `Mathf.Clamp` result not assigned — `ActorRuntime.ChangeBuildup()`

**File:** `Assets/Scripts/Battle/Actor/ActorRuntime.cs` — Line ~245

```csharp
public void ChangeBuildup(float value)
{
    currentBuildup += value;
    Mathf.Clamp(currentBuildup, 0, maxBuildup);  // ← result discarded!
}
```

`Mathf.Clamp` is a pure function — it returns a value and does NOT modify the argument. The corrected code should be:

```csharp
currentBuildup = Mathf.Clamp(currentBuildup, 0, maxBuildup);
```

**Impact:** Buildup can go negative or exceed `maxBuildup`, causing UI display bugs and logical errors in any code that checks buildup thresholds. Note that the sister methods `DealPostureDamage` and `DealStaminaDamage` both assign the clamp result correctly — this is an isolated mistake in `ChangeBuildup`.

---

### 1.2 `TrainingManager.AwardRandomStat()` — unreachable `case 3`

**File:** `Assets/Scripts/Game/TrainingManager.cs` — Lines ~115–131

```csharp
int roll = UnityEngine.Random.Range(0, 3);  // returns 0, 1, or 2 (exclusive upper bound)

switch (roll)
{
    case 0: /* +1 STR */ break;
    case 1: /* +1 AGI */ break;
    case 2: /* +1 MND */ break;
    case 3: /* "Training failed" */ break;  // ← NEVER REACHED
}
```

`Random.Range(int, int)` has an exclusive upper bound, so `case 3` is dead code. Either change the range to `Random.Range(0, 4)` or add a `default` fallthrough.

---

### 1.3 `TargetingSystem.ClosestEnemy` — `.First()` throws on empty collection

**File:** `Assets/Scripts/Battle/Actor/Systems/TargetingSystem.cs` — Lines ~85–89

```csharp
public Actor ClosestEnemy
{
    get
    {
        if (actor.isControllable)
            return BattleManager.instance.EnemyActors
                .OrderBy(e => (e.transform.position - actor.transform.position).magnitude)
                .Where(a => !a.Runtime.isDead())
                .First();           // ← throws InvalidOperationException if no enemies alive!
        else
            return BattleManager.instance.PlayerActors[0];  // ← IndexOutOfRange if empty!
    }
}
```

This property is accessed every frame by AI behaviors, targeting logic, and `Actor.Update()`. When all enemies are dead (e.g., during the brief window before `BattleActiveState` transitions to `BattleEndState`), this throws. Use `.FirstOrDefault()` with a null check, or guard with `.Any()`.

---

### 1.4 `InputHandler` — Debug event subscriptions leak in release builds

**File:** `Assets/Scripts/Utility/InputHandler.cs` — Lines ~53–55

```csharp
OnSwipe += delta => { Debug.Log("ONSWIPEEVENT TRIGGERED. DELTA IS " + delta); };
OnClick += pos => { Debug.Log("ONCLICKEVENT TRIGGERED. POSITION IS " + pos); };
OnHold += pos => { Debug.Log("ONHOLDEVENT TRIGGERED. POSITION IS " + pos); };
```

These permanent lambda subscriptions allocate delegate objects and execute on every touch input. Even though `Debug.Log` calls may be stripped in release builds, the delegate wrappers remain. Guard with `#if UNITY_EDITOR` or remove entirely.

---

### 1.5 `Actor.Update()` — `BattleManager.instance` accessed without null check

**File:** `Assets/Scripts/Battle/Actor/Actor.cs` — Line ~98

```csharp
if (BattleManager.instance.enabled == false) return;
```

If an `Actor` exists in a scene without a `BattleManager` (e.g., in the Rest Area or Title scene), this throws a `NullReferenceException`. Should be:

```csharp
if (BattleManager.instance == null || !BattleManager.instance.enabled) return;
```

---

### 1.6 `SaveManager.Awake()` — no duplicate-instance protection

**File:** `Assets/Scripts/Save/SaveManager.cs` — Lines ~12–15

```csharp
void Awake()
{
    DontDestroyOnLoad(this);
    instance = this;
}
```

Unlike `GameSession` (which checks `_instance != null && _instance != this`), `SaveManager` does not guard against duplicates. If a scene contains a `SaveManager` and one is already alive via `DontDestroyOnLoad`, the old one is orphaned and `instance` is silently overwritten. Add a singleton guard:

```csharp
if (instance != null && instance != this) { Destroy(gameObject); return; }
instance = this;
DontDestroyOnLoad(gameObject);
```

---

### 1.7 `ItemEventBus` — static dictionary leaks actor references

**File:** `Assets/Scripts/Battle/Items/ItemEventBus.cs`

```csharp
private static readonly Dictionary<ItemTrigger, List<(Actor owner, IItemEffect effect)>> listeners = new();
```

There is no mechanism to clean up subscriptions when an `Actor` is destroyed. If an actor dies and its body is destroyed without calling `Unequip()`, the `(owner, effect)` tuple remains in the static dictionary forever, preventing GC of the `Actor` reference and causing potential null-reference calls during future `Raise()` events.

---

## 2. 🟠 High-Priority Issues

### 2.1 `BaseSkill.CalculateDuration()` — reflection on private field, always returns 0

**File:** `Assets/Scripts/Battle/Actions/BaseSkill.cs` — Lines ~41–76

```csharp
public float CalculateDuration(Animator animator)
{
    BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
    FieldInfo eventsField = typeof(AnimationClip).GetField("m_Events", flags);
    // ...
    float duration = 0f;  // ← never modified, always returns 0
    return duration;
}
```

This method:
- Uses reflection to read private field `m_Events` — brittle across Unity versions
- Always returns `0f` (the `duration` variable is declared but never assigned from any computation)
- Has no callers that use the return value

Either implement the calculation properly (iterate clip events to find the last event time) or remove the method entirely.

---

### 2.2 `Actor` events — no systematic unsubscribe on `OnDestroy()`

**File:** `Assets/Scripts/Battle/Actor/Actor.cs` — Lines ~37–42

```csharp
public event Action<DamageInstance> OnBeforeTakeDamage;
public event Action<DamageInstanceResult> OnAfterTakeDamage;
public event Action<DamageInstance> OnBeforeDealDamage;
public event Action<DamageInstance> OnAfterDealDamage;
public event Action<BaseAction> OnActionUsed;
```

Sub-systems like `EffectManager`, `AudioManager`, `MovementSystem`, and `TargetingSystem` subscribe to these events in their constructors. When an `Actor` GameObject is destroyed (e.g., enemy death), these sub-systems are NOT disposed. There is no `OnDestroy()` on `Actor` that cleans up these subscriptions. This causes:
- Leaked event subscriptions on dead actors
- Ghost `ItemEventBus.Raise()` calls referencing destroyed bodies

**Fix:** Add an `OnDestroy()` method that disposes sub-systems (implement `IDisposable` on each) and nulls events.

---

### 2.3 `BlockStatus` subscribes to `BattleManager.OnNewTurn` — never unsubscribes

**File:** `Assets/Scripts/Battle/Status/BlockStatus.cs`

```csharp
BattleManager.instance.OnNewTurn += Remove;
```

The `Remove()` method calls `base.Remove()` (which is empty in `BaseStatus`). The `-=` unsubscription is never performed. If `BlockStatus` is removed through any path other than the turn event, the subscription leaks indefinitely.

---

### 2.4 Duplicate `Status` directories — RESOLVED

The legacy `Assets/Scripts/Battle/Status/` (old status system: `StatusManager.cs`, `StatusEffect.cs`, `PoisonStatus.cs`, `EffectLogic/*`) was **removed** during the folder restructure (verified unreferenced in code and scenes). The live system was moved up from `Battle/Components/Status/` to `Assets/Scripts/Battle/Status/` with namespace `Assets.Scripts.Battle.Status`. The duplicate-definition issue is gone.

---

### 2.5 `ActorSaveData` stat naming — `CON` stores Agility, `AGI` stores Mind

**File:** `Assets/Scripts/Save/SaveData.cs` — Lines ~53–56

```csharp
save.STR = ad.Strength;       // Strength → STR  ✓
save.CON = ad.Agility;        // Agility → CON   ✗ (CON implies Constitution)
save.AGI = ad.Mind;           // Mind → AGI       ✗ (AGI implies Agility)
save.Spirit = ad.Spirit;      // Spirit → Spirit  ✓
```

The round-trip works because save and load use the same wrong mapping, but:
- Anyone reading the JSON file is misled about which field stores which stat
- Adding future features that read specific stats from save data will have bugs
- The `ActorDefinition` has proper `Strength`, `Agility`, `Mind`, `Spirit` fields — the save format invents a different naming scheme

**Fix:** Rename `CON` → `Agility`, `AGI` → `Mind` in `ActorSaveData` and update both `FromActor()` and `LoadInto()`.

---

### 2.6 AI tick cooldown disabled — AI evaluates every frame

**File:** `Assets/Scripts/Battle/Actor/AI/AIBT.cs` — Lines ~114–117

```csharp
//aiTickTimer -= Time.deltaTime;
//if (aiTickTimer > 0) return;
//aiTickTimer = aiTickCooldown;
```

The throttle mechanism is commented out, so every enemy's AI runs every frame. With multiple enemies, this means many LINQ-heavy evaluations per frame. Uncomment and tune `aiTickCooldown` (0.05s = ~20 evaluations/sec is reasonable).

---

## 3. Performance & GC Pressure

### 3.1 Heavy LINQ in `TargetingSystem.Update()` — called every frame

- `.Where(actor => !actor.Runtime.isDead()).ToList()` allocates a new `List<Actor>` every frame
- `.OrderBy(...).First()` sorts the entire collection just to find the nearest enemy
- `LargestDirectionFromEnemies()` and `GetWeightedAverageEnemyPosition()` each re-query and re-allocate

**Fix:** Cache enemy lists in `BattleManager` and update on add/remove. Replace `OrderBy().First()` with a manual `for` loop tracking the minimum distance.

### 3.2 LINQ in AI behavior nodes — called every AI tick

Every `AttackWithValidSkill`, `DodgeBehavior`, `BlockBehavior`, etc. uses queries like:
```csharp
actor.Runtime.actions.OfType<MoveAction>().FirstOrDefault()
actor.Runtime.actions.Where(skill => skill is AttackSkill && ...).OrderBy(x => Random.value).FirstOrDefault()
```

These allocate enumerators and temp collections. Pre-categorize actions at init time (e.g., `attackActions`, `movementActions`, `defenseActions` lists on `ActorRuntime`).

### 3.3 `new List<Vector3>` per hitbox check — hot path allocation

`AttackSkill`, `ShowHitboxOnPlayer`, and `ShowHitboxOnPoint` allocate new `List<Vector3>` for `transformedPoints` every time an attack lands. Pool or reuse these lists.

### 3.4 `CameraManager` — O(n²) over all actor pairs every frame

`CameraManager.GetBattleSideAxis()` iterates all actor pairs. With even 6–8 actors, this is wasteful per frame. Cache or compute on change.

### 3.5 `new System.Random()` instantiated repeatedly

- `BaseSkill.RandomFromList()` creates `new System.Random()` per call
- `SoundManager` creates `new System.Random()` per audio play
- `BaseSkill.WaitForOneFrame()` yields `new WaitForSeconds(0.01f)` — should cache the `YieldInstruction`

`new System.Random()` seeded by system time can produce identical sequences if called rapidly. Use `UnityEngine.Random` or a shared `System.Random` instance.

### 3.6 `GameObject.Find` calls in `Start()`/`Awake()`

- `UIManager.Start()` — `GameObject.FindGameObjectWithTag("OriginPoint")`
- `ActionButtonBattle.Awake()` — `GameObject.Find("LeftContainer")`, etc.
- `BattleManager.Enter()` — `GameObject.Find("Level_" + battleDefinition.Level)`
- `JoystickManager.Awake()` — `GameObject.Find("VirtualJoystickBase")`, etc.

These are expensive string scans of the entire hierarchy. Use serialized references (`[SerializeField]`) or a registry pattern instead.

---

## 4. 📌 Code Quality & Maintainability

### 4.1 Public fields where `[SerializeField] private` should be used

Many classes expose fields as `public` that are only needed for Inspector serialization:

| File | Field | Should be |
|------|-------|-----------|
| `UIManager.cs` | `public GameObject OriginPoint` | `[SerializeField] private` |
| `BattleManager.cs` | `public GameObject enemyPrefab` | `[SerializeField] private` |
| `CameraManager.cs` | `public Transform playerTransform` | `[SerializeField] private` |
| `GameSession.cs` | ALL fields public | Consider properties or `[SerializeField] private` |
| `ActorRuntime.cs` | ALL fields public | Consider properties with getters/setters |

### 4.2 String-based `Animator.Play()` calls

- `ActingState.Set()` — `animator.Play("RollBackwards")`, `animator.Play("Roll")`, `animator.Play(skill.Animation.ToString())`
- `Actor.PlayAnimation()` — `animator.Play(AnimationName)`

Use `Animator.StringToHash()` for all animation names to avoid string allocations and typos.

### 4.3 No `SaveData` version field

**File:** `Assets/Scripts/Save/SaveData.cs`

```csharp
[Serializable]
public class SaveData
{
    // No version field!
    public int currentFloor;
    // ...
}
```

Any schema change will silently break existing saves. Add an `int saveVersion` field and a migration path.

### 4.4 `GameObject.Find` string concatenation

```csharp
GameObject.Find("Level_" + battleDefinition.Level)
```

This is fragile — if the scene hierarchy changes or `Level` is an unexpected value, the lookup fails silently. Use the `SpawnGroup` system instead.

### 4.5 `UIManager.GetCurrentLoadout()` can return empty list on error

If `player` or `player.Runtime` is null, the method logs an error and returns an empty list. Callers like `SaveManager.SaveToSlot()` then overwrite the saved loadout with nothing. Add a guard: if the loadout can't be captured, don't overwrite the existing saved data.

### 4.6 `ActorRuntime` `GetCurrentHP()` uses LINQ `LastOrDefault()`

```csharp
var lastBar = hpBars.Where(item => item.alive).LastOrDefault();
```

This allocates an enumerator. Since `hpBars` is typically small (1–3), a `for` loop iterating backwards would be more efficient and allocation-free.

---

## 5. Skill Domain: Unity C# Scripting

### 5.1 Missing `RequireComponent` attributes

`Actor` has `[RequireComponent(typeof(StatusManager))]` but does NOT have `[RequireComponent(typeof(Rigidbody))]`, `[RequireComponent(typeof(Animator))]`, or `[RequireComponent(typeof(ActorStateMachine))]` even though `Awake()` calls `GetComponent<>()` for these. Adding them ensures the dependencies can never be missing and provides auto-add in the Editor.

### 5.2 `new AudioManager(this)` in `Awake()` — not a MonoBehaviour

`AudioManager`, `EffectManager`, `TargetingSystem`, `MovementSystem`, `ActorInventory` are all plain C# classes (not MonoBehaviours) subscribed to by the Actor. They lack `IDisposable`/cleanup patterns. The Actor should call a cleanup method when destroyed.

### 5.3 `TryGetComponent` not used

Multiple places use `GetComponent<T>()` + null check. Prefer `TryGetComponent<T>(out var component)` — it avoids the null allocation pattern and is the modern Unity idiom.

### 5.4 `StartCoroutine` called on non-MonoBehaviours

`BattleManager.StartCoroutine()` (a MonoBehaviour) is used from `BattleEndState` (a plain class). This works because the reference is passed in, but it's worth noting the pattern depends on the manager never being destroyed mid-coroutine.

---

## 6. Skill Domain: Unity Physics

### 6.1 Global gravity override in `BattleManager.Start()`

```csharp
Physics.gravity = new Vector3(0, -6f, 0);
```

This modifies the global gravity setting, which affects ALL Rigidbodies in the project. If other scenes have different gravity needs (e.g., the Title screen), they'll inherit this value. Restore default gravity on scene exit or set per-scene via project physics settings instead.

### 6.2 `MovementSystem` uses `agent.updatePosition = false` with custom position sync

```csharp
// Actor.FixedUpdate()
movement.agent.nextPosition = transform.position;
```

This pattern (NavMeshAgent with manual Rigidbody control) is valid but fragile. If the agent and Rigidbody positions diverge, the agent may try to walk back to its NavMesh position. Ensure `agent.updatePosition = false` and `agent.updateRotation = false` are set consistently.

### 6.3 `IsGrounded()` raycast is very short

```csharp
Ray ray = new(transform.position + Vector3.up * 0.01f, Vector3.down);
if (Physics.Raycast(ray, 0.02f))
```

The ray is only 0.02 units long. With the reduced gravity (-6), actors may not detect ground correctly at higher fall speeds. Consider a longer ray (0.1–0.2 units) and a `LayerMask` to exclude the actor's own collider.

---

## 7. Skill Domain: Unity ScriptableObjects

### 7.1 GUIDs may not be unique across assets

`BaseAction.guid` and `ActorDefinition.guid` are `string` fields. There's no validation that GUIDs are unique. If two assets share a GUID, `ActionDatabase` will return inconsistent results. Consider:
- Using `System.Guid.NewGuid().ToString()` programmatically in an `OnValidate()` / custom Editor
- Adding an `AssetPostprocessor` that checks for duplicates

### 7.2 `ActorDefinition` base stats are mutable at runtime via SO reference

ScriptableObjects are shared assets. If an `ActorDefinition` is directly modified at runtime (the SO itself, not a clone), the change persists in the Editor across play sessions. The `ActorRuntime` clones actions via `Instantiate`, but the base stats on `ActorDefinition` are read directly — they should be treated as read-only config.

### 7.3 No `Reset()` on shared runtime ScriptableObject variables

The `FloatVariable` pattern (from the skill) uses `OnEnable()` to reset `runtimeValue = initialValue`. The project doesn't use this pattern, but if shared SO variables are added later, ensure they reset on play mode exit.

---

## 8. Skill Domain: Game AI

### 8.1 Behavior tree is rigid — no blackboard or shared state

The BT implementation (`AIBT` + `BTNode`) uses inline logic in condition/behavior classes, but there is no **blackboard** — a shared data store that nodes can read/write. This makes it harder to share state (like "am I flanking?") between nodes.

### 8.2 `ConditionNode` uses `Debug.WriteLine()` (does nothing in Unity)

```csharp
Debug.WriteLine("HesitateCondition is evaluating...");
```

`System.Diagnostics.Debug.WriteLine()` writes to the trace listeners, not the Unity Console. Use `Debug.Log()` instead.

### 8.3 `AiRuleset.Ninja` case does nothing

```csharp
case AiRuleset.Ninja:
    break;  // empty — no behavior list assigned
```

The Ninja AI ruleset has no behaviors and will do nothing. Either implement or remove the enum value.

### 8.4 No debug visualization

The behavior tree has no visualizer, no node-status overlay, and no debug logging for which node is currently active. Adding `BTNode.status` (SUCCESS/FAILURE/RUNNING) and a gizmo or console log would greatly aid AI tuning.

### 8.5 `HesitateCondition` with `GlobalAttackTokenCondition` — race risk

The global attack token is an interesting design, but there's no timeout or fallback. If an enemy acquires the token but is then interrupted (stagger, death), the token is held until it expires or the enemy acts again. Consider a release-on-interrupt mechanism.

---

## 9. Skill Domain: Game Feel

### 9.1 `SlowdownManager` is well-implemented but has one issue

The state-based slowdown blocks temporary slowdowns, which is correct design. However, `TriggerTemporarySlowdown()` does not check if `StateSlowdownActive` is changing during the temporary window, which could cause unexpected time-scale resets.

### 9.2 Screen shake values are hardcoded

`CameraManager` shake parameters (magnitude, duration, decay) appear to be hardcoded rather than exposed as tuning parameters. Consider making them `[SerializeField]` fields with tooltips.

### 9.3 Damage popup system (`DamagePopup`) could be pooled

If multiple hits land in quick succession (e.g., `DamageTickEffect`), many popup GameObjects are instantiated. An object pool for damage number popups would reduce GC pressure.

### 9.4 Hit-stop is implemented via slowdown — consider freeze-frame alternative

The current hit-stop is a temporary slowdown (`TriggerTemporarySlowdown(0.4f)`). For heavier impacts, consider a true freeze-frame (setting `Time.timeScale = 0` for a brief real-time-delayed period) to sell the hit, but keep it short (< 0.1s).

---

## 10. Skill Domain: Game UI/UX

### 10.1 No responsive scaling or safe-area handling

The project is targeting mobile + WebGL but:
- No `CanvasScaler` with `Scale With Screen Size` explicitly configured (check if this is set on the Canvas)
- No safe-area handling for notches/rounded corners on mobile
- No resolution/aspect-ratio testing

### 10.2 Action button containers use `GameObject.Find()` lookup

`ActionButtonBattle` finds containers by name at runtime. If container names change, the UI breaks silently. Use serialized references.

### 10.3 `UIManager` uses `GameObject.FindGameObjectWithTag("OriginPoint")`

Tags are editor settings that can get out of sync. Prefer a direct `[SerializeField]` reference.

### 10.4 HUD updates may be polling-heavy

The UI code doesn't show an event-driven pattern for HUD updates. If HUD elements (HP bars, stamina, etc.) are updated in `Update()` rather than in response to events, this is wasteful. Ensure HUD elements subscribe to events like `OnAfterTakeDamage` rather than polling every frame.

### 10.5 No keyboard/gamepad navigation for menus

The UI uses touch input but there's no apparent keyboard/gamepad focus navigation for menus (e.g., the Start screen, skill selection cards). This makes the game less accessible and breaks controller-only play.

---

## 11. Skill Domain: Save Systems

### 11.1 No save versioning — any schema change breaks saves

As noted in [4.3](#43-no-savedata-version-field). This is the single most impactful save-system risk. Add versioning before shipping.

### 11.2 Save is not atomic — crash risk

```csharp
File.WriteAllText(GetSlotPath(slot), json);
```

This writes directly to the target file. A crash mid-write produces a truncated, unloadable save file. Use the temp + rename pattern:

```csharp
string tmp = GetSlotPath(slot) + ".tmp";
File.WriteAllText(tmp, json);
File.Delete(GetSlotPath(slot));
File.Move(tmp, GetSlotPath(slot));
```

### 11.3 `JsonUtility` + polymorphic `List<BaseItem>` — potential serialization bug

```csharp
public List<BaseItem> items;  // BaseItem is the abstract base of Consumable, Trinket, etc.
```

`JsonUtility.FromJson` does NOT natively support polymorphic serialization. When deserializing a `List<BaseItem>` that contains `BaseConsumable` or `BaseTrinket` instances, the subtype-specific fields may be lost. This needs testing with actual consumable/trinket items to confirm.

### 11.4 `ActionDatabase` must be initialized before save load

`SaveManager.Start()` calls `actionDatabase.Initialize()`. If any code attempts to load a save before `Start()` runs (e.g., in an `Awake()`), GUIDs won't resolve. The initialization order in `Architecture.md` confirms this works for the current start-up path, but it's fragile — guard with a `isInitialized` flag.

### 11.5 No save encryption or obfuscation

JSON save files are plain text at `Application.persistentDataPath`. Players can trivially edit their save to give themselves max stats. Consider at minimal a CRC/hash to detect tampering, or full encryption for a shipping product.

---

## 12. Skill Domain: Camera Systems

### 12.1 Heavy per-frame computation

`CameraManager.Update()` calls `GetBattleFocusPoint()` and `GetBattleSideAxis()` every frame. The latter is O(n²) over all actor pairs. For a mobile game at 30–60 FPS, this will eat frame budget.

### 12.2 No deadzone or look-ahead

The camera follows targets but there's no apparent deadzone (to ignore micro-movements) or look-ahead (to lead the action). This can cause jittery camera motion in fast-paced combat.

### 12.3 No camera smoothing fallback if `BattleManager` is null

`CameraManager` likely accesses `BattleManager.instance` in its update logic. If the camera exists outside a battle scene (e.g., in the Rest Area), it needs a non-battle follow mode. Ensure there's a graceful fallback.

---

## 13. Skill Domain: Audio Design

### 13.1 `SoundManager` creates `new System.Random()` per call

Each audio play creates a fresh RNG instance. Use a static shared `System.Random` or `UnityEngine.Random`.

### 13.2 No ducking or audio bus architecture

There's no evidence of audio ducking (music dips when SFX plays) or bus-based mixing. For a polished game, route sounds through buses (`Master → {Music, SFX, Voice, UI}`) and apply sidechain compression for ducking.

### 13.3 `AudioManager` subscriptions leak on actor death

`AudioManager` subscribes to `OnAfterTakeDamage` in its constructor but never unsubscribes. When an enemy actor is destroyed, the `AudioManager` instance (a plain C# object) becomes unreachable, but its subscription remains on the actor's event. If the actor object itself leaks, the audio callback references a dead manager.

### 13.4 No SFX variation for repeated sounds

Repeated sounds (footsteps, hits) likely play the same clip at the same pitch. Implement pitch randomization (±0.1 semitones) and sample pool rotation to avoid "machine-gun" audio artifacts.

---

## 14. Skill Domain: Input Systems

### 14.1 No action-based input abstraction

The project uses Unity's Input System but the input handling appears to use touch-position direct readings rather than named actions. Consider wrapping input in named actions (`Attack`, `Block`, `Dodge`, `Move`) for rebindability and multi-device support.

### 14.2 Debug subscriptions in release builds (see [1.4](#14-inputhandler--debug-event-subscriptions-leak-in-release-builds))

### 14.3 No deadzone for joystick input

If the `OnScreenStick` or joystick doesn't have a radial deadzone, resting drift will register as small movements. Apply a radial deadzone (not per-axis) to the stick input:

```csharp
float magnitude = input.magnitude;
if (magnitude < deadzone) return Vector2.zero;
return input.normalized * ((magnitude - deadzone) / (1f - deadzone));
```

### 14.4 No input buffering or coyote time

For an action game, input buffering (remembering a pressed button for a short window) and coyote time (allowing jump shortly after leaving a ledge) are important feel improvements. Neither is currently implemented.

---

## 15. Skill Domain: Unity Animation

### 15.1 String-based `Animator.Play()` (see [4.2](#42-string-based-animatorplay-calls))

### 15.2 Animation events are fragile with no fallback

The action system depends entirely on animation events (`EnterWindup`, `OnHit`, `EnterRecovery`, `OnEnd`) being keyframed at exact frames. If:
- An animation clip is replaced/edited without the events
- An event is accidentally deleted
- The animation is interrupted

The action may never call `EndAction()`, permanently freezing the actor. Consider:
- Adding a timeout fallback in `ActingState` that forces `EndAction()` if the animation exceeds a max duration
- Validating animation events at edit time (custom Editor script)

### 15.3 `animator.speed` modification pattern is fragile

`ActingState.Exit()` resets `animator.speed = 1f`. However, if `Exit()` is not called (e.g., direct state transition bypassing the normal path), the animator speed stays at `windupTimeMult` or `recoveryTimeMult`, causing all subsequent animations to play at the wrong speed. Ensure all state transitions reset the animator speed.

---

## 16. Skill Domain: Performance Optimization

### 16.1 No object pooling

The project creates/destroys enemies, projectiles, damage popups, and UI action buttons via `Instantiate`/`Destroy`. This causes GC spikes. Priority pooling targets:
- **Enemy bodies** — reuse from a pool instead of destroy/create per battle
- **Projectiles** — `ProjectileHandler` objects are instantiated per shot
- **Damage popups** — can spawn many rapidly with multihit attacks
- **Action button prefabs** — recreated whenever `InitializePlayerActionButtonPrefabs` is called

### 16.2 LINQ queries on hot paths (see [3.1](#31-heavy-linq-in-targetingsystemupdate--called-every-frame) and [3.2](#32-linq-in-ai-behavior-nodes--called-every-ai-tick))

### 16.3 `StatusManager.Tick()` iterates all statuses every frame per actor

With 6+ actors and multiple statuses each, this is regular per-frame overhead. Consider:
- Only ticking statuses that have durations or per-frame effects
- Using a delta-time threshold for cheap statuses

### 16.4 `TargetingSystem.Update()` runs every frame for controllable actors

The targeting system recalculates closest enemy, weighted positions, and direction every frame even when nothing has changed. Consider caching and invalidating on significant events (enemy death, actor move threshold).

---

## 17. Skill Domain: Physics Tuning

### 17.1 Fixed timestep not explicitly configured

The project doesn't explicitly set `Time.fixedDeltaTime` (outside of `SlowdownManager` which modifies it during slowdown). For a combat game, consider configuring the fixed timestep explicitly in project settings (e.g., 50 Hz = 0.02s) to ensure consistent physics across platforms.

### 17.2 `FixedUpdate` is used for NavMeshAgent sync, not physics

```csharp
void FixedUpdate()
{
    movement.agent.nextPosition = transform.position;
    animator.SetFloat(SpeedParam, movement.GetSpeed());
}
```

This is technically appropriate (NavMesh updates in fixed step), but the combined animator update in `FixedUpdate` means visual speed changes only update at the physics rate. For smoother animation speed transitions, also set the animator parameter in `Update()`.

### 17.3 Reduced gravity value

`Physics.gravity = new Vector3(0, -6f, 0)` is about 60% of default (-9.81). This makes jumps floatier and falls slower. If this is by design for the game's feel, document it. If not, tune to match the intended jump arc.

---

## 18. Skill Domain: Procedural Gen / Roguelike / RPG

### 18.1 `SkillGenerator` loads all actions once — not dynamic

```csharp
private void Start()
{
    allActions = Resources.LoadAll<BaseAction>("Actions/Droptable").ToList();
}
```

This loads the droptable once at scene start. If modding or runtime content generation is ever desired, this won't pick up new assets without a scene reload.

### 18.2 `FloorManager.ProgressToNextFloor()` — potential index out of bounds

```csharp
// ProgressToNextFloor accesses StoryBattles[currentFloor - 1]
```

If `currentFloor` is somehow 0 or exceeds `StoryBattles.Count`, this throws. Floor 0 → index -1 (CLR bounds check throws). Floor 6+ → index out of range. Add a bounds check.

### 18.3 `FloorManager.QuickFight()` — no guard against empty `RandomBattles`

```csharp
var randomBattle = RandomBattles[UnityEngine.Random.Range(0, RandomBattles.Count)];
```

If `RandomBattles` is empty, `Random.Range(0, 0)` returns 0, and accessing `[0]` on an empty list throws. Guard with `if (RandomBattles.Count == 0) return`.

### 18.4 No "run complete" condition

There are 5 story floors defined, but no victory/run-complete screen, achievement, or ending state when the 5th floor boss is defeated. The `FloorManager` just increments past floor 5, which triggers the bounds error above.

### 18.5 Stat system is simple but has no leveling

Stats are rolled 3d6 at start and only increased via `TrainingManager`. There's no XP/level-up system for a traditional RPG feel. The training stat awards don't use energy/resource costs (though energy exists in `ActorRuntime`).

---

## 19. Dead Code & Stubs

| File | Status | Notes |
|------|--------|-------|
| `CrawlerManager.cs` | Empty stub | Full `MonoBehaviour` with empty `Start()`/`Update()` — no functionality |
| `ActorBehaviorTree.cs` | Empty stub | Full `MonoBehaviour` with empty `Start()`/`Update()` — appears to be a leftover |
| `SkillGenerator.cs` — commented-out methods | ~100 lines of dead code | `UpgradeSkillAtRandom()` and `UpgradeReactionAtRandom()` fully commented out |
| `Battle/Status/*` (legacy) | Removed | Old duplicate status system deleted in folder restructure; live system at `Battle/Status/` |
| `BaseSkill.CalculateDuration()` | Always returns 0 | Reflection-based method with no actual calculation |
| `TrainingManager.case 3` | Dead code | Unreachable due to `Random.Range(0, 3)` exclusive upper bound |
| `AiRuleset.Ninja` | Empty behavior | No behaviors assigned — does nothing |


---

## Summary by Priority

| Priority | Count | Key Areas |
|----------|-------|-----------|
| 🔴 Critical | 7 | Runtime bugs, null safety, memory leaks, dead code paths |
| 🟠 High | 6 | Event leaks, perf, AI throttling, save schema, duplicate code |
| 📌 Medium | 6 | Code quality, public fields, `GameObject.Find`, string anim params |
| 💡 Suggestions | Many | Object pooling, audio ducking, input buffering, safe areas |

---

## Appendix: Files with Highest Improvement Potential

1. **`Actor.cs`** — Event cleanup, null safety, missing `RequireComponent`, pooling
2. **`ActorRuntime.cs`** — `Mathf.Clamp` bug, `GetCurrentHP()` LINQ, stat access patterns
3. **`TargetingSystem.cs`** — `.First()` crash, LINQ allocation, per-frame computation
4. **`SaveData.cs`** — Stat naming, versioning, atomic writes
5. **`AIBT.cs`** — Tick cooldown disabled, Ninja behavior empty, no blackboard
6. **`ItemEventBus.cs`** — Static dictionary leak, no cleanup path
7. **`TrainingManager.cs`** — Dead `case 3`, unreachable failure path
8. **`BattleManager.cs`** — Global gravity, `GameObject.Find`, singleton pattern
9. **`CameraManager.cs`** — O(n²) computation, hardcoded shake, no deadzone
10. **`SoundManager.cs`** — `new Random()` per call, no ducking, no pooling
