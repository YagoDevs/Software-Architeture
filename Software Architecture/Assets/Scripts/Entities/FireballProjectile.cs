using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Runtime")]
    public int damage = 10;
    public float speed = 18f;
    public float lifetime = 3f;
    public LayerMask hitLayers = ~0;

    [Header("FX (optional)")]
    public bool destroyOnHit = true;

    Rigidbody rb;
    float dieAt;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dieAt = Time.time + lifetime;
    }

    public void Launch(Vector3 direction, int dmg, float projectileSpeed, LayerMask layersToHit)
    {
        damage = dmg;
        speed = projectileSpeed;
        hitLayers = layersToHit;

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

    void OnTriggerEnter(Collider other)
    {
        // Layer filter
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

        var enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
            return;
        }
    }
}

