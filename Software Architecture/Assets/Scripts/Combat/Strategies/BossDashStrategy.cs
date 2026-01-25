/*
This script implements the boss dash attack strategy.
It dashes forward, then optionally applies damage if the target is still in range.
*/

using System.Collections;

public sealed class BossDashStrategy : IBossAttackStrategy
{
    public IEnumerator Execute(BossController boss, BossAttackDefinition attack)
    {
        if (boss == null || attack == null) yield break;
        yield return boss.DashForward(attack.dashDistance, attack.dashDuration);
        boss.ApplyDamageIfInRange(attack.damage, attack.range);
    }
}

