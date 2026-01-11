using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Config")]
    public EnemyConfig config;

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

    void Awake()
    {
        if (config == null)
            Debug.LogWarning($"{name}: EnemyHealth has no config.");

        // If not set in Inspector, start full HP.
        if (currentHP <= 0) currentHP = MaxHP;
        currentHP = Mathf.Clamp(currentHP, 1, MaxHP);

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
        if (logDeath)
        {
            string n = (config != null && !string.IsNullOrEmpty(config.displayName)) ? config.displayName : name;
            Debug.Log($"{n} died!");
        }

        OnDied?.Invoke();
        CombatEvents.RaiseEnemyKilled(config);

        TryDropItems();
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
        // Minimal, no prefab required: create a simple sphere with collider + ItemPickup.
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = $"Drop_{item.itemName}";
        go.transform.position = transform.position + Vector3.up * 0.5f;
        go.transform.localScale = Vector3.one * 0.5f;

        var col = go.GetComponent<Collider>();
        if (col != null) col.isTrigger = false; // PlayerInteraction uses OverlapSphere + F (no trigger needed)

        var pickup = go.AddComponent<ItemPickup>();
        pickup.itemData = item;
        pickup.quantity = Mathf.Max(1, quantity);
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
}

