using UnityEngine;

public class PhaserBullet : MonoBehaviour
{
    private float speed;
    private int damage;
    private bool hasHit;
    private Camera mainCamera;
    private Rigidbody2D body;

    // Lưu camera/vật lý của viên đạn và cấu hình collider thành trigger.
    private void Awake()
    {
        mainCamera = Camera.main;
        body = GetComponent<Rigidbody2D>();

        Collider2D bulletCollider = GetComponent<Collider2D>();
        if (bulletCollider != null)
        {
            // Đạn dùng trigger để va chạm được xử lý bởi OnTriggerEnter2D thay vì phản lực vật lý.
            bulletCollider.isTrigger = true;
        }
    }

    // Hủy viên đạn khi nó đi ra ngoài khung nhìn camera.
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

    // Kiểm tra vị trí viên đạn có nằm ngoài viewport camera hay không.
    private bool IsOutsideCamera()
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.z < 0f
            || viewportPosition.x < 0f
            || viewportPosition.x > 1f
            || viewportPosition.y < 0f
            || viewportPosition.y > 1f;
    }

    // Gây sát thương cho mục tiêu đầu tiên bị trúng rồi hủy viên đạn.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        // Đạn tìm script mục tiêu trên object cha vì collider thường nằm trong prefab nhiều tầng.
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

    // Thiết lập sát thương và vận tốc ban đầu cho viên đạn mới tạo.
    public void Initialize(float bulletSpeed, int bulletDamage, Vector2 direction)
    {
        speed = Mathf.Max(0f, bulletSpeed);
        damage = Mathf.Max(1, bulletDamage);
        hasHit = false;
        // Rigidbody2D nhận hướng và tốc độ từ PhaserWeapon ngay sau Instantiate.
        body.linearVelocity = direction.normalized * speed;
    }
}
