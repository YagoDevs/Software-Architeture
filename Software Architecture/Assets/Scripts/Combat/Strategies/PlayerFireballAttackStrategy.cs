/*
This script implements a fireball (projectile) attack strategy for the player.
It spawns and launches a FireballProjectile with the provided damage and speed settings.
*/

using System.Collections;
using UnityEngine;

public sealed class PlayerFireballAttackStrategy : IPlayerAttackStrategy
{
    public void Execute(PlayerCombat combat, PlayerAttackRequest request, int damage)
    {
        if (combat == null) return;
        combat.TriggerAttackAnimation(request.animatorTrigger);
        combat.StartCoroutine(ExecuteAfterWindup(combat, request, damage));
    }

    IEnumerator ExecuteAfterWindup(PlayerCombat combat, PlayerAttackRequest request, int damage)
    {
        if (request.windupSeconds > 0f)
            yield return new WaitForSeconds(request.windupSeconds);

        // If UI opened during windup, cancel
        if (!combat.IsInputAllowed())
            yield break;

        if (!request.isFireball || request.fireballPrefab == null)
        {
            // Fallback to melee behavior if fireball is not configured.
            new PlayerMeleeAttackStrategy().Execute(combat, request, damage);
            yield break;
        }

        Vector3 spawnPos;
        Vector3 dir = combat.transform.forward;

        if (request.fireballSpawnPoint != null)
        {
            spawnPos = request.fireballSpawnPoint.position;
            dir = request.fireballSpawnPoint.forward;
        }
        else
        {
            spawnPos = combat.transform.position + Vector3.up * 1.2f + combat.transform.forward * request.fireballSpawnForwardOffset;
        }

        var fb = Object.Instantiate(request.fireballPrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));
        fb.Launch(dir, damage, request.fireballSpeed, request.enemyLayers);

        combat.PlayAttackSfx(request.fireballLaunchSfx != null ? request.fireballLaunchSfx : request.sfx, spawnPos);
    }
}

