# Hit Windows System - Implementation Plan

## Overview
Refactor GenericSkill to use frame-based hit windows instead of animation events. Each HitWindow defines:
- Frame range (startFrame, endFrame, inclusive)
- Per-enemy hit limit (maxHitsPerEnemy)
- List of effects to run while window is active

## Architecture

### Frame Tracking
- Tracked in `GenericSkill.OnUpdate(dt)` which is called every frame by ActingState
- `actionElapsedTime` accumulates elapsed seconds since action began
- `currentFrame = Mathf.RoundToInt(actionElapsedTime * 60f)` converts to frame number (60 FPS baseline)
- Frame numbers are independent of animator.speed variations

### Hit Window Manager
- Instance created per action in `PerformSpecific()`
- Tracks per-enemy/per-window hit counts via Dictionary<(Actor enemy, int windowIndex), int>
- Prevents same enemy being hit more than maxHitsPerEnemy times per window
- Counts reset when window closes

### Effect Execution
- Window effects run every frame the window is active
- Each call to `Eval()` happens every frame (frames startFrame through endFrame)
- DamageEffect queries HitWindowManager.CanHit() to enforce per-enemy limits
- If effect implements IEndableEffect, add to runtimeEndEffects for cleanup

## Files to Create

### 1. HitWindow.cs
**Location:** `Assets/Scripts/Battle/Actions/HitWindows/HitWindow.cs`

**Struct definition:**
```csharp
[System.Serializable]
public struct HitWindow
{
    public string displayName;                              // "Jab", "Dash Active", etc.
    public int startFrame;                                 // Inclusive start frame
    public int endFrame;                                   // Inclusive end frame
    public int? maxHitsPerEnemy;                           // null = unlimited; 1 = once; etc.
    
    [SerializeReference, SubclassSelector]
    public List<IEffect> windowEffects;                   // Effects to run while active
}
```

**Responsibilities:**
- Pure data structure for serialization
- Validates frame ranges in custom PropertyDrawer (optional enhancement)

---

### 2. HitWindowManager.cs
**Location:** `Assets/Scripts/Battle/Actions/HitWindows/HitWindowManager.cs`

**Class definition:**
```csharp
public class HitWindowManager
{
    private List<HitWindow> hitWindows;
    private Dictionary<(Actor enemy, int windowIndex), int> hitCounts = new();
    
    public HitWindowManager(List<HitWindow> windows)
    {
        hitWindows = windows;
    }
    
    public bool CanHit(Actor enemy, int windowIndex)
    {
        // Returns false if enemy already hit maxHitsPerEnemy times in this window
        // Returns true if:
        //   - windowIndex is invalid
        //   - window.maxHitsPerEnemy is null (unlimited)
        //   - enemy hasn't reached hit limit yet
    }
    
    public void RecordHit(Actor enemy, int windowIndex)
    {
        // Increment hit count for this enemy in this window
    }
}
```

**Responsibilities:**
- Track per-enemy hit counts per window
- Answer "can this enemy be hit again in this window?"
- Record hits when they occur

---

## Files to Modify

### 3. GenericSkill.cs
**Location:** `Assets/Scripts/Battle/Actions/Actions/GenericSkill.cs`

**Changes:**

1. **Add new serialized fields:**
   ```csharp
   [SerializeReference, SubclassSelector]
   public List<IEffect> OnStartEffects = new();      // (unchanged)
   
   [SerializeReference, SubclassSelector]
   public List<HitWindow> hitWindows = new();        // NEW
   
   [SerializeReference, SubclassSelector]
   public List<IEffect> OnEndEffects = new();        // (unchanged)
   
   [SerializeReference, SubclassSelector]
   [Obsolete("Use hitWindows instead. Kept for backward compatibility during migration.")]
   public List<IEffect> OnHitEffects = new();        // DEPRECATED (mark as obsolete)
   ```

2. **Add private tracking fields:**
   ```csharp
   private float actionElapsedTime = 0f;
   private HitWindowManager hitWindowManager;
   private int currentActiveWindowIndex = -1;        // Which window is active now (-1 if none)
   ```

3. **Add public property to expose CurrentActiveWindowIndex:**
   ```csharp
   public int CurrentActiveWindowIndex => currentActiveWindowIndex;
   ```

