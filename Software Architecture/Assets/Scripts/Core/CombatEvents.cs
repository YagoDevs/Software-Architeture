/*
This script is used as a simple event bus. It broadcasts when an enemy is killed so other systems can react without tight coupling.
*/

using System;

public static class CombatEvents
{
    // Fired when an enemy dies (regular or boss).
    public static event Action<EnemyConfig> OnEnemyKilled;

    public static void RaiseEnemyKilled(EnemyConfig config)
    {
        OnEnemyKilled?.Invoke(config);
    }
}

