/*
This script implements the boss melee-hit attack strategy.
It applies damage if the target is within the configured range.
*/

using System.Collections;

public sealed class BossMeleeHitStrategy : IBossAttackStrategy
{
    public IEnumerator Execute(BossController boss, BossAttackDefinition attack)
    {
        if (boss == null || attack == null) yield break;
        boss.ApplyDamageIfInRange(attack.damage, attack.range);
    }
}

