/*
This script defines a boss attack as a ScriptableObject (type, damage, range, cooldown, windup, animation trigger).
*/

using UnityEngine;

public enum BossAttackType
{
    MeleeHit,
    SlamAOE,
    Dash
}

[CreateAssetMenu(fileName = "BossAttack", menuName = "Game/Boss Attack")]
public class BossAttackDefinition : ScriptableObject
{
    [Header("Identity")]
    public string attackId = "boss_attack";
    public string displayName = "Boss Attack";
    public BossAttackType type = BossAttackType.MeleeHit;

    [Header("Timing")]
    [Min(0f)] public float cooldown = 2f;
    [Min(0f)] public float windup = 0.25f; // delay before damage/move (sync with animation)

    [Header("Common")]
    [Min(0)] public int damage = 10;
    [Min(0.1f)] public float range = 3f; // planar distance check to allow attack
    public string animationTrigger = "BossAttack";

    [Header("Slam AOE")]
    [Min(0.5f)] public float aoeRadius = 4f;

    [Header("Dash")]
    [Min(0.5f)] public float dashDistance = 6f;
    [Min(0.1f)] public float dashDuration = 0.2f;
}

