/*
This script defines the Strategy interface for player attacks. Concrete strategies (melee, fireball, etc.)
implement the attack algorithm while PlayerCombat acts as the Context that selects and executes a strategy.
*/

using UnityEngine;

public interface IPlayerAttackStrategy
{
    void Execute(PlayerCombat combat, PlayerAttackRequest request, int damage);
}