4. **Add public property to expose HitWindowManager:**
   ```csharp
   public HitWindowManager HitWindowMgr => hitWindowManager;
   ```

5. **Modify PerformSpecific():**
   - Initialize `actionElapsedTime = 0f`
   - Initialize `hitWindowManager = new HitWindowManager(hitWindows)`
   - Keep OnStartEffects as-is:
     ```csharp
     foreach (var effect in OnStartEffects)
     {
         if (effect is IEndableEffect endable) runtimeEndEffects.Add(endable);
         effect.Eval(_caster, this);
     }
     ```
   - Transition to ActingState unchanged

6. **Replace OnHit() override:**
   - For backward compatibility during migration phase, keep it but make it call windowEffects from deprecated OnHitEffects list
   - Add warning comment: "// TODO: Remove this when all actions migrated to HitWindows"
   - Alternative: Delete entirely if starting fresh

7. **Rewrite OnUpdate(float dt):**
   ```csharp
   public override void OnUpdate(float dt)
   {
       actionElapsedTime += dt;
       int currentFrame = Mathf.RoundToInt(actionElapsedTime * 60f);
       
       // Determine which windows are active this frame
       for (int i = 0; i < hitWindows.Count; i++)
       {
           var window = hitWindows[i];
           bool isActive = currentFrame >= window.startFrame && 
                          currentFrame <= window.endFrame;
           
           if (isActive)
           {
               currentActiveWindowIndex = i;
               
               // Run all window effects
               foreach (var effect in window.windowEffects)
               {
                   if (effect is IEndableEffect endable)
                       runtimeEndEffects.Add(endable);
                   effect.Eval(_caster, this);
               }
           }
       }
       
       currentActiveWindowIndex = -1;
       
       // Run other updateable effects (OnStartEffects that are IUpdateableEffect)
       foreach (var effect in runtimeUpdateEffects)
           effect.Update(_caster, this, dt);
   }
   ```

8. **Keep Cleanup() unchanged:**
   - OnEndEffects already added during PerformSpecific
   - Existing cleanup logic remains the same

---

### 4. DamageEffect.cs
**Location:** `Assets/Scripts/Battle/Actions/Actions/Effects/DamageEffect.cs`

**Changes:**

1. **Add field to track current window index (transient, set per Eval call):**
   ```csharp
   private int currentWindowIndex = -1;
   ```

2. **Rewrite Eval(Actor actor, BaseAction action):**
   ```csharp
   public virtual void Eval(Actor.Actor actor, BaseAction action)
   {
       var gs = action as GenericSkill;
       if (gs == null) return;
       
       // Get current active window index
       currentWindowIndex = gs.CurrentActiveWindowIndex;
       
       // Query hitbox from OnStartEffects
       IHitbox hitboxEffect = gs.OnStartEffects
           .OfType<IHitbox>()
           .FirstOrDefault();
       
       if (hitboxEffect == null) return;
       
       var enemies = hitboxEffect.CheckEnemiesInsideHitbox(actor);
       
       foreach (var enemy in enemies)
       {
           // Check if this enemy can be hit in current window
           if (!gs.HitWindowMgr.CanHit(enemy, currentWindowIndex))
               continue;
           
           ApplyDamageEffects(actor, enemy, action);
           gs.HitWindowMgr.RecordHit(enemy, currentWindowIndex);
       }
   }
   ```

3. **Keep ApplyDamageEffects() unchanged:**
   - Existing logic for blocking, buildup gain, damage application stays the same

---

## Implementation Order

### Phase 1: Core Infrastructure (Lowest Risk)
1. Create HitWindow.cs struct
2. Create HitWindowManager.cs class
3. Add fields and properties to GenericSkill
4. Implement frame tracking in GenericSkill.OnUpdate()

### Phase 2: Effect Integration
5. Modify GenericSkill.OnUpdate() to call window effects
6. Modify DamageEffect.Eval() to use HitWindowManager
7. Ensure OnStartEffects and OnEndEffects still work

### Phase 3: Backward Compatibility
8. Mark OnHitEffects as [Obsolete]
9. Keep OnHit() override (or make it call deprecated OnHitEffects list as fallback)
10. Test existing actions still work

