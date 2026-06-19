# Item System

## Purpose

Provides collectible items that passively or actively modify actor behavior during a run.

---

## Responsibilities

- Define item data (name, icon, description) via ScriptableObjects.
- Execute item effects via the `IItemEffect` interface.
- Manage per-actor item inventory with equip/unequip lifecycle.
- Route trigger-based item effects via a static event bus (`ItemEventBus`).
- Support recipe-based item crafting.

---

## Main Scripts

| Script | Role |
|--------|------|
| `BaseItem.cs` | Base ScriptableObject; name, icon, stackable |
| `BaseConsumable.cs` | Adds `List<IItemEffect> Effects`; fired once on use |
| `BaseTrinket.cs` | Passive item; subscribes effects to `ItemEventBus` on equip |
| `ActorInventory.cs` | Manages actor's item list; AddItem auto-equips trinkets |
| `ItemEventBus.cs` | Static event bus: `Subscribe/Unsubscribe/Raise(trigger)` |
| `ItemTrigger.cs` | Enum of trigger events |
| `CraftingSystem.cs` | Static `TryCraft()` utility; no current UI integration |

---

## Item Type Hierarchy

```
BaseItem (SO)
├── BaseConsumable (SO)       - one-time use effects
└── BaseTrinket (SO)          - passive, trigger-driven effects
```

---

## Item Triggers (ItemTrigger enum)

| Trigger | Raised By |
|---------|-----------|
| `OnNewBattle` | `BattleStartState.Enter()` |
| `OnNewFloor` | `FloorManager.ProgressToNextFloor()` |
| `OnDamageTaken` | `Actor.ApplyDamageInstance()` |

---

## Effect Interfaces Used

- `IItemEffect` — `void Eval(Actor owner)` — all item effects implement this
- Shared with action effects; classes like `HealEffect`, `PlayParticleEffect`, `DamageEffect` implement both `IEffect` and `IItemEffect`

---

## Known Item Assets (`Resources/Items/`)

| Asset | Presumed Type | Notes |
|-------|--------------|-------|
| `BlueOrb.asset` | BaseItem (material?) | [UNVERIFIED effect] |
| `HealingPot.asset` | BaseConsumable | Likely heals HP |
| `InnerFire.asset` | BaseTrinket | [UNVERIFIED trigger/effect] |

---

## Data Sources

- `ActorDefinition.startingItems` → initial items placed in inventory at spawn
- `BattleDefinition.RewardItemsPools` → post-battle item reward pools
- `ActorRuntime.items` → shared list (shared reference between Actor body and GameSession)

---

## Events

- `ItemEventBus.Raise(trigger)` — fires all subscribed effects for that trigger
- `ItemEventBus.Raise(trigger, specificOwner)` — fires only for one actor (e.g. `OnDamageTaken`)

---

## Inventory Lifecycle

```
Actor.Start()
  → if not bound: foreach(item in Runtime.items) inventory.AddItem(item)
  
Actor.Bind(runtime)  [arena scene player re-spawn]
  → inventory.RebindTo(runtime.items)
    → re-equips all trinkets on the new body

inventory.AddItem(item)
  → Items.Add(item)
  → if BaseTrinket: trinket.Equip(owner) → subscribe to ItemEventBus

inventory.RemoveItem(item)
  → Items.Remove(item)  
  → if BaseTrinket: trinket.Unequip(owner) → unsubscribe from ItemEventBus

inventory.UseConsumable(item)
  → foreach effect: effect.Eval(actor)
  → RemoveOne/Remove based on stackable flag
  → SaveManager.SaveToSlot(currentSaveSlot)
```

---

## External Dependencies

- `SaveManager` — called after consumable use
- `BattleEndState` — awards items from `RewardItemsPool`

---

## Extension Points

- Add new consumable: create `BaseConsumable` asset, add `IItemEffect` list in Inspector.
- Add new trinket: create `BaseTrinket` asset, set `Triggers` + `Effects`.
- Add new trigger: extend `ItemTrigger` enum, add `ItemEventBus.Raise()` call at the appropriate site.
- Add crafting UI: wire `CraftingSystem.TryCraft()` to a UI element; `Recipe` SO already exists.

---

## Risks

- **`ItemEventBus` is static and global**: Subscribing trinkets from dead/removed actors will continue receiving events unless `Unequip()` is explicitly called. If `Actor` is destroyed without cleanup, effects may fire on null owners.
- **`JsonUtility` polymorphism limitation**: `ActorSaveData.items` is `List<BaseItem>` — subtype-specific fields of `BaseConsumable`/`BaseTrinket` may not round-trip correctly through JsonUtility.
- **No max item cap**: The inventory has no slot limit; uncapped growth is possible in a long run.
- **Crafting has no UI**: `CraftingSystem` is implemented but not wired to any user interaction.
