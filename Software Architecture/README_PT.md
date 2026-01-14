# Explicação do Código (para apresentação)

Este arquivo explica **o que cada classe faz** e **como elas se interligam**.

## Visão geral (fluxo do jogo)
- O **Player** se move (`SimpleMovement`), luta (`PlayerCombat`), tem vida (`PlayerHealth`) e progresso (`PlayerProgression`).
- **Inimigos** têm stats configuráveis (`EnemyConfig`), vida (`EnemyHealth`) e comportamento (`EnemyAI` / `MageTurretAI`).
- Quando um inimigo morre, dispara um evento global (`CombatEvents`) que atualiza **XP**, **quests** e **spawners** sem acoplamento.
- O **Inventário** é gerenciado por `InventoryManager` e exibido por `InventoryUI`.
- O **HUD** (vida/xp/quests) é atualizado por eventos (`PlayerHUD`, `QuestHUD`).
- **Game Over** é controlado por `GameManager` + `GameOverUI` (com botão Restart).
- O **Boss** nasce via trigger (`BossSpawnTrigger`) e ataca via `BossController` + `BossAttackDefinition` (ataques configuráveis).

---

## Managers / Core

### `GameManager`
- **Responsabilidade**: controlar estado global (principalmente **Game Over** e **Restart**).
- **Liga com**:
  - `PlayerHealth` chama `GameManager.HandlePlayerDied()` quando HP chega a 0.
  - `GameOverUI` chama `GameManager.Restart()` ao clicar no botão.

### `InventoryManager`
- **Responsabilidade**: lista de itens (`InventoryItem`), capacidade, adicionar/remover/usar.
- **Liga com**:
  - `PlayerInteraction` e `ItemPickup` chamam `InventoryManager.AddItem(...)`.
  - `InventoryUI` lê e mostra `InventoryManager.items`.
  - `QuestManager` escuta `InventoryManager.OnInventoryChanged` para quest de fetch.
  - Som de pickup (`pickupItemSfx`) ao adicionar item.

### `EnemySpawner`
- **Responsabilidade**: spawnar inimigos por tempo, limitar vivos (`maxAlive`), usar `spawnPoints` (inclusive alternando 1→2→1→2).
- **Liga com**:
  - Escuta mortes via `CombatEvents` (reduz `aliveCount`).
  - Pode parar de spawnar quando uma quest completa (`QuestManager.IsQuestCompleted`).

### `BossSpawnTrigger`
- **Responsabilidade**: spawna boss ao atravessar um trigger (ou fallback por bounds).
- **Liga com**:
  - Instancia o boss em `spawnPoint` e ativa `BossController`.

### `CombatEvents`
- **Responsabilidade**: “event bus” simples.
- **Liga com**:
  - `EnemyHealth` chama `CombatEvents.RaiseEnemyKilled(config)` ao morrer.
  - `PlayerProgression` escuta para dar XP.
  - `QuestManager` escuta para quest “kill X”.
  - `EnemySpawner` escuta para ajustar vivos/kills.

---

## Player

### `SimpleMovement`
- **Responsabilidade**: WASD + câmera com mouse (cursor lock/unlock) e envia **Speed** para o Animator do player.
- **Liga com**:
  - `InventoryUI` desbloqueia cursor quando inventário abre.

### `PlayerHealth`
- **Responsabilidade**: HP atual/máximo, dano, cura, morte.
- **Liga com**:
  - `PlayerHUD` escuta `OnHealthChanged` para atualizar UI.
  - `InventoryManager.UseItem()` chama `TryHeal()`.
  - `EnemyAI` e projéteis chamam `TakeDamage()`.
  - `GameManager` ao morrer.
  - `CombatTextManager` mostra texto de dano/cura.
  - Toca áudio `hitSfx`/`healSfx`.

### `PlayerProgression`
- **Responsabilidade**: XP/level up, buffs simples (maxHP, dano).
- **Liga com**:
  - Escuta `CombatEvents.OnEnemyKilled` para ganhar XP.
  - `PlayerHUD` escuta `OnXPChanged`/`OnLevelUp`.
  - Toca áudio `levelUpSfx`.

### `PlayerCombat`
- **Responsabilidade**: ataque 1 (clique esquerdo melee) e ataque 2 (clique direito melee OU fireball) com windup/ cooldown.
- **Liga com**:
  - Aplica dano em `EnemyHealth`.
  - Dispara triggers no Animator (`Attack1`/`Attack2`).
  - Spawna `FireballProjectile` (quando habilitado).
  - Toca SFX (attack/fireball).

### `PlayerInteraction`
- **Responsabilidade**: detectar `ItemPickup` perto e mostrar “Press F …”, coletar com F.
- **Liga com**:
  - Chama `InventoryManager.AddItem(...)` e destrói o pickup.

---

## Inimigos (regulares)

