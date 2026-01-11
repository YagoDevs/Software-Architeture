using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Min(1)] public int maxHP = 100;
    [SerializeField] private int currentHP;

    [Header("Feedback (optional)")]
    public bool logDamage = true;

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDied;

    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;

    void Awake()
    {
        // If not set in Inspector, start full HP.
        if (currentHP <= 0) currentHP = maxHP;
        currentHP = Mathf.Clamp(currentHP, 1, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (logDamage)
            Debug.Log($"Player took {amount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP == 0)
            Die();
    }

    public bool TryHeal(int amount)
    {
        if (IsDead) return false;
        if (amount <= 0) return false;
        if (currentHP >= maxHP) return false;

        int before = currentHP;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        int healed = currentHP - before;
        Debug.Log($"Player healed {healed}. HP: {currentHP}/{maxHP}");
        OnHealthChanged?.Invoke(currentHP, maxHP);
        return healed > 0;
    }

    void Die()
    {
        Debug.Log("Player died!");
        OnDied?.Invoke();
        if (GameManager.Instance != null)
            GameManager.Instance.HandlePlayerDied();
    }
}

