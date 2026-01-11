using System;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [Header("Leveling")]
    [Min(1)] public int level = 1;
    [Min(0)] public int currentXP = 0;
    [Min(1)] public int xpToNextLevel = 20;
    [Min(1)] public int xpGrowthPerLevel = 15;

    [Header("Power Growth (simple)")]
    public float damageMultiplier = 1f;
    public float damageMultiplierPerLevel = 0.12f;
    public int maxHPPerLevel = 10;

    public event Action<int, int, int> OnXPChanged; // level, currentXP, xpToNext
    public event Action<int> OnLevelUp; // new level

    PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        RaiseXPChanged();
    }

    void OnEnable()
    {
        CombatEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    void OnDisable()
    {
        CombatEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    void HandleEnemyKilled(EnemyConfig config)
    {
        if (config == null) return;
        GainXP(config.xpReward, config.displayName);
    }

    public void GainXP(int amount, string source = null)
    {
        if (amount <= 0) return;
        currentXP += amount;
        Debug.Log($"XP +{amount}" + (string.IsNullOrEmpty(source) ? "" : $" (from {source})") + $". XP: {currentXP}/{xpToNextLevel}");

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        RaiseXPChanged();
    }

    void LevelUp()
    {
        level += 1;
        xpToNextLevel += xpGrowthPerLevel;
        damageMultiplier += damageMultiplierPerLevel;

        if (playerHealth != null)
        {
            playerHealth.maxHP += maxHPPerLevel;
            // keep it simple: fully heal on level up
            playerHealth.TryHeal(999999);
        }

        Debug.Log($"LEVEL UP! Now level {level}");
        OnLevelUp?.Invoke(level);
    }

    void RaiseXPChanged()
    {
        OnXPChanged?.Invoke(level, currentXP, xpToNextLevel);
    }
}

