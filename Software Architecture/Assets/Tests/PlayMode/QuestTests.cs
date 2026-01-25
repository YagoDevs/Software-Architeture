/*
This test file is used to validate quest progression in PlayMode (kill quest completion through a kill event).
*/

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class QuestTests
{
    static void SetPrivateBool(object obj, string fieldName, bool value)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, $"Private field '{fieldName}' was not found on {obj.GetType().Name}.");
        f.SetValue(obj, value);
    }

    static void ResetQuestManagerSingleton()
    {
        var prop = typeof(QuestManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        if (prop == null) return;
        var setMethod = prop.GetSetMethod(true);
        if (setMethod != null) setMethod.Invoke(null, new object[] { null });
    }

    [UnityTest]
    public IEnumerator Quest_KillQuest_CompletesWhenMatchingEnemyKilled()
    {
        if (QuestManager.Instance != null)
        {
            Object.DestroyImmediate(QuestManager.Instance.gameObject);
            ResetQuestManagerSingleton();
        }

        var qDef = ScriptableObject.CreateInstance<QuestDefinition>();
        qDef.questId = "test_kill_quest";
        qDef.title = "Kill Test Enemy";
        qDef.type = QuestType.KillEnemy;
        qDef.requiredAmount = 1;
        qDef.targetEnemyId = "goblin";

        var go = new GameObject("QuestManager_Test");
        var qm = go.AddComponent<QuestManager>();

        // avoid opening endgame UI during test.
        SetPrivateBool(qm, "endgameShown", true);

        qm.StartQuests(new[] { qDef });

        var enemyCfg = ScriptableObject.CreateInstance<EnemyConfig>();
        enemyCfg.enemyId = "goblin";

        CombatEvents.RaiseEnemyKilled(enemyCfg);

        Assert.IsTrue(qm.ActiveQuests.Count > 0);
        Assert.IsTrue(qm.ActiveQuests[0].completed);

        yield return null;

        Object.Destroy(go);
        Object.Destroy(qDef);
        Object.Destroy(enemyCfg);
        ResetQuestManagerSingleton();
    }
}

