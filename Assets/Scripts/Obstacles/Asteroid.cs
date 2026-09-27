using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites; // Các sprite cho tiểu hành tinh

    private SpriteRenderer spriteRenderer;  
    private Rigidbody2D rb; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();  
        rb = GetComponent<Rigidbody2D>(); 
        
        // 1. Kiểm tra an toàn mảng sprites trước khi truy cập
        if (sprites != null && sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        }

        // 2. Tạo lực đẩy/xoay tự do ban đầu
        float pushX = Random.Range(-1f, 1f); 
        float pushY = Random.Range(-1f, 1f);  
        rb.linearVelocity = new Vector2(pushX, pushY).normalized * Random.Range(1f, 3f);
    }

    void Update()
    {
        // 3. Lấy tốc độ cuộn thế giới (kiểm tra Null để tránh văng lỗi)
        float currentWorldSpeed = 1f;
        if (GameManager.Instance != null)
        {
            currentWorldSpeed = GameManager.Instance.worlSpeed;
        }

        // 4. Lấy trạng thái tăng tốc từ Player
        float boostFactor = 1f;
        if (PlayerController.Instance != null)
        {
            // Giả định: Tốc độ trôi tăng lên khi Player đang Boost
            // (Cần khai báo public bool IsBoosting => isBoosting; bên PlayerController)
            // boostFactor = PlayerController.Instance.IsBoosting ? 1.5f : 1f;
        }

        // 5. Di chuyển tiểu hành tinh trôi về bên TRÁI (-X)
        float moveX = currentWorldSpeed * boostFactor;
        transform.position -= new Vector3(moveX * Time.deltaTime, 0, 0); 

        // 6. Hủy bỏ khi tiểu hành tinh đi khuất khỏi màn hình bên TRÁI
        if (transform.position.x < -10f) 
        {
            Destroy(gameObject); 
        }
    }
}