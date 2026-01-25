/*
This script implements the boss slam AOE attack strategy.
It applies damage if the target is within the configured AOE radius.
*/

using System.Collections;

public sealed class BossSlamAOEStrategy : IBossAttackStrategy
{
    public IEnumerator Execute(BossController boss, BossAttackDefinition attack)
    {
        if (boss == null || attack == null) yield break;
        boss.ApplyDamageIfInRange(attack.damage, attack.aoeRadius);
    }
}

