# Gates of Gehera — Data Architecture

## ScriptableObject Types

### ActorDefinition
**Menu**: `ScriptableObjects/Actor`  
**Script**: `Scripts/Battle/Actor/ActorDefinition.cs`  
**Purpose**: Defines all static properties of an actor (player or enemy).  

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `guid` | string | Unique ID for save reference |
| `Name` | string | Display name |
| `hpBars` | `List<HpBar>` | Multiple HP bars (boss layers) |
| `baseMaxBuildup` | float | Base buildup resource cap |
| `baseMaxPosture` | float | Base posture cap |
| `baseMaxStamina` | float | Base stamina cap |
| `Strength` | int | +maxPosture bonus |
| `Agility` | int | +maxStamina×2 bonus |
| `Mind` | int | +maxBuildup bonus |
| `Spirit` | int | Purpose [UNVERIFIED] |
| `postureRegenRate` | float | Posture regen multiplier |
| `staminaRegenRate` | float | Stamina regen multiplier |
| `Controllable` | bool | True = player-controlled |
| `AIRuleset` | `AiRuleset` enum | Which BT behavior set to use |
| `mainColor` | Color | Primary tint |
| `secondaryColor` | Color | Secondary tint |
| `startingItems` | `List<BaseItem>` | Items in inventory at spawn |
| `baseActions` | `List<BaseAction>` | Cloned into ActorRuntime on init |

**Known Assets** (`Resources/Actors/`):
| Asset | Notes |
|-------|-------|
| `MC.asset` | Player character — Controllable=true |
| `MC_Sandbox.asset` | Sandbox player variant |
| `MC_Uncontrollable.asset` | Non-player MC (cutscene?) |
| `Boxer.asset` | Enemy |
| `Enraged Maniac.asset` | Enemy — `AiRuleset.Maniac` |
| `Malnourished Individual.asset` | Enemy |
| `Old Prisoner.asset` | Enemy — `AiRuleset.OldMan` |
| `Shuriken Thrower.asset` | Enemy — `AiRuleset.ShurkienThrower` |
| `Tutorial Guy.asset` | Tutorial enemy |

---

### BaseAction (Abstract ScriptableObject)
**Menu**: Multiple sub-type menus  
**Script**: `Scripts/Battle/Actions/BaseAction.cs`  
**Purpose**: Defines a single skill/move. Cloned per actor runtime.  

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `guid` | string | Unique ID for save/database lookup |
| `Type` | `BUTTONTYPE` | INSTANT / VECTOR / CONTINUOUS / CONTINUOUS_VECTOR |
| `Name` | string | Display name |
| `Rarity` | `RARITY` | [UNVERIFIED enum values] |
| `Description` | string | Tooltip text |
| `CooldownTimer` | float | Seconds between uses |
| `TotalUses` | int | 0 = unlimited |
| `BuildupCost` | int | Buildup spent on use |
| `BuildupGain` | int | Buildup gained on use |
| `StaminaCost` | int | Stamina spent on use |
| `Tags` | `List<TAG>` | Behavior modifiers |
| `StickMult` | float | Joystick force multiplier |

**Known Action Assets** (`Resources/Actions/Attacks/`):
- `Jab_Starter.asset`, `Jab_Fast.asset`, `Jab_RHook.asset`
- `Kick_High.asset`, `PalmStrike.asset`

**Known Droptable Assets** (`Resources/Actions/Droptable/`):
- `CageKick`, `Charge`, `EnhancedPunch`, `Fire Jutsu`, `Fire Pillar`, `Fireball`
- `Focus`, `Kick_Forward`, `Kick_Sweep`
- `Shuriken_Big`, `Shuriken_Small`, `Teleport_Omae`

**Other Actions** (`Resources/Actions/`):
- `AI Move.asset`, `Player Move.asset`, `Jump.asset`

---

### BattleDefinition
**Menu**: `ScriptableObjects/Battle`  
**Script**: `Scripts/Battle/Manager/BattleDefinition.cs`  
**Purpose**: Defines a single battle encounter (enemies, arena, rewards).  

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `enemyActors` | `List<ActorDefinition>` | Enemies to spawn |
| `arena` | `Arena` enum | `Arena.None` = in-scene; otherwise loads arena scene |
| `spawnGroup` | `SpawnGroupId` enum | Which spawn group to use in arena |
| `Level` | string | Legacy level suffix for in-scene battles |
| `RewardPool` | `RewardPool` | Actions for post-battle skill selection |
| `RewardItemsPools` | `List<RewardItemsPool>` | Item reward probability pools |

**Known Assets** (`Resources/Battles/Story/`):
- `1st Floor.asset`, `2nd Floor.asset`, `3th Floor.asset`, `4th Floor.asset`, `5th Floor.asset`

