using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameCompleteUI
{
    const string RootName = "GameCompleteUI_Root";

    public static bool IsShown => GameObject.Find(RootName) != null;

    public static void Show(string title = "Endgame!", string body = "You completed all quests!", string buttonText = "Play Again")
    {
        if (IsShown) return;

        // Pause gameplay
        Time.timeScale = 0f;

        var root = new GameObject(RootName);
        Object.DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        root.AddComponent<GraphicRaycaster>();

        // Dim background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(root.transform, false);
        var bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Panel
        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(root.transform, false);
        var panel = panelGO.AddComponent<Image>();
        panel.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(640f, 320f);
        panelRT.anchoredPosition = Vector2.zero;

        // Title (TMP)
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = title;
        titleTMP.fontSize = 44;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(0f, 80f);
        titleRT.anchoredPosition = new Vector2(0f, -30f);

        // Body (TMP)
        var bodyGO = new GameObject("Body");
        bodyGO.transform.SetParent(panelGO.transform, false);
        var bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyTMP.text = body;
        bodyTMP.fontSize = 24;
        bodyTMP.alignment = TextAlignmentOptions.Center;
        bodyTMP.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        bodyTMP.enableWordWrapping = true;
        var bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0.08f, 0.35f);
        bodyRT.anchorMax = new Vector2(0.92f, 0.75f);
        bodyRT.offsetMin = Vector2.zero;
        bodyRT.offsetMax = Vector2.zero;

        // Button
        var btnGO = new GameObject("RestartButton");
        btnGO.transform.SetParent(panelGO.transform, false);
        var btnImage = btnGO.AddComponent<Image>();
        btnImage.color = new Color(0.25f, 0.75f, 0.35f, 1f);
        var btn = btnGO.AddComponent<Button>();

        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0f);
        btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot = new Vector2(0.5f, 0f);
        btnRT.sizeDelta = new Vector2(320f, 70f);
        btnRT.anchoredPosition = new Vector2(0f, 30f);

        var btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.text = buttonText;
        btnTMP.fontSize = 28;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.color = Color.black;
        var btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        // Hook click
        btn.onClick.AddListener(Restart);
    }

    static void Restart()
    {
        Time.timeScale = 1f;
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);

        var existing = GameObject.Find(RootName);
        if (existing != null)
            Object.Destroy(existing);
    }
}

