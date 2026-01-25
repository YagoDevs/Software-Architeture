/*
This test file is used to validate core enemy behavior in PlayMode (death flow and global kill event).
*/

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyTests
{
    static void SetPrivateInt(object obj, string fieldName, int value)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, $"Private field '{fieldName}' was not found on {obj.GetType().Name}.");
        f.SetValue(obj, value);
    }

    [UnityTest]
    public IEnumerator Enemy_Dies_RaisesCombatEventsOnEnemyKilled()
    {
        var cfg = ScriptableObject.CreateInstance<EnemyConfig>();
        cfg.enemyId = "test_enemy";
        cfg.displayName = "Test Enemy";
        cfg.maxHP = 5;

        var go = new GameObject("Enemy_Test");
        var eh = go.AddComponent<EnemyHealth>();
        eh.config = cfg;

        SetPrivateInt(eh, "currentHP", cfg.maxHP);

        bool raised = false;
        EnemyConfig received = null;

        void Handler(EnemyConfig c) { raised = true; received = c; }
        CombatEvents.OnEnemyKilled += Handler;
        try
        {
            eh.TakeDamage(999);
        }
        finally
        {
            CombatEvents.OnEnemyKilled -= Handler;
        }

        Assert.IsTrue(raised);
        Assert.AreSame(cfg, received);

        yield return null;
        Object.Destroy(go);
        Object.Destroy(cfg);
    }
}