**Known Assets** (`Resources/Battles/QuickBattles/`):
- `1.asset` through `8.asset` (8 random battle definitions)

---

### BaseItem
**Menu**: `ScriptableObjects/Item/Material`  
**Script**: `Scripts/Battle/Items/BaseItem.cs`  
**Purpose**: Base data for collectible items.  
**Fields**: `ItemName`, `Description`, `Icon (Sprite)`, `stackable`

**Known Assets** (`Resources/Items/`):
| Asset | Type | Notes |
|-------|------|-------|
| `BlueOrb.asset` | BaseItem | [UNVERIFIED effect] |
| `HealingPot.asset` | Likely BaseConsumable | [UNVERIFIED] |
| `InnerFire.asset` | Likely BaseTrinket | [UNVERIFIED] |

---

### BaseConsumable (extends BaseItem)
**Menu**: `ScriptableObjects/Item/Consumable`  
**Script**: `Scripts/Battle/Items/BaseConsumable.cs`  
**Adds**: `List<IItemEffect> Effects` (SubclassSelector serialization)  
**Usage**: `ActorInventory.UseConsumable()` calls all effects then removes the item.

---

### BaseTrinket (extends BaseItem)
**Menu**: `ScriptableObjects/Items/Trinket`  
**Script**: `Scripts/Battle/Items/BaseTrinket.cs`  
**Adds**: `List<ItemTrigger> Triggers`, `List<IItemEffect> Effects`  
**Behavior**: Auto-subscribes effects to `ItemEventBus` on `Equip(owner)`.

---

### BaseStatus (Abstract)
**Script**: `Scripts/Battle/Status/BaseStatus.cs`  
**Purpose**: Defines a status effect type.  

**Known Concrete Types**:
| Type | Script | Notes |
|------|--------|-------|
| `PoisonStatus` | `PoisonStatus.cs` | Damage over time |
| `BlockStatus` | `BlockStatus.cs` | Active block state modifier |
| `DamageMultiplierStatus` | `DamageMultiplierStatus.cs` | Multiplies damage dealt/taken |
| `DoubleDamageStatus` | `DoubleDamageStatus.cs` | 2× damage modifier |
| `Stagger` | `Stagger.cs` | Forced stagger state |

**Known Assets** (`Resources/`):
- `DamageMultiplierStatus.asset`
- `PoisonStatus.asset`

---

### ActionDatabase
**Menu**: N/A (likely `ScriptableObjects/ActionDatabase` or custom)  
**Script**: `Scripts/Utility/ActionDatabase.cs`  
**Asset**: `Resources/Action Database.asset`  
**Purpose**: GUID → BaseAction lookup map. Used by SaveManager to restore player actions from saved GUIDs.

---

### RewardPool
**Script**: `Scripts/Battle/Actions/RewardPool.cs`  
**Purpose**: Contains a list of `BaseAction` assets that can be offered as post-battle rewards.  
**Asset Location**: `Resources/Battles/Story/RewardPool/`  

---

## Data Flow Summary

```
New Game:
  GameSession.StartNewRun()
    → Resources.Load<ActorDefinition>("Actors/MC")
    → ActorRuntime(definition) [clones actions from definition.baseActions]
    → RollStats() [3d6 for STR, AGI, MND, SPT]

Enemy Spawn:
  BattleManager.SpawnEnemies()
    → Instantiate(enemyPrefab)
    → actor.SetDefinition(ActorDefinition)
    → actor.Init() + actor.Spawn()
    → ActorRuntime.ResetToDefinition() [clones actions]

Post-Battle Rewards:
  BattleEndState.Enter()
    → Random item from RewardItemsPool (probability check)
    → SkillGenerator.DrawSkillsFromSelectionAndWaitForSelection(3 random from RewardPool)
    → Player picks → added to ActorRuntime.actions

Save:
  ActorSaveData.FromActor(actor)
    → Captures hpBars, stats, stamina, posture, buildup, items
    → Action layout from UIManager.GetCurrentLoadout() → List<ActionSlotSaveData> (GUID + container + slot)

Load:
  ActorSaveData.LoadInto(save, actor)
    → Restores all fields
    → ActionDatabase.Lookup(guid) to restore actions from saved GUIDs
```

---

## HpBar Structure

```csharp
[Serializable]
public class HpBar {
    float maxHp;
    float currentHp;
    bool alive;
}
```

Multiple HpBars per actor enable "boss phase" mechanics — when a bar depletes, it dies; the next bar becomes the active one.  
`ActorRuntime.isDead()` returns true when **all** bars are dead.
