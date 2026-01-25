/*
This script is used to display the player's HP and XP/level on the HUD by subscribing to player events.
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Auto-find (optional)")]
    public PlayerHealth playerHealth;
    public PlayerProgression playerProgression;

    [Header("HP UI (optional)")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText; // ex: "HP 80/100"

    [Header("XP/Level UI (optional)")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText; // ex: "Lv 2"
    public TextMeshProUGUI xpText;    // ex: "XP 5/35"

    void Awake()
    {
        AutoFindIfNeeded();
    }

    void OnEnable()
    {
        AutoFindIfNeeded();

        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;

        if (playerProgression != null)
        {
            playerProgression.OnXPChanged += HandleXPChanged;
            playerProgression.OnLevelUp += HandleLevelUp;
        }

        // Force initial paint
        if (playerHealth != null) HandleHealthChanged(playerHealth.CurrentHP, playerHealth.maxHP);
        if (playerProgression != null) HandleXPChanged(playerProgression.level, playerProgression.currentXP, playerProgression.xpToNextLevel);
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;

        if (playerProgression != null)
        {
            playerProgression.OnXPChanged -= HandleXPChanged;
            playerProgression.OnLevelUp -= HandleLevelUp;
        }
    }

    void AutoFindIfNeeded()
    {
        if (playerHealth != null && playerProgression != null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (playerHealth == null) playerHealth = player.GetComponent<PlayerHealth>();
        if (playerProgression == null) playerProgression = player.GetComponent<PlayerProgression>();
    }

    void HandleHealthChanged(int current, int max)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }

        if (hpText != null)
            hpText.text = $"HP {current}/{max}";
    }

    void HandleXPChanged(int level, int currentXP, int xpToNext)
    {
        if (levelText != null)
            levelText.text = $"Lv {level}";

        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNext;
            xpSlider.value = currentXP;
        }

        if (xpText != null)
            xpText.text = $"XP {currentXP}/{xpToNext}";
    }

    void HandleLevelUp(int newLevel)
    {
        // optional hook for animations later
        if (levelText != null)
            levelText.text = $"Lv {newLevel}";
    }
}

