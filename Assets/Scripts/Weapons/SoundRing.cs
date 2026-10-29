using UnityEngine;

public class SoundRing : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0f)] private float lifeTime = 3f;
    [SerializeField, Min(0f)] private float damage = 0.1f;

    // Tự hủy sóng sau thời gian tồn tại đã cấu hình.
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private bool hasHitPlayer;
    private bool hasStageTwoDamageMultiplier;

    // Nếu sóng được tạo ở Stage 2, áp dụng hệ số sát thương ngay khi khởi tạo.
    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsStageTwoActive)
        {
            ApplyStageTwoDamageMultiplier(GameManager.Instance.StageTwoEnemyDamageMultiplier);
        }
    }

    // Di chuyển sóng sang trái và kiểm tra mục tiêu trong vùng quanh sóng.
    private void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

        if (hasHitPlayer)
        {
            return;
        }

        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 4f);
        foreach (Collider2D overlappingCollider in overlappingColliders)
        {
            if (TryHitPlayer(overlappingCollider))
            {
                break;
            }
        }
    }

    // Xử lý va chạm Trigger trực tiếp với Player nếu collider được cấu hình phù hợp.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHitPlayer(collision);
    }

    // Gây sát thương tối đa một lần, kể cả khi Player có nhiều collider.
    private bool TryHitPlayer(Collider2D collision)
    {
        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if (hasHitPlayer || player == null)
        {
            return false;
        }

        hasHitPlayer = true;
        player.ApplySoundWaveHit(Vector2.left, damage);
        return true;
    }

    // Chỉ nhân sát thương một lần để không cộng dồn khi nhiều hệ thống áp dụng Stage 2.
    public void ApplyStageTwoDamageMultiplier(float multiplier)
    {
        if (hasStageTwoDamageMultiplier)
        {
            return;
        }

        damage *= Mathf.Max(1f, multiplier);
        hasStageTwoDamageMultiplier = true;
    }
}