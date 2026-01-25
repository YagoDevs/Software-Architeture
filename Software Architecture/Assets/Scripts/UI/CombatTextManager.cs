/*
This script is used to spawn floating combat text on the UI from world positions.
*/

using TMPro;
using UnityEngine;

public class CombatTextManager : MonoBehaviour
{
    public static CombatTextManager Instance { get; private set; }

    [Header("UI")]
    public RectTransform canvasRect;
    public Camera worldCamera;
    public TextMeshProUGUI textPrefab; // prefab TMP (UI) with CombatText component

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (worldCamera == null) worldCamera = Camera.main;
        if (canvasRect == null) canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
    }

    public void SpawnWorldText(Vector3 worldPos, string text, Color color)
    {
        if (textPrefab == null) return;
        if (canvasRect == null) return;

        if (worldCamera == null) worldCamera = Camera.main;
        if (worldCamera == null) return;

        Vector3 screen = worldCamera.WorldToScreenPoint(worldPos);
        if (screen.z < 0) return; // behind camera

        var tmp = Instantiate(textPrefab, canvasRect);
        tmp.text = text;
        tmp.color = color;

        RectTransform rt = tmp.rectTransform;
        rt.position = screen;
    }
}

