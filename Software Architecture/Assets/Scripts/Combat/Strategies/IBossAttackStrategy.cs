/*
This script defines the Strategy interface for boss attacks.
Concrete strategies (melee hit, slam AOE, dash, etc.) implement the attack algorithm,
while BossController acts as the Context that chooses which strategy to run.
*/

using System.Collections;

public interface IBossAttackStrategy
{
    IEnumerator Execute(BossController boss, BossAttackDefinition attack);
}

