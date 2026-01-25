/*
This script is used to manage XP and leveling. It increases stats on level up and notifies the UI via events.
*/

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

    [Header("Audio (optional)")]
    public AudioSource audioSource; // if null, uses PlayClipAtPoint
    public AudioClip levelUpSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    public event Action<int, int, int> OnXPChanged; // level, currentXP, xpToNext
    public event Action<int> OnLevelUp; // new level

    PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
        RaiseXPChanged();
    }

    // this avoid event handlers to be called after the object is destroyed
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
        PlaySfx(levelUpSfx, transform.position);
        OnLevelUp?.Invoke(level);
    }

    void PlaySfx(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;
        if (sfxVolume <= 0f) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, pos, sfxVolume);
    }

    void RaiseXPChanged()
    {
        OnXPChanged?.Invoke(level, currentXP, xpToNextLevel);
    }
}

