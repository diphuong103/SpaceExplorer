using UnityEngine;

public class PhaserBullet : MonoBehaviour
{
    private float speed;
    private int damage;
    private bool hasHit;
    private Camera mainCamera;
    private Rigidbody2D body;

    private void Awake()
    {
        mainCamera = Camera.main;
        body = GetComponent<Rigidbody2D>();

        Collider2D bulletCollider = GetComponent<Collider2D>();
        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null && IsOutsideCamera())
        {
            Destroy(gameObject);
        }
    }

    private bool IsOutsideCamera()
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.z < 0f
            || viewportPosition.x < 0f
            || viewportPosition.x > 1f
            || viewportPosition.y < 0f
            || viewportPosition.y > 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        Asteroid asteroid = other.GetComponentInParent<Asteroid>();
        if (asteroid != null)
        {
            hasHit = true;
            asteroid.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Whale whale = other.GetComponentInParent<Whale>();
        if (whale != null)
        {
            hasHit = true;
            whale.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        WhaleMini whaleMini = other.GetComponentInParent<WhaleMini>();
        if (whaleMini != null)
        {
            hasHit = true;
            whaleMini.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public void Initialize(float bulletSpeed, int bulletDamage, Vector2 direction)
    {
        speed = Mathf.Max(0f, bulletSpeed);
        damage = Mathf.Max(1, bulletDamage);
        hasHit = false;
        body.linearVelocity = direction.normalized * speed;
    }
}
