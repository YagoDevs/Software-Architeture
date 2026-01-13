using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Button restartButton;
    public TextMeshProUGUI titleText;
    public string title = "GAME OVER";

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        if (titleText != null)
            titleText.text = title;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(Restart);
            restartButton.onClick.AddListener(Restart);
        }
    }

    void Update()
    {
        // Simple fallback: press R to restart when game over panel is visible
        if (panel != null && panel.activeSelf && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Restart();
        }
    }

    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);

        if (titleText != null)
            titleText.text = title;
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Restart()
    {
        Hide();
        if (GameManager.Instance != null)
            GameManager.Instance.Restart();
    }
}

