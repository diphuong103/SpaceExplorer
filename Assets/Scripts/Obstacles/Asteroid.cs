using System.Collections;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites; 
    [SerializeField, Min(1)] private int health = 4;
    [Header("Score")]
    [SerializeField, Min(0), Tooltip("Điểm thưởng khi tiểu hành tinh bị phá hủy.")]
    private int scoreReward = 1;
    [SerializeField, Min(0f)] private float hitFlashDuration = 0.12f;

    [Header("Material Flash")]
    [SerializeField] private Material whiteMaterial;

    [SerializeField] private GameObject destroyEffectPrefab; // Prefab của hiệu ứng khi Asteroid bị phá hủy

    private SpriteRenderer spriteRenderer;  
    private Rigidbody2D rb; 
    private Material originalMaterial;
    private Coroutine hitFlashCoroutine;
    private bool isDestroyed;
    private bool hasStageTwoHealthMultiplier;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsStageTwoActive)
        {
            ApplyStageTwoHealthMultiplier(GameManager.Instance.StageTwoEnemyHealthMultiplier);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();  
        rb = GetComponent<Rigidbody2D>(); 

        if (spriteRenderer != null)
        {
            // Lưu material để có thể khôi phục sau hiệu ứng chớp khi bị trúng đạn.
            originalMaterial = spriteRenderer.material;

            // Chọn biến thể hình ảnh ngẫu nhiên từ mảng sprites được gán trong Inspector.
            if (sprites != null && sprites.Length > 0)
            {
                spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
            }
        }

        // Vận tốc Rigidbody2D tạo chuyển động ngẫu nhiên ban đầu; Update còn cuộn asteroid sang trái.
        float pushX = Random.Range(-1f, 1f); 
        float pushY = Random.Range(-1f, 1f);  
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(pushX, pushY).normalized * Random.Range(1f, 3f);
        }
    }

    public void ApplyStageTwoHealthMultiplier(float multiplier)
    {
        if (hasStageTwoHealthMultiplier)
        {
            return;
        }

        health = Mathf.CeilToInt(health * Mathf.Max(1f, multiplier));
        hasStageTwoHealthMultiplier = true;
    }

    private void Update()
    {
        // Đọc tốc độ chung từ GameManager để asteroid đồng bộ với nhịp cuộn của màn chơi.
        float currentWorldSpeed = 1f;
        if (GameManager.Instance != null)
        {
            currentWorldSpeed = GameManager.Instance.worldSpeed; 
        }

        // Trôi về bên TRÁI (-X)
        transform.position -= new Vector3(currentWorldSpeed * Time.deltaTime, 0, 0); 

        // Hủy khi trôi khỏi màn hình bên trái
        if (transform.position.x < -10f) 
        {
            Destroy(gameObject); 
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed || damage <= 0) return;

        health -= damage;
        FlashOnHit();

        if (health <= 0)
        {
            isDestroyed = true;

            // Điểm thưởng được cộng khi phá hủy asteroid; va chạm Player làm giảm HP ở PlayerController.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreReward);
            }
            else
            {
                Debug.LogError("Cannot award asteroid points: GameManager is missing from the scene.", this);
            }

            if (destroyEffectPrefab != null)
            {
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAsteroidDestroyed();
            }

            Destroy(gameObject, hitFlashDuration);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAsteroidHit();
        }
    }

    private void FlashOnHit()
    {
        if (spriteRenderer == null || whiteMaterial == null || hitFlashDuration <= 0f) return;

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
        }
        hitFlashCoroutine = StartCoroutine(ResetMaterialRoutine());
    }

    // Coroutine đổi Material sang màu trắng rồi khôi phục lại
    private IEnumerator ResetMaterialRoutine()
    {
        spriteRenderer.material = whiteMaterial;
        yield return new WaitForSeconds(hitFlashDuration);

        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }

        hitFlashCoroutine = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Chỉ hiệu ứng hình ảnh khi chạm Player/đạn; sát thương Player và asteroid được xử lý riêng.
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
        {
            FlashOnHit(); // Gọi chung hàm Flash đã đồng nhất
        }
    }
}