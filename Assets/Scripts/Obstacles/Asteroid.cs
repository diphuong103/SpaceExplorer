using System.Collections;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites; 
    [SerializeField, Min(1)] private int health = 4;
    [SerializeField, Min(0f)] private float hitFlashDuration = 0.12f;

    [Header("Material Flash")]
    [SerializeField] private Material whiteMaterial;

    private SpriteRenderer spriteRenderer;  
    private Rigidbody2D rb; 
    private Material originalMaterial;
    private Coroutine hitFlashCoroutine;
    private bool isDestroyed;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();  
        rb = GetComponent<Rigidbody2D>(); 

        if (spriteRenderer != null)
        {
            // Lưu lại Material gốc của Sprite
            originalMaterial = spriteRenderer.material;

            // Đổi ngẫu nhiên Sprite nếu mảng có dữ liệu
            if (sprites != null && sprites.Length > 0)
            {
                spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
            }
        }

        // Tạo lực đẩy/xoay tự do ban đầu
        float pushX = Random.Range(-1f, 1f); 
        float pushY = Random.Range(-1f, 1f);  
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(pushX, pushY).normalized * Random.Range(1f, 3f);
        }
    }

    void Update()
    {
        // Lấy tốc độ cuộn thế giới
        float currentWorldSpeed = 1f;
        if (GameManager.Instance != null)
        {
            // Nếu biến trong GameManager của bạn là worldSpeed thì giữ nguyên, còn chuẩn sẽ là worldSpeed
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
            Destroy(gameObject, hitFlashDuration);
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
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
        {
            FlashOnHit(); // Gọi chung hàm Flash đã đồng nhất
        }
    }
}