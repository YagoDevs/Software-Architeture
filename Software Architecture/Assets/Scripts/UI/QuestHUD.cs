using System.Text;
using TMPro;
using UnityEngine;

public class QuestHUD : MonoBehaviour
{
    public QuestManager questManager;
    public TextMeshProUGUI questsText;

    [Header("Colors (TMP Rich Text)")]
    public string completedColorHex = "#2DFF6A";
    public string allCompletedColorHex = "#2DFF6A";

    void Awake()
    {
        if (questManager == null) questManager = QuestManager.Instance;
    }

    void OnEnable()
    {
        if (questManager == null) questManager = QuestManager.Instance;
        if (questManager != null) questManager.OnQuestsChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (questManager != null) questManager.OnQuestsChanged -= Refresh;
    }

    public void Refresh()
    {
        if (questsText == null) return;
        if (questManager == null)
        {
            questsText.text = "Quests: (QuestManager missing)";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Quests:");

        var quests = questManager.ActiveQuests;
        bool allCompleted = quests.Count > 0;
        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null || q.def == null) continue;
            allCompleted &= q.completed;
        }

        if (allCompleted)
            sb.AppendLine($"<color={allCompletedColorHex}>All quests completed!</color>");
        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null || q.def == null) continue;

            string status = q.completed ? "✓" : "";
            string line = $"- {q.def.title}: {q.currentAmount}/{q.Required} {status}".TrimEnd();

            // Only the completed quest turns green; if all completed, everything becomes green.
            if (allCompleted || q.completed)
                line = $"<color={completedColorHex}>{line}</color>";

            sb.AppendLine(line);
        }

        questsText.text = sb.ToString().TrimEnd();
    }
}

