# AI System

> **Last verified:** 2026-08-03

## Purpose

Controls the decision-making of non-player actors using a priority-ordered Behavior Tree.

---

## Responsibilities

- Select and execute the highest-priority valid action each frame.
- Govern approach, attack, block, dodge, and flanking behaviors.
- Rate-limit attacks via `GlobalAttackTokenCondition` to prevent all enemies from attacking simultaneously.
- Scale behavior complexity via `AiRuleset` variants assigned per enemy type.

---

## Main Scripts

| Script | Role |
|--------|------|
| `AIBT.cs` | Top-level BT controller; holds behavior lists, ticks every frame |
| `BTNode.cs` | Base behavior tree node |
| `Behaviors/ApproachBehavior.cs` | Move toward player |
| `Behaviors/FlankApproachBehavior.cs` | Move toward player from a flank angle (also defines `CircleApproachBehavior`) |
| `Behaviors/SmartApproachBehavior.cs` | Crowd-aware surround: stable slot angles + ring positioning (used by `SmartApproachBehaviorTree`) |
| `Behaviors/GroupFlankBehavior.cs` | Coordinate flank with other enemies |
| `Behaviors/MoveToOrbitBehavior.cs` | Circle at a set radius |
| `Behaviors/MoveBehavior.cs` | Generic directional move |
| `Behaviors/DashBehavior.cs` | Quick dash move |
| `Behaviors/DodgeBehavior.cs` | Use dodge action |
| `Behaviors/AttackWithValidSkill.cs` | Select and use a valid melee action |
| `Behaviors/AttackProjectile.cs` | Select and use a valid ranged action |
| `Behaviors/BlockBehavior.cs` | Enter block stance |
| `Behaviors/BlockCancelBehavior.cs` | Cancel active block |
| `Behaviors/ReactionBehavior.cs` | React to incoming attack |
| `Conditions/DistanceConditions.cs` | Distance threshold checks |
| `Conditions/GlobalAttackTokenCondition.cs` | Global rate limiter for AI attacks |
| `Conditions/HesitateCondition.cs` | Probability-gated timing pause |
| `Conditions/InsideEnemyHitbox.cs` | Detects if inside a hitbox |
| `Conditions/StaminaConditions.cs` | Stamina threshold gates |

---

## Data Sources

- `ActorDefinition.AIRuleset` → which behavior list to use
- `ActorRuntime.actions` → pool of actions `AttackWithValidSkill` draws from
- `Actor.target.DistanceToClosestEnemy` → distance metric for conditions
- `Actor.state` → AIBT.Update() only runs when Idle or Moving (not Stagger/Death/Acting)

---

## Events

No events emitted. AI reads actor state and calls `actor.UseAction()` directly.

---

## Rulesets

| Ruleset | Behavior | Aggression |
|---------|----------|-----------|
| `DEFAULT` | `SandboxGuy` — move away from level edge | Passive |
| `OldMan` | `OldManBehavior` — block → attack → circle | Defensive |
| `Maniac` | `EngragedManiac` — dodge hits → attack aggressively → chase | High |
| `ShurkienThrower` | `ShurkienThrowerBehavior` — stay 3–5u away, throw projectiles | Ranged |
| `TacticalFlanker` | `TacticalFlankerBehavior` — block → hesitate+token gate → attack → orbit | Tactical |
| `SmartApproach` | `SmartApproachBehaviorTree` — surround with stable slot angles + ring hold, falls back to `ApproachBehavior` | Smart |
| `Hungry` | Maps to `OldManBehavior` | Moderate |
| `Ninja` | Empty behavior list — does nothing | Unknown |

---

## External Dependencies

- `Actor.UseAction(action)` — how AI triggers actions
- `Actor.target` (TargetingSystem) — enemy positions, distances
- `Actor.movement` (MovementSystem) — AI movement execution
- `BattleManager` — indirectly through actor references

---

## Extension Points

- Add a new ruleset: add case to `AIBT.Reset()` switch, create a `static readonly List<BTNode>`.
- Add a new condition: implement `BTNode` as a condition node.
- Add a new behavior: implement `BTNode` with action logic, add to a ruleset list.

---

## Risks

- **AI tick throttled** — `aiTickCooldown = 0.05f` (throttle re-enabled 2026-08-03); dense enemy groups still run LINQ-heavy evaluations per tick.
- **Ninja ruleset is empty** — assigning `AiRuleset.Ninja` to an actor produces a passive, non-functioning AI.
- **GlobalAttackTokenCondition** limits simultaneous attacks globally, but the token budget and reset logic need verification for balance.
- **AIBT.Reset()** is called on `Actor.OnReset` — if OnReset fires mid-battle, AI state is wiped.

---

## Common Modification Locations

- Assign AI behavior to an enemy: edit the `ActorDefinition` asset, set `AIRuleset`.
- Tune aggression: modify probability in `HesitateCondition`, distance thresholds in behavior constructors.
- Add a new enemy archetype: create `ActorDefinition` with a new or existing `AiRuleset`, add starter actions.