### `EnemyConfig` (ScriptableObject)
- **Responsabilidade**: dados do inimigo sem hardcode (HP, dano, range, cooldown, XP, drops).
- **Liga com**:
  - `EnemyHealth` usa `config.maxHP`.
  - `EnemyAI` usa range/cooldown/dano/velocidade.
  - `PlayerProgression` usa `config.xpReward`.
  - `EnemyHealth` usa `config.drops` para dropar itens.

### `EnemyHealth`
- **Responsabilidade**: vida do inimigo, dano, morte, drops e delay para destruir depois da animação.
- **Liga com**:
  - Dispara evento `CombatEvents` ao morrer.
  - Spawna drops via `ItemData.pickupPrefab`.
  - Atualiza `EnemyHealthBar` via `OnHealthChanged`.
  - Dispara trigger `Die` no Animator e espera `destroyDelay` antes de sumir.
  - Mostra combat text ao tomar dano.

### `EnemyAI`
- **Responsabilidade**: inimigo melee que persegue o player (planar distance) e ataca quando em range.
- **Liga com**:
  - Chama `PlayerHealth.TakeDamage()`.
  - Dispara trigger `Attack` no Animator e `Speed` para walk.

### `MageTurretAI`
- **Responsabilidade**: inimigo parado (mago) que gira para olhar o player e atira projétil.
- **Liga com**:
  - Spawna `FireballProjectile` configurado para **acertar player**.
  - Dispara trigger `Cast` no Animator e toca `castSfx`.

---

## Boss (configurável)

### `BossAttackDefinition` (ScriptableObject)
- **Responsabilidade**: define um ataque do boss (tipo, dano, range, cooldown, windup, trigger de animação).

### `BossController`
- **Responsabilidade**: executa ataques do boss com base na lista de `BossAttackDefinition` (sem hardcode).
- **Liga com**:
  - Vira para o player e escolhe o primeiro ataque disponível em range e sem cooldown.
  - Aplica dano no `PlayerHealth`.

---

## Itens / Pickups

### `ItemData` (ScriptableObject)
- **Responsabilidade**: dados do item (nome, ícone, descrição, stack, consumível, healthRestore, prefab de pickup).

### `ItemPickup`
- **Responsabilidade**: objeto no mundo que representa um item pegável (bobbing/rotação).
- **Liga com**:
  - `PlayerInteraction` coleta com F.

### `FireballProjectile`
- **Responsabilidade**: projétil que se move e aplica dano ao colidir (pode acertar inimigos OU player).

---

## UI

### `InventoryUI`
- **Responsabilidade**: abre/fecha inventário, cria slots, seleciona item, botões Use/Drop.
- **Liga com**:
  - Chama `InventoryManager.UseItem()` e `RemoveItem()`.
  - Pausa o jogo e libera cursor quando inventário abre.

### `InventorySlot`
- **Responsabilidade**: representa um slot do inventário; ao clicar chama `InventoryUI.SelectItem(...)`.

### `PlayerHUD`
- **Responsabilidade**: mostra HP e XP/Level (Text/Slider).
- **Liga com**: `PlayerHealth` e `PlayerProgression` (via eventos).

### `QuestHUD`
- **Responsabilidade**: lista quests e progresso; pinta verde a quest completa (ou tudo verde se todas completas).
- **Liga com**: `QuestManager.OnQuestsChanged`.

### `EnemyHealthBar`
- **Responsabilidade**: HP acima do inimigo (world-space), auto-offset.
- **Liga com**: `EnemyHealth.OnHealthChanged`.

### `CombatTextManager` / `CombatText`
- **Responsabilidade**: spawnar textos flutuantes (dano, cura, ATTACK/CAST).

### `GameOverUI`
- **Responsabilidade**: painel “GAME OVER” + botão Restart (chama `GameManager.Restart()`).

---

## Quests

### `QuestDefinition` (ScriptableObject)
- **Responsabilidade**: define quest do tipo `KillEnemy` ou `FetchItem`.

### `QuestManager`
- **Responsabilidade**: manter quests ativas, atualizar progresso, marcar completa, tocar som.
- **Liga com**:
  - Escuta `CombatEvents.OnEnemyKilled` (kill quest).
  - Escuta `InventoryManager.OnInventoryChanged` (fetch quest).
  - Notifica UI via `OnQuestsChanged`.

---

## “Códigos feitos com ajuda de AI” (apenas 2, os mais complexos)
Se você precisar citar 2 scripts como os mais “complexos” que foram feitos com ajuda de AI, estes são bons candidatos:
1) `Assets/Scripts/Boss/BossController.cs` (seleção/execução de ataques por ScriptableObject, cooldown, windup)
2) `Assets/Scripts/Quests/QuestManager.cs` (gerenciamento de quests, eventos, overrides, fetch/kill, integração com inventory e UI)

