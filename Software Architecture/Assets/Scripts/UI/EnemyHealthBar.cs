/*
This script is used to show an enemy HP bar above the enemy and keep it facing the camera.
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Target")]
    public EnemyHealth enemyHealth;

    [Header("UI (optional)")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText; // ex: "12/30"
    public TextMeshProUGUI nameText; // ex: "Goblin"

    [Header("Billboard")]
    public bool faceCamera = true;
    public Vector3 worldOffset = new Vector3(0, 2.0f, 0);

    [Header("Auto position (recommended)")]
    public bool autoOffsetFromTarget = true;
    public float extraHeight = 0.25f;

    Transform cachedTarget;
    Renderer cachedRenderer;
    Collider cachedCollider;

    void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
            cachedTarget = enemyHealth.transform;

        if (cachedTarget != null)
        {
            cachedRenderer = cachedTarget.GetComponentInChildren<Renderer>();
            cachedCollider = cachedTarget.GetComponentInChildren<Collider>();
        }

        ApplyStaticTexts();
    }

    void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged += HandleHealthChanged;

        // initial paint
        if (enemyHealth != null)
            HandleHealthChanged(enemyHealth.CurrentHP, enemyHealth.MaxHP);
    }

    void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= HandleHealthChanged;
    }

    void LateUpdate()
    {
        if (cachedTarget != null)
        {
            Vector3 offset = worldOffset;
            if (autoOffsetFromTarget)
                offset = Vector3.up * CalculateAutoHeight();

            transform.position = cachedTarget.position + offset;
        }

        if (!faceCamera) return;

        var cam = Camera.main;
        if (cam == null) return;

        // Look at camera (keep it readable)
        transform.forward = cam.transform.forward;
    }

    float CalculateAutoHeight()
    {
        // Prefer renderer bounds (visual size), fallback to collider, then fallback to worldOffset.y
        if (cachedRenderer != null)
        {
            float top = cachedRenderer.bounds.extents.y * 2f;
            return top + extraHeight;
        }

        if (cachedCollider != null)
        {
            float top = cachedCollider.bounds.extents.y * 2f;
            return top + extraHeight;
        }

        return Mathf.Max(0.5f, worldOffset.y);
    }

    void ApplyStaticTexts()
    {
        if (nameText == null) return;
        if (enemyHealth == null || enemyHealth.config == null)
        {
            nameText.text = "Enemy";
            return;
        }

        nameText.text = string.IsNullOrEmpty(enemyHealth.config.displayName) ? "Enemy" : enemyHealth.config.displayName;
    }

    void HandleHealthChanged(int current, int max)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }

        if (hpText != null)
            hpText.text = $"{current}/{max}";
    }
}

