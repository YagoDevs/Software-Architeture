using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Auto start quests (no NPC)")]
    public List<QuestDefinition> startingQuests = new List<QuestDefinition>();

    public event Action OnQuestsChanged;

    [Serializable]
    public class QuestState
    {
        public QuestDefinition def;
        public int currentAmount;
        public bool completed;

        public int Required => def != null ? def.requiredAmount : 0;
    }

    readonly List<QuestState> active = new List<QuestState>();

    public IReadOnlyList<QuestState> ActiveQuests => active;

    InventoryManager hookedInventory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        CombatEvents.OnEnemyKilled += HandleEnemyKilled;
        TryHookInventory();
    }

    void OnDisable()
    {
        CombatEvents.OnEnemyKilled -= HandleEnemyKilled;
        UnhookInventory();
    }

    void Start()
    {
        StartQuests(startingQuests);
        TryHookInventory();
        RefreshFetchQuests();
        RaiseChanged();
    }

    void Update()
    {
        // In some scenes, QuestManager can initialize before InventoryManager.
        // Keep it simple: retry until we successfully hook.
        if (hookedInventory == null)
            TryHookInventory();
    }

    void TryHookInventory()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;
        if (hookedInventory == inv) return;

        UnhookInventory();
        hookedInventory = inv;
        hookedInventory.OnInventoryChanged += HandleInventoryChanged;
    }

    void UnhookInventory()
    {
        if (hookedInventory == null) return;
        hookedInventory.OnInventoryChanged -= HandleInventoryChanged;
        hookedInventory = null;
    }

    public void StartQuests(IEnumerable<QuestDefinition> defs)
    {
        if (defs == null) return;

        foreach (var def in defs)
        {
            if (def == null) continue;
            if (active.Any(q => q.def == def)) continue;

            active.Add(new QuestState
            {
                def = def,
                currentAmount = 0,
                completed = false
            });
        }

        RaiseChanged();
    }

    void HandleEnemyKilled(EnemyConfig config)
    {
        if (config == null) return;

        bool changed = false;
        for (int i = 0; i < active.Count; i++)
        {
            var q = active[i];
            if (q == null || q.def == null) continue;
            if (q.completed) continue;
            if (q.def.type != QuestType.KillEnemy) continue;
            if (string.IsNullOrEmpty(q.def.targetEnemyId)) continue;
            if (q.def.targetEnemyId != config.enemyId) continue;

            q.currentAmount = Mathf.Min(q.Required, q.currentAmount + 1);
            if (q.currentAmount >= q.Required)
            {
                q.completed = true;
                Debug.Log($"QUEST COMPLETE: {q.def.title}");
            }
            changed = true;
        }

        if (changed) RaiseChanged();
    }

    void HandleInventoryChanged()
    {
        RefreshFetchQuests();
        RaiseChanged();
    }

    void RefreshFetchQuests()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        for (int i = 0; i < active.Count; i++)
        {
            var q = active[i];
            if (q == null || q.def == null) continue;
            if (q.def.type != QuestType.FetchItem) continue;
            if (q.def.targetItem == null) continue;

            int qty = inv.GetItemQuantity(q.def.targetItem);
            q.currentAmount = Mathf.Clamp(qty, 0, q.Required);
            if (!q.completed && q.currentAmount >= q.Required)
            {
                q.completed = true;
                Debug.Log($"QUEST COMPLETE: {q.def.title}");
            }
        }
    }

    void RaiseChanged()
    {
        OnQuestsChanged?.Invoke();
    }
}

