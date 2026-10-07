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

    public void Initialize(float bulletSpeed, int bulletDamage)
    {
        speed = Mathf.Max(0f, bulletSpeed);
        damage = Mathf.Max(1, bulletDamage);
        hasHit = false;
        body.linearVelocity = Vector2.right * speed;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null && IsAtRightScreenEdge())
        {
            Destroy(gameObject);
        }
    }

    private bool IsAtRightScreenEdge()
    {
        Collider2D bulletCollider = GetComponent<Collider2D>();
        float bulletRightEdge = bulletCollider != null
            ? bulletCollider.bounds.max.x
            : transform.position.x;

        float viewportY = mainCamera.WorldToViewportPoint(transform.position).y;
        float cameraDepth = mainCamera.WorldToViewportPoint(transform.position).z;
        float screenRightEdge = mainCamera.ViewportToWorldPoint(
            new Vector3(1f, viewportY, cameraDepth)).x;

        return bulletRightEdge >= screenRightEdge;
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
}
