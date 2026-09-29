using UnityEngine;

public class PhaserBullet : MonoBehaviour
{
    private float speed;
    private int damage;
    private bool hasHit;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize(float bulletSpeed, int bulletDamage)
    {
        speed = Mathf.Max(0f, bulletSpeed);
        damage = Mathf.Max(1, bulletDamage);
        hasHit = false;
    }

    private void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null && mainCamera.WorldToViewportPoint(transform.position).x > 1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit)
        {
            return;
        }

        Asteroid asteroid = collision.gameObject.GetComponentInParent<Asteroid>();
        if (asteroid != null)
        {
            hasHit = true;
            asteroid.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Whale whale = collision.gameObject.GetComponentInParent<Whale>();
        if (whale != null)
        {
            hasHit = true;
            whale.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
