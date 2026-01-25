/*
This script defines a small data object used by player attack strategies.
It keeps the attack parameters separate from the execution algorithm (Strategy Pattern).
*/

using UnityEngine;

public struct PlayerAttackRequest
{
    public float range;
    public float windupSeconds;

    public LayerMask enemyLayers;

    public string animatorTrigger;
    public AudioClip sfx;
    public bool isAlt;

    // Fireball-only (optional)
    public bool isFireball;
    public FireballProjectile fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballSpeed;
    public float fireballSpawnForwardOffset;
    public AudioClip fireballLaunchSfx;
}

