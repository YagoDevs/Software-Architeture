using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Config")]
    public EnemyConfig config;

    [Header("Animation / Death (optional)")]
    public Animator animator;
    public string dieTrigger = "Die";
    public bool delayDestroyOnDeath = true;
    public float destroyDelay = 2.0f; // time to let death animation play
    public bool disableCollidersOnDeath = true;
    public bool disableAIOnDeath = true;
    public bool debugDeathLogs = false;

    [Header("Runtime")]
    [SerializeField] int currentHP;

    [Header("Feedback (optional)")]
    public Renderer[] flashRenderers;
    public Color flashColor = new Color(1f, 0.25f, 0.25f);
    public float flashDuration = 0.08f;
    public bool logHits = true;
    public bool logDeath = true;

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDied;

    public int CurrentHP => currentHP;
    public int MaxHP => config != null ? config.maxHP : 1;
    public bool IsDead => currentHP <= 0;

    Color[] originalColors;
    float flashUntil;
    bool hasDied;
    bool warnedNoAnimator;

    void Awake()
    {
        if (config == null)
            Debug.LogWarning($"{name}: EnemyHealth has no config.");

        // If not set in Inspector, start full HP.
        if (currentHP <= 0) currentHP = MaxHP;
        currentHP = Mathf.Clamp(currentHP, 1, MaxHP);

        animator = ResolveAnimator(animator);

        CacheOriginalColors();
        OnHealthChanged?.Invoke(currentHP, MaxHP);
    }

    void Update()
    {
        if (flashRenderers == null || flashRenderers.Length == 0) return;
        if (Time.time <= flashUntil) return;
        RestoreColors();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        int before = currentHP;
        currentHP = Mathf.Max(0, currentHP - amount);
        OnHealthChanged?.Invoke(currentHP, MaxHP);
        Flash();

        if (logHits)
        {
            string n = (config != null && !string.IsNullOrEmpty(config.displayName)) ? config.displayName : name;
            Debug.Log($"{n} took {amount} damage. HP: {currentHP}/{MaxHP}");
        }

        if (CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 2f, $"-{amount}", new Color(1f, 0.85f, 0.25f));

        if (currentHP == 0)
            Die();
    }

    void Die()
    {
        if (hasDied) return;
        hasDied = true;

        if (logDeath)
        {
            string n = (config != null && !string.IsNullOrEmpty(config.displayName)) ? config.displayName : name;
            Debug.Log($"{n} died!");
        }

        OnDied?.Invoke();
        CombatEvents.RaiseEnemyKilled(config);

        TryDropItems();

        if (animator == null)
        {
            if (!warnedNoAnimator && debugDeathLogs)
            {
                warnedNoAnimator = true;
                Debug.LogWarning($"{name}: EnemyHealth could not find an Animator to play death animation. Assign 'animator' or ensure the model has an Animator.");
            }
        }
        else if (!string.IsNullOrEmpty(dieTrigger))
            animator.SetTrigger(dieTrigger);
        else if (debugDeathLogs)
            Debug.LogWarning($"{name}: EnemyHealth has no Animator or dieTrigger is empty, so no death animation will play.");

        if (disableAIOnDeath)
        {
            var ai = GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = false;

            var mage = GetComponent<MageTurretAI>();
            if (mage != null) mage.enabled = false;

            var boss = GetComponent<BossController>();
            if (boss != null) boss.enabled = false;
        }

        if (disableCollidersOnDeath)
        {
            var cols = GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
                cols[i].enabled = false;
        }

        if (delayDestroyOnDeath)
            Destroy(gameObject, Mathf.Max(0.05f, destroyDelay));
        else
            Destroy(gameObject);
    }

    void TryDropItems()
    {
        if (config == null) return;
        if (config.drops == null || config.drops.Count == 0) return;

        foreach (var entry in config.drops)
        {
            if (entry == null || entry.item == null) continue;
            if (UnityEngine.Random.value > entry.chance) continue;

            int qty = UnityEngine.Random.Range(entry.minQuantity, entry.maxQuantity + 1);
            SpawnDrop(entry.item, qty);
        }
    }

    void SpawnDrop(ItemData item, int quantity)
    {
        GameObject go = null;

        // Prefer item-specific prefab (e.g., HealthPotion_Pickup)
        if (item != null && item.pickupPrefab != null)
        {
            go = Instantiate(item.pickupPrefab);
            go.name = $"Drop_{item.itemName}";
            go.transform.position = transform.position + Vector3.up * 0.5f;
            if (item.pickupScaleMultiplier != 1f)
                go.transform.localScale = go.transform.localScale * item.pickupScaleMultiplier;

            var pickup = go.GetComponentInChildren<ItemPickup>();
            if (pickup == null) pickup = go.AddComponent<ItemPickup>();
            pickup.Configure(item, Mathf.Max(1, quantity));
            pickup.autoPickupOnTrigger = false; // keep Press-F interaction
            return;
        }

        // Fallback: create a visible sphere pickup
        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = $"Drop_{item.itemName}";
        go.transform.position = transform.position + Vector3.up * 0.8f;
        go.transform.localScale = Vector3.one * 1.2f;

        var col = go.GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        var fallbackPickup = go.AddComponent<ItemPickup>();
        fallbackPickup.Configure(item, Mathf.Max(1, quantity));
        fallbackPickup.autoPickupOnTrigger = false;
    }

    void CacheOriginalColors()
    {
        if (flashRenderers == null || flashRenderers.Length == 0) return;
        originalColors = new Color[flashRenderers.Length];
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r != null && r.material != null)
                originalColors[i] = r.material.color;
        }
    }

    void Flash()
    {
        if (flashRenderers == null || flashRenderers.Length == 0) return;
        flashUntil = Time.time + flashDuration;
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r != null && r.material != null)
                r.material.color = flashColor;
        }
    }

    void RestoreColors()
    {
        if (originalColors == null) return;
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r != null && r.material != null)
                r.material.color = originalColors[i];
        }
    }

    Animator ResolveAnimator(Animator preferred)
    {
        if (preferred != null) return preferred;

        var anims = GetComponentsInChildren<Animator>(true);
        if (anims == null || anims.Length == 0) return null;
        if (anims.Length == 1) return anims[0];

        Animator best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < anims.Length; i++)
        {
            var a = anims[i];
            if (a == null) continue;
            int score = 0;
            if (a.transform != transform) score += 10;
            if (a.GetComponentInChildren<SkinnedMeshRenderer>(true) != null) score += 5;
            if (a.runtimeAnimatorController != null) score += 1;
            if (score > bestScore)
            {
                bestScore = score;
                best = a;
            }
        }

        return best != null ? best : anims[0];
    }
}

