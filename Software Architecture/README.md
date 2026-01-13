# Documentation / How to Test (Software Architecture Dungeon)

## Main scene
- Open: `Assets/OutdoorsScene.unity`

## Required objects in the Hierarchy (minimum)
- **Player** (Tag: `Player`)
  - `SimpleMovement`
  - `PlayerHealth`
  - `PlayerProgression`
  - `PlayerCombat`
  - `PlayerInteraction`
- **GameManager**
  - `GameManager` (`gameOverUI` assigned to the `GameOverUI` object)
- **InventoryManager**
  - `InventoryManager`
- **QuestManager**
  - `QuestManager` (with `startingQuests` filled)
- **Canvas**
  - `PlayerHUD` (script `PlayerHUD` wired to texts/sliders)
  - `QuestHUD` (script `QuestHUD` wired to TMP text)
  - `InventoryUIManager` (script `InventoryUI`)
  - `CombatTextManager` (script `CombatTextManager` + TMP prefab configured)
  - `GameOverUI` (script `GameOverUI` + panel and button configured)
- **Spawners**
  - `Spawner_Normal` (Goblin)
  - `Spawner_Mages` (SkeletonMage)
- **Boss**
  - `BossTrigger` (with `BossSpawnTrigger` and BoxCollider `Is Trigger = ON`)
  - `BossSpawnPoint` (Transform where the boss spawns)

## Controls
- **WASD**: move
- **Mouse**: rotate camera
- **E**: open/close inventory
- **F**: pick up nearby item (shows “Press F - <Item>”)
- **Left click**: Attack 1 (melee)
- **Right click**: Attack 2 (e.g., fireball / alt attack)
- **R**: Restart (when Game Over is visible)
- **ESC**: unlock cursor (if needed)

## Combat system (what to look for)
- Damage/heal appears as floating combat text (CombatText)
- Enemy HP bar/value appears above enemies (EnemyHealthBar)
- Enemies damage the Player and when HP reaches 0 → **Game Over**

## Inventory & Potion (mandatory requirement)
- Enemies can drop items.
- Pick the item (F), open inventory (E), select and click **Use**.
- The **HP Potion** heals the player if HP is not full.

## Quests (mandatory requirement)
The system provides at least:
- **Kill quest**: “kill X enemies of a certain type”
- **Fetch quest**: “fetch a specific item”

### Kill Skeletons (example)
- `QuestDefinition` type `KillEnemy`
  - `targetEnemyId` must match the Skeleton Mage `EnemyConfig.enemyId`.
- The amount can be overridden via `QuestManager.overrides` (e.g., 3).

### Fetch Item (example: potion)
- `QuestDefinition` type `FetchItem`
  - `targetItem` must point to the correct `ItemData`.
- When the item is picked up (added to inventory), the quest updates automatically.

## Spawns
### Spawner_Normal (Goblins)
- `enemyTypeA = Goblin`
- `enemyTypeB = None`
- `maxAlive` can be > 1

### Spawner_Mages (Skeleton Mage)
- `enemyTypeA = None`
- `enemyTypeB = SkeletonMage`
- `maxAlive = 1`
- `spawnPoints`: 2 points (e.g., `MagePoint1`, `MagePoint2`)
- `useSequentialSpawnPoints = true` (alternates 1→2→1→2)
- `stopSpawningWhenQuestCompleted = true`
- `questIdToStopSpawning = kill_skeletons` (or your quest `questId`)

## Boss via Trigger (no chasing)
- The boss spawns when crossing `BossTrigger`.
- The boss prefab should contain `BossController` (attacks configured via `BossAttackDefinition`).
- `BossSpawnTrigger` requires:
  - `bossPrefab` (boss prefab)
  - `spawnPoint` (BossSpawnPoint)
  - `spawnOnce = true`

## Debug / Quick Troubleshooting
- **Boss does not spawn**:
  - Ensure `BossTrigger` has a **Collider** and **Is Trigger = ON**.
  - Set `BossSpawnTrigger.debugLogs = true` to see logs in Console.
  - `BossSpawnTrigger.useBoundsFallback = true` works even if `OnTriggerEnter` does not fire.
- **Fetch quest does not update**:
  - Confirm `QuestManager.startingQuests` contains the quest.
  - Confirm `targetItem` points to the same `ItemData` (or at least same `itemName`).
- **Enemy does not damage player**:
  - Check `EnemyConfig.contactDamage > 0` and `attackRange/attackCooldown`.

