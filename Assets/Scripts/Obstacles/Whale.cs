using System.Collections;
using UnityEngine;

public class Whale : MonoBehaviour
{
    [Header("--- Whale Stats ---")]
    [SerializeField, Min(1)] private int health = 1;

    [Header("--- Sequence Settings ---")]
    [SerializeField] private float stopX = 3.5f;
    [SerializeField] private Sprite howlSprite;
    [SerializeField] private GameObject soundRingPrefab;
    [SerializeField] private Transform mouthPoint;
    [SerializeField, Min(0f)] private float chargeDelay = 0.5f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 1f;

    [Header("Hit Effects")]
    [SerializeField, Min(0f)] private float hitFlashDuration = 0.12f;
    [SerializeField] private Material colorMaterial;
    [SerializeField] private GameObject destroyEffectPrefab;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Rigidbody2D body;
    private Coroutine sequenceCoroutine;
    private Coroutine hitFlashCoroutine;
    private bool isDead;

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
        AutoFindMouthPoint();
    }

    private void OnValidate()
    {
        AutoFindMouthPoint();
    }

    private void AutoFindMouthPoint()
    {
        if (mouthPoint == null)
        {
            mouthPoint = transform.Find("MouthPoint");
        }
    }

    private void Start()
    {
        sequenceCoroutine = StartCoroutine(WhaleSequenceRoutine());
    }

    private IEnumerator WhaleSequenceRoutine()
    {
        while (!isDead && body.position.x > stopX)
        {
            float worldSpeed = GameManager.Instance != null ? GameManager.Instance.worldSpeed : 1f;
            float boost = PlayerController.Instance != null ? PlayerController.Instance.boost : 1f;
            float moveDistance = Mathf.Max(0f, worldSpeed * boost) * Time.fixedDeltaTime;
            float nextX = Mathf.Max(stopX, body.position.x - moveDistance);

            body.MovePosition(new Vector2(nextX, body.position.y));
            if (nextX <= stopX)
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        if (isDead)
        {
            yield break;
        }

        if (howlSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = howlSprite;
        }

        yield return new WaitForSeconds(chargeDelay);
        if (isDead)
        {
            yield break;
        }

        if (soundRingPrefab != null && mouthPoint != null)
        {
            Instantiate(soundRingPrefab, mouthPoint.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.8f);
        if (isDead)
        {
            yield break;
        }

        if (spriteRenderer != null)
        {
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;

            while (elapsed < fadeOutDuration && !isDead)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeOutDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            if (isDead)
            {
                yield break;
            }

            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        }

        Destroy(gameObject);
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
            if (sequenceCoroutine != null)
            {
                StopCoroutine(sequenceCoroutine);
                sequenceCoroutine = null;
            }

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
}