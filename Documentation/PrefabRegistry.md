# Gates of Gehera — Prefab Registry

## Assets/Prefabs

### Actor.prefab
- **Purpose**: Generic/enemy actor body. Template for non-player characters spawned via `BattleManager.SpawnEnemies()`.
- **Key Components**: `Actor`, `ActorStateMachine`, `StatusManager`, `Rigidbody`, `CapsuleCollider`, `Animator`, `NavMeshAgent`.
- **Dependencies**: Requires a `BattleManager` in the scene; `ActorDefinition` set via `Actor.SetDefinition()` at spawn.
- **Spawning**: Instantiated by `BattleManager.SpawnEnemies()` using `BattleManager.enemyPrefab` reference.

### PlayerBattler.prefab
- **Purpose**: Player character body. Pre-placed in `CaveScene`; instantiated from `ArenaBootstrapper.playerPrefab` in arena scenes.
- **Key Components**: Same as `Actor.prefab` plus `VelocityIndicator`, `ParticleController`, `SpriteShapeRenderer`.
- **Dependencies**: Bound to `GameSession.PlayerRuntime` via `Actor.Bind()` when loaded into arena scenes.
- **Spawning**: Pre-placed in CaveScene; spawned by `ArenaBootstrapper.SpawnAndBindPlayer()` in arena scenes.
- **Notes**: The player body is re-created each scene; runtime state lives on `GameSession.PlayerRuntime`.

### ReworkedActor.prefab
- **Purpose**: [UNVERIFIED] — Possibly an in-progress reworked version of the Actor prefab.
- **Components**: [UNVERIFIED — not inspected]

### HpBar.prefab
- **Purpose**: World-space HP bar displayed above an actor.
- **Key Components**: `HpBar`, UI Image elements.
- **Dependencies**: Referenced by `ActorUIController` or `HpBarHandler`.
- **Spawning**: [UNVERIFIED — instantiation site not confirmed in scripts reviewed]

### Projectile.prefab
- **Purpose**: Projectile fired by `ProjectileAttack` action. Travels toward target and triggers `ProjectileHandler` on collision.
- **Key Components**: `ProjectileHandler`, `Rigidbody`, `Collider`.
- **Dependencies**: Referenced by `ProjectileAttack.projectilePrefab`; spawned in `ProjectileAttack.OnHit()`.

### Fireball.prefab
- **Purpose**: Specific projectile variant for fire-based ranged attacks.
- **Key Components**: Likely `ProjectileHandler` + particle system.
- **Dependencies**: Assigned to fire-themed `ProjectileAttack` assets.

### Fire.prefab / Torch.prefab / Wall_Torch Variant.prefab
- **Purpose**: Environmental decoration (fire/torch visuals).
- **Dependencies**: None at runtime.

### HpPopup.prefab / PosturePopup.prefab
- **Purpose**: Floating damage number popups shown when damage is dealt.
- **Location**: `Assets/Resources/` (loaded at runtime via `Resources.Load`).
- **Key Components**: `DamagePopup`.

### ActionButton.prefab
- **Purpose**: A single draggable action button in the UI action containers.
- **Key Components**: `ActionButtonBattle` or `ActionButtonInventory`, `Button`, `Image`, drag-and-drop handlers.
- **Spawning**: Instantiated by `UIManager.InitializePlayerActionButtonPrefabs()`.
- **Dependencies**: Requires a parent `DropSlot` container.

### ActionButtonMenu.prefab
- **Purpose**: Action button variant used in inventory/menu context.
- **Key Components**: `ActionButtonInventory`.

### InventorySlot.prefab
- **Purpose**: A slot in the skill/inventory view panel.
- **Key Components**: [UNVERIFIED]

### SkillCard.prefab
- **Purpose**: Post-battle skill selection card shown by `SkillGenerator.DrawSkillsFromSelectionAndWaitForSelection()`.
- **Key Components**: `ActionCardHandler`, `Button`.
- **Spawning**: Instantiated inside `Choose Skills UI` canvas by `SkillGenerator`.

### GameObject.prefab
- **Purpose**: [UNVERIFIED — generic placeholder name]

---

## Assets/Resources/HpPopup.prefab / PosturePopup.prefab

Loaded via `Resources.Load` by `DamagePopup` and related display scripts.

---

## Assets/Prefabs/Effects/

[UNVERIFIED — directory not listed; presumed to contain particle system prefabs used by `PlayParticleEffect` effect]

---

## Runtime Spawning Summary

| Spawner | Prefab | When |
|---------|--------|------|
| `BattleManager.SpawnEnemies()` | `BattleManager.enemyPrefab` (Actor.prefab) | On `BattleManager.Enter()` |
| `ArenaBootstrapper.SpawnAndBindPlayer()` | `ArenaBootstrapper.playerPrefab` (PlayerBattler.prefab) | Arena scene Start() |
| `GameFlowManager.EnsurePlayerBody()` | `GameFlowManager.playerPrefab` | CaveScene Start() if no pre-placed actor |
| `UIManager.InitializePlayerActionButtonPrefabs()` | `UIManager.buttonPrefab` (ActionButton.prefab) | After scene load or battle end |
| `SkillGenerator.DrawSkillsFromSelectionAndWaitForSelection()` | `SkillGenerator.CardPrefab` (SkillCard.prefab) | After battle victory |
| `ProjectileAttack.OnHit()` | `ProjectileAttack.projectilePrefab` | During combat action |
| `PlayParticleEffect.Eval()` | `PlayParticleEffect.particlePrefab` | During action effects |
