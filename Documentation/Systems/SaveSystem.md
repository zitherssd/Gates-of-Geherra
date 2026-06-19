# Save System

## Purpose

Provides persistent run-state across game sessions using JSON serialization to disk (or PlayerPrefs on WebGL).

---

## Responsibilities

- Serialize actor state (HP bars, stats, stamina, posture, buildup, items) to disk.
- Persist action loadout (which action is in which UI slot).
- Restore full player state on game load.
- Delete save on player death.
- Support multiple save slots (single slot in current usage).

---

## Main Scripts

| Script | Role |
|--------|------|
| `SaveManager.cs` | Orchestrates all save/load; DontDestroyOnLoad singleton |
| `SaveData.cs` | Root serializable container |
| `ActorSaveData.cs` | Actor state serializer/deserializer |
| `ActionSlotSaveData.cs` | Single action slot: containerID + slotIndex + actionGuid |
| `ActionDatabase.cs` | GUID → BaseAction lookup; initialized by SaveManager |
| `GameSession.cs` | Holds live state between saves; owns PlayerRuntime |

---

## Data Structures

### SaveData
```
SaveData {
    int currentFloor
    int trainingsDone
    int trainingsDoneThisFloor
    ActorSaveData player
    List<TimeLock> timelocks
}
```

### ActorSaveData
```
ActorSaveData {
    string actorId          // ActorDefinition GUID
    string Name
    List<HpBar> hpBars
    float maxBuildup/Posture/Stamina
    float currentStamina/Posture/Buildup
    int STR, CON, AGI, Spirit   // NOTE: CON maps to Agility, AGI maps to Mind (naming mismatch in code)
    float postureRegenRate/staminaRegenRate
    List<ActionSlotSaveData> actions   // UI slot layout
    List<BaseItem> items
}
```

### ActionSlotSaveData
```
ActionSlotSaveData {
    string ContainerID    // "Left", "LeftSec", "Right", "RightSec"
    int SlotIndex
    string ActionGuid     // resolved via ActionDatabase
}
```

---

## Save Triggers

| Trigger | Location |
|---------|----------|
| New game created | `GameFlowManager.Start()` → `SaveManager.SaveToSlot()` |
| Battle victory | `BattleEndState.Enter()` → after skill selection callback |
| Item used | `ActorInventory.UseConsumable()` → `SaveManager.SaveToSlot()` |

---

## Load Flow

```
GameFlowManager.Start()
  → SaveManager.SlotExists(slot)?
    → YES: SaveManager.LoadFromSlot(slot)
             → LoadGame(slot) → JsonUtility.FromJson<SaveData>
             → Restore floor, trainings, timelocks to FloorManager + GameFlowManager
             → return SaveData
           EnsurePlayerBody() → create body if needed
           SaveData.LoadActor(playerActor)
             → ActorSaveData.LoadInto(save, actor)
               → Restore hpBars, stats, stamina, posture, buildup
               → ActionDatabase.Lookup(guid) per ActionSlotSaveData
           GameSession.AdoptPlayerRuntime(actor.Runtime)
    → NO (new game): GameSession.StartNewRun(playerName)
```

---

## Platform Differences

| Platform | Storage | Key |
|----------|---------|-----|
| PC / Editor | `Application.persistentDataPath/save_slot_{n}.json` | File I/O |
| WebGL | `PlayerPrefs` | `"save_slot_" + slot` |

---

## External Dependencies

- `GameSession` — source of truth for run data
- `UIManager.GetCurrentLoadout()` — captures current slot layout at save time
- `FloorManager.currentFloor` — delegated to GameSession
- `ActionDatabase` — resolves action GUIDs back to ScriptableObject references

---

## Extension Points

- Add new saved fields: extend `SaveData` or `ActorSaveData` with `[Serializable]` fields.
- Add auto-save triggers: call `SaveManager.SaveToSlot(currentSaveSlot)` at new trigger sites.
- Support multiple save slots: plumb slot selection UI and change `SaveManager.currentSaveSlot`.

---

## Risks

- **Stat field naming mismatch**: `ActorSaveData.CON` stores Agility; `ActorSaveData.AGI` stores Mind. This is a legacy naming bug — any new code reading these fields must use the same reversed mapping.
- **JsonUtility limitations**: No support for polymorphic types — `BaseItem` subtype list may not serialize correctly if subtypes have unique fields. [UNVERIFIED — needs testing with Consumable/Trinket items]
- **No save versioning**: Schema changes break existing saves. No migration path exists.
- **ActionDatabase must be initialized before load**: `SaveManager.Start()` calls `actionDatabase.Initialize()`; if save is loaded before that (e.g. `Awake()`), GUIDs won't resolve.
- **Single save slot**: Only one active slot; `currentSaveSlot` is an int but UI for slot selection is not implemented.