### Phase 4: Validation
11. Create test action with multiple HitWindows (1-2 punch)
12. Create test action with long HitWindow (dash attack)
3. Verify frame counting is correct
14. Verify hit limits per enemy per window

---

## Testing Checklist

### Basic Functionality
- [ ] Simple attack with 1 HitWindow: hits enemies correctly
- [ ] Multi-hit attack with 2 HitWindows: enemy hit twice (once per window)
- [ ] Dash attack with long HitWindow (frames 8-14): enemy hit up to maxHitsPerEnemy times
- [ ] maxHitsPerEnemy=1: same enemy never hit twice in same window
- [ ] maxHitsPerEnemy=2: same enemy can be hit exactly twice in same window

### Frame Accuracy
- [ ] HitWindow frame ranges match visual animation timing
- [ ] Frame calculation (currentFrame = actionElapsedTime * 60) is accurate
- [ ] animator.speed changes don't break frame counting

### Effect Integration
- [ ] OnStartEffects (ShowHitbox) still run once at action start
- [ ] OnEndEffects (ClearHitbox) still run once at action end
- [ ] Window effects run every frame window is active
- [ ] IEndableEffect in window effects are properly tracked for cleanup

### Backward Compatibility
- [ ] Existing GenericSkills with empty hitWindows list still work
- [ ] Deprecated OnHitEffects field still works (if fallback implemented)
- [ ] No serialization errors on existing action assets

### Edge Cases
- [ ] Overlapping HitWindows: both run and effects execute
- [ ] HitWindow with startFrame=endFrame: triggers for exactly 1 frame
- [ ] maxHitsPerEnemy=null: unlimited hits allowed
- [ ] Empty windowEffects list: no errors, just no effects

---

## Migration Notes

### For Migrating Existing Actions
Actions currently using OnHitEffects should be rebuilt using HitWindows:

**Before (OnHitEffects):**
```
OnStartEffects: [ShowHitboxOnPlayer]
OnHitEffects: [DamageEffect, FlashColor]
OnEndEffects: [ClearHitbox]
Animation: OnHit event at frame 12
```

**After (HitWindows):**
```
OnStartEffects: [ShowHitboxOnPlayer]
HitWindows:
  [0] "MainHit"
      startFrame: 12
      endFrame: 12
      maxHitsPerEnemy: 1
      windowEffects: [DamageEffect, FlashColor]
OnEndEffects: [ClearHitbox]
Animation: No OnHit event needed
```

### Remove Animation Events
- Delete "OnHit" animation events from attack animation clips
- No other animation events should be needed (OnWindup/OnRecovery remain if used)

---

## Known Limitations (By Design)

1. **No frame scaling by animator.speed:** HitWindows are defined in real-time frames (60 FPS), not animator frames. If you speed up animations with windupTimeMult, hit windows stay the same duration (in real time).
   - **Rationale:** Simpler to understand and configure; designers think in real-time seconds.
   - **Future enhancement:** Could add optional scaling factor if needed.

2. **No automatic hit recovery delay:** All windows evaluated every frame. If you want "only hit every other frame", define maxHitsPerEnemy=1 with a single-frame window (startFrame=endFrame).

3. **No action-wide hit immunity:** Hit limits are per-window, per-enemy. If you want "hit this specific enemy only once per entire action", define maxHitsPerEnemy=1 with a window spanning the entire action duration.

---

## Implementation Tips

- Use `Mathf.RoundToInt()` for frame calculation to avoid floating-point drift
- Test with different dt values to ensure frame counting is stable
- Consider adding debug visualization: draw active window ranges in Scene view during editing
- Store HitWindow template assets in `Assets/ScriptableObjects/ActionTemplates/` for reuse

---

## Questions for Developer During Implementation

1. Should `CurrentActiveWindowIndex` be public or internal? (Currently: public)
2. Should we validate HitWindow frame ranges (startFrame <= endFrame) in editor or runtime?
3. Do you want to keep the deprecated OnHitEffects field indefinitely, or remove it after a migration period?
4. Should overlapping HitWindows be prevented/warned about, or allowed?
