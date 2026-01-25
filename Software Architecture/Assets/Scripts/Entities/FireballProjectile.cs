/*
This script is used to move a projectile forward and apply damage on trigger collision.
*/

using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Runtime")]
    public int damage = 10;
    public float speed = 18f;
    public float lifetime = 3f;
    public LayerMask hitLayers = ~0;
    public bool hitsEnemies = true;
    public bool hitsPlayer = false;

    [Header("FX (optional)")]
    public bool destroyOnHit = true;

    Rigidbody rb;
    float dieAt;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dieAt = Time.time + lifetime;
    }

    // Default launch (player fireball): hits enemies
    // here i normalize the direction to avoid the projectile to go crazy
    public void Launch(Vector3 direction, int dmg, float projectileSpeed, LayerMask layersToHit)
    {
        damage = dmg;
        speed = projectileSpeed;
        hitLayers = layersToHit;
        hitsEnemies = true;
        hitsPlayer = false;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            direction = transform.forward;
        direction.Normalize();

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            // Fallback if no Rigidbody
            transform.position += direction * 0.01f;
        }

        transform.forward = direction;
    }

    // this is for the projectile to move and destroy itself after a certain time
    void Update()
    {
        if (rb == null)
        {
            // Manual move fallback (no Rigidbody)
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        if (Time.time >= dieAt)
            Destroy(gameObject);
    }

    // this is for the projectile to hit the enemy or the player
    void OnTriggerEnter(Collider other)
    {
        // Layer filter
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

        if (hitsEnemies)
        {
            var enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage);
                if (destroyOnHit) Destroy(gameObject);
                return;
            }
        }

        if (hitsPlayer)
        {
            var ph = other.GetComponentInParent<PlayerHealth>();
            if (ph == null) ph = other.GetComponentInChildren<PlayerHealth>();
            if (ph != null && !ph.IsDead)
            {
                ph.TakeDamage(damage);
                if (destroyOnHit) Destroy(gameObject);
                return;
            }
        }
    }
}

