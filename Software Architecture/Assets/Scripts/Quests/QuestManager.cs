using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Auto start quests (no NPC)")]
    public List<QuestDefinition> startingQuests = new List<QuestDefinition>();

    [Header("Runtime Overrides (optional)")]
    public List<QuestOverride> overrides = new List<QuestOverride>();

    [Header("Audio (optional)")]
    public AudioSource audioSource; // if null, uses PlayClipAtPoint
    public AudioClip questCompleteSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    public event Action OnQuestsChanged;

    [Serializable]
    public class QuestState
    {
        public QuestDefinition def;
        public int currentAmount;
        public bool completed;
        public int requiredOverride;

        public int Required => requiredOverride > 0 ? requiredOverride : (def != null ? def.requiredAmount : 0);
    }

    [Serializable]
    public class QuestOverride
    {
        public string questId;
        [Min(1)] public int requiredAmount = 1;
    }

    readonly List<QuestState> active = new List<QuestState>();

    public IReadOnlyList<QuestState> ActiveQuests => active;

    InventoryManager hookedInventory;
    bool endgameShown;
    [Header("Endgame UI (optional)")]
    [Min(0f)] public float endgameDelaySeconds = 3f;

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

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
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

            int requiredOverride = 0;
            if (overrides != null && overrides.Count > 0 && !string.IsNullOrEmpty(def.questId))
            {
                for (int i = 0; i < overrides.Count; i++)
                {
                    var ov = overrides[i];
                    if (ov == null) continue;
                    if (string.IsNullOrEmpty(ov.questId)) continue;
                    if (ov.questId != def.questId) continue;
                    requiredOverride = Mathf.Max(1, ov.requiredAmount);
                    break;
                }
            }

            active.Add(new QuestState
            {
                def = def,
                currentAmount = 0,
                completed = false,
                requiredOverride = requiredOverride
            });
        }

        RaiseChanged();
    }

    public bool IsQuestCompleted(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return false;
        for (int i = 0; i < active.Count; i++)
        {
            var q = active[i];
            if (q == null || q.def == null) continue;
            if (q.def.questId != questId) continue;
            return q.completed;
        }
        return false;
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
                PlaySfx(questCompleteSfx, transform.position);
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

            bool wasCompleted = q.completed;
            int qty = inv.GetItemQuantity(q.def.targetItem);
            q.currentAmount = Mathf.Clamp(qty, 0, q.Required);
            if (!q.completed && q.currentAmount >= q.Required)
            {
                q.completed = true;
                Debug.Log($"QUEST COMPLETE: {q.def.title}");
                if (!wasCompleted)
                    PlaySfx(questCompleteSfx, transform.position);
            }
        }
    }

    void PlaySfx(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;
        if (sfxVolume <= 0f) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, pos, sfxVolume);
    }

    void RaiseChanged()
    {
        OnQuestsChanged?.Invoke();
        TryShowEndgameIfAllCompleted();
    }

    void TryShowEndgameIfAllCompleted()
    {
        if (endgameShown) return;
        if (active == null || active.Count == 0) return;

        bool allCompleted = true;
        for (int i = 0; i < active.Count; i++)
        {
            var q = active[i];
            if (q == null || q.def == null) continue;
            allCompleted &= q.completed;
        }

        if (!allCompleted) return;
        endgameShown = true;
        StartCoroutine(ShowEndgameAfterDelay());
    }

    IEnumerator ShowEndgameAfterDelay()
    {
        float delay = Mathf.Max(0f, endgameDelaySeconds);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);
        GameCompleteUI.Show();
    }
}

