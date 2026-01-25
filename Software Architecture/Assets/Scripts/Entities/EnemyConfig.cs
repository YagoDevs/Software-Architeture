/*
This script defines enemy stats and rewards as a ScriptableObject (HP, damage, speed, drops).
*/

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Identity")]
    public string enemyId = "enemy_basic";
    public string displayName = "Enemy";
    public bool isBoss = false;

    [Header("Stats")]
    [Min(1)] public int maxHP = 20;
    [Min(0)] public int contactDamage = 5;
    [Min(0.1f)] public float moveSpeed = 2f;

    [Header("Attack")]
    [Min(0.1f)] public float attackRange = 1.6f;
    [Min(0.05f)] public float attackCooldown = 1.0f;

    [Header("Rewards")]
    [Min(0)] public int xpReward = 5;
    public List<DropEntry> drops = new List<DropEntry>();

    [Serializable]
    public class DropEntry
    {
        public ItemData item;
        [Range(0f, 1f)] public float chance = 0.35f;
        [Min(1)] public int minQuantity = 1;
        [Min(1)] public int maxQuantity = 1;
    }
}

