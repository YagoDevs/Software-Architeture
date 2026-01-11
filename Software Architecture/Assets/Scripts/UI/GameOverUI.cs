using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
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

