/*
This script implements a melee attack strategy for the player.
It searches for nearby enemies in range and applies damage to the first valid target.
*/

using System.Collections;
using UnityEngine;

public sealed class PlayerMeleeAttackStrategy : IPlayerAttackStrategy
{
    public void Execute(PlayerCombat combat, PlayerAttackRequest request, int damage)
    {
        if (combat == null) return;

        // Trigger animation/sfx via the Context (PlayerCombat) to keep the strategy focused on the algorithm.
        combat.TriggerAttackAnimation(request.animatorTrigger);
        if (request.windupSeconds > 0f)
        {
            combat.StartCoroutine(ExecuteAfterWindup(combat, request, damage));
            return;
        }

        combat.PlayAttackSfx(request.sfx, combat.transform.position);
        ApplyMelee(combat, request, damage);
    }

    IEnumerator ExecuteAfterWindup(PlayerCombat combat, PlayerAttackRequest request, int damage)
    {
        yield return new WaitForSeconds(request.windupSeconds);

        // If UI opened during windup, cancel
        if (!combat.IsInputAllowed())
            yield break;

        combat.PlayAttackSfx(request.sfx, combat.transform.position);
        ApplyMelee(combat, request, damage);
    }

    static void ApplyMelee(PlayerCombat combat, PlayerAttackRequest request, int damage)
    {
        if (combat == null) return;

        // Simple melee: hit any EnemyHealth near the player
        Collider[] hits = Physics.OverlapSphere(combat.transform.position, request.range, request.enemyLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            var eh = hits[i].GetComponentInParent<EnemyHealth>();
            if (eh != null && !eh.IsDead)
            {
                eh.TakeDamage(damage);
                combat.LogEnemyHit(eh, isAlt: request.isAlt);
                // 1 target per swing (simple + readable)
                return;
            }
        }
    }
}

