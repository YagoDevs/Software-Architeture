using UnityEngine;

public enum QuestType
{
    KillEnemy,
    FetchItem
}

[CreateAssetMenu(fileName = "QuestDefinition", menuName = "Game/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Info")]
    public string questId = "quest_1";
    public string title = "Quest";
    [TextArea(2, 4)] public string description = "Do something...";
    public QuestType type;

    [Header("Goal")]
    [Min(1)] public int requiredAmount = 1;

    [Header("Kill Enemy")]
    public string targetEnemyId; // must match EnemyConfig.enemyId

    [Header("Fetch Item")]
    public ItemData targetItem;
}

