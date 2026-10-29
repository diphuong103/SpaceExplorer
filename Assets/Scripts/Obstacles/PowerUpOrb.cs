using UnityEngine;

public class PowerUpOrb : MonoBehaviour
{
    [SerializeField] private PowerUpType powerUpType;
    [Header("Score")]
    [SerializeField, Min(0), Tooltip("Điểm cộng khi Player nhặt orb này.")]
    private int scoreOnPickup = 1;

    public PowerUpType Type => powerUpType;

    private bool collected;

    // Đảm bảo orb có trigger và căn chỉnh collider theo kích thước sprite.
    private void Awake()
    {
        // Đảm bảo prefab có trigger để Unity gọi OnTriggerEnter2D khi Player chạm orb.
        CircleCollider2D pickupCollider = GetComponent<CircleCollider2D>();
        if (pickupCollider == null)
        {
            pickupCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        pickupCollider.isTrigger = true;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            pickupCollider.offset = spriteBounds.center;
            pickupCollider.radius = Mathf.Max(spriteBounds.extents.x, spriteBounds.extents.y);
        }
    }

    // Kích hoạt buff, cộng điểm và xử lý âm thanh khi Player nhặt orb.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        // Hỗ trợ collider va chạm nằm trên object con trong prefab Player.
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        collected = true;
        if (powerUpType != PowerUpType.None)
        {
            // Chuyển loại orb sang PlayerBuffManager để áp dụng hiệu ứng gameplay tương ứng.
            player.ActivatePowerUp(powerUpType);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreOnPickup);
        }
        else
        {
            Debug.LogError("Cannot award orb points: GameManager is missing from the scene.", this);
        }

        if (AudioManager.Instance != null)
        {
            // Âm thanh nhặt orb được phát qua AudioManager của scene.
            AudioManager.Instance.PlayPowerUpPickup();
        }

        Destroy(gameObject);
    }
}
