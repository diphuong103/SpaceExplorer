using UnityEngine;

public class WhaleMini : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speedMultiplier = 1f;
    [SerializeField] private float despawnX = -10f;
    [SerializeField, Min(0f)] private float waveForwardOffset = 1.5f;
    [SerializeField, Min(0.1f)] private float waveHitRadius = 1.5f;
    [SerializeField, Min(0f)] private float waveDamage = 1f;
    [SerializeField, Min(0.1f)] private float waveHitInterval = 1f;

    private Rigidbody2D body;
    private float waveHitTimer;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.useFullKinematicContacts = false;

        BoxCollider2D bodyCollider = GetComponent<BoxCollider2D>();
        if (bodyCollider == null)
        {
            bodyCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        bodyCollider.isTrigger = false;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            bodyCollider.size = spriteRenderer.sprite.bounds.size;
            bodyCollider.offset = spriteRenderer.sprite.bounds.center;
        }

        gameObject.tag = "Obstacles";
    }

    private void Update()
    {
        waveHitTimer -= Time.deltaTime;
        TryHitPlayerWithWave();
    }

    private void FixedUpdate()
    {
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.worldSpeed : 1f;
        float boost = PlayerController.Instance != null ? PlayerController.Instance.boost : 1f;
        float speed = Mathf.Max(0f, worldSpeed * boost * speedMultiplier);
        Vector2 nextPosition = body.position + Vector2.left * speed * Time.fixedDeltaTime;
        body.MovePosition(nextPosition);

        if (nextPosition.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }

    private void TryHitPlayerWithWave()
    {
        if (waveHitTimer > 0f)
        {
            return;
        }

        Vector2 waveCenter = (Vector2)transform.position + Vector2.left * waveForwardOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(waveCenter, waveHitRadius);

        foreach (Collider2D hit in hits)
        {
            PlayerController player = hit.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                continue;
            }

            player.ApplySoundWaveHit(Vector2.left, waveDamage);
            waveHitTimer = Mathf.Max(0.1f, waveHitInterval);
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 waveCenter = transform.position + Vector3.left * waveForwardOffset;
        Gizmos.DrawWireSphere(waveCenter, waveHitRadius);
    }
}
