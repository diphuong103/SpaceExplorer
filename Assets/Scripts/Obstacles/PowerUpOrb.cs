using UnityEngine;

public class PowerUpOrb : MonoBehaviour
{
    [SerializeField] private PowerUpType powerUpType;
    [Header("Score")]
    [SerializeField, Min(0), Tooltip("Điểm cộng khi Player nhặt orb này.")]
    private int scoreOnPickup = 1;

    public PowerUpType Type => powerUpType;

    private bool collected;

    private void Awake()
    {
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        collected = true;
        if (powerUpType != PowerUpType.None)
        {
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
            AudioManager.Instance.PlayPowerUpPickup();
        }

        Destroy(gameObject);
    }
}
