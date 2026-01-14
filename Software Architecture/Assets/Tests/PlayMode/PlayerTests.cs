using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{
    static void SetPrivateInt(object obj, string fieldName, int value)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, $"Private field '{fieldName}' was not found on {obj.GetType().Name}.");
        f.SetValue(obj, value);
    }

    [UnityTest]
    public IEnumerator Player_TakeDamage_DecreasesHP_AndDiesAtZero()
    {
        var go = new GameObject("Player_Test");
        var ph = go.AddComponent<PlayerHealth>();

        ph.maxHP = 10;
        SetPrivateInt(ph, "currentHP", 10);

        bool died = false;
        ph.OnDied += () => died = true;

        ph.TakeDamage(3);
        Assert.AreEqual(7, ph.CurrentHP);
        Assert.IsFalse(ph.IsDead);
        Assert.IsFalse(died);

        ph.TakeDamage(7);
        Assert.AreEqual(0, ph.CurrentHP);
        Assert.IsTrue(ph.IsDead);
        Assert.IsTrue(died);

        yield return null;
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Player_GainXP_LevelsUp_IncreasesMaxHP_AndHeals()
    {
        var go = new GameObject("Player_XP_Test");
        var ph = go.AddComponent<PlayerHealth>();
        var prog = go.AddComponent<PlayerProgression>();

        ph.maxHP = 100;
        SetPrivateInt(ph, "currentHP", 50);

        prog.level = 1;
        prog.currentXP = 0;
        prog.xpToNextLevel = 10;
        prog.xpGrowthPerLevel = 999;
        prog.damageMultiplier = 1f;
        prog.damageMultiplierPerLevel = 0.12f;
        prog.maxHPPerLevel = 10;

        bool leveled = false;
        int newLevel = 0;
        prog.OnLevelUp += (lvl) => { leveled = true; newLevel = lvl; };

        prog.GainXP(10, "test");

        Assert.IsTrue(leveled);
        Assert.AreEqual(2, newLevel);
        Assert.AreEqual(2, prog.level);
        Assert.AreEqual(110, ph.maxHP);
        Assert.AreEqual(110, ph.CurrentHP);

        yield return null;
        Object.Destroy(go);
    }
}

