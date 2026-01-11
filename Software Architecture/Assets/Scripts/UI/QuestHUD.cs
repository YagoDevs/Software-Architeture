using System.Text;
using TMPro;
using UnityEngine;

public class QuestHUD : MonoBehaviour
{
    public QuestManager questManager;
    public TextMeshProUGUI questsText;

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
        for (int i = 0; i < quests.Count; i++)
        {
            var q = quests[i];
            if (q == null || q.def == null) continue;

            string status = q.completed ? "✓" : "";
            sb.AppendLine($"- {q.def.title}: {q.currentAmount}/{q.Required} {status}".TrimEnd());
        }

        questsText.text = sb.ToString().TrimEnd();
    }
}

