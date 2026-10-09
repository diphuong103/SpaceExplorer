using System.Collections;
using UnityEngine;

public class WhaleMini : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speedMultiplier = 1f;
    [SerializeField] private float despawnX = -10f;
    [SerializeField, Min(0f)] private float waveForwardOffset = 1.5f;
    [SerializeField, Min(0.1f)] private float waveHitRadius = 1.5f;
    [SerializeField, Min(0f)] private float waveDamage = 1f;
    [SerializeField, Min(0.1f)] private float waveHitInterval = 1f;

    [SerializeField, Min(1)] private int health = 1;
    [Header("Hit Effects")]
    [SerializeField, Min(0f)] private float hitFlashDuration = 0.12f;
    [SerializeField] private Material colorMaterial;
    [SerializeField] private GameObject destroyEffectPrefab;

    private bool isDead;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Coroutine hitFlashCoroutine;
    private float waveHitTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }

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
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            bodyCollider.size = spriteRenderer.sprite.bounds.size;
            bodyCollider.offset = spriteRenderer.sprite.bounds.center;
        }

        gameObject.tag = "Obstacles";
    }



    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        health -= damage;
        FlashOnHit();

        if (health <= 0)
        {
            isDead = true;

            if (destroyEffectPrefab != null)
            {
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject, hitFlashDuration);
        }
    }

    private void FlashOnHit()
    {
        if (spriteRenderer == null || colorMaterial == null || hitFlashDuration <= 0f)
        {
            return;
        }

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
        }

        hitFlashCoroutine = StartCoroutine(ResetMaterialRoutine());
    }

    private IEnumerator ResetMaterialRoutine()
    {
        spriteRenderer.material = colorMaterial;
        yield return new WaitForSeconds(hitFlashDuration);

        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }

        hitFlashCoroutine = null;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        waveHitTimer -= Time.deltaTime;
        TryHitPlayerWithWave();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

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
