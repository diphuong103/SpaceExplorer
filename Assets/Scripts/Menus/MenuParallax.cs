using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    [SerializeField] private float moveSpeed; // Đặt giá trị âm nếu muốn cuộn sang trái (ví dụ: -2)
    private float backgroundImageWidth;
    private Vector3 startPos; // Vị trí ban đầu của object, dùng làm mốc để wrap

    // Lưu vị trí gốc và tính chiều rộng ảnh để lặp nền menu.
    void Start()
    {
        startPos = transform.position;

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        // Lấy chiều rộng thực tế của ảnh, có nhân thêm scale của object
        backgroundImageWidth = (sprite.texture.width / sprite.pixelsPerUnit) * transform.localScale.x;
    }
    // Cuộn nền menu theo tốc độ cấu hình và lặp lại khi vượt quá ảnh.
    void Update()
    {
        // Di chuyển theo trục X
        float moveX = moveSpeed * Time.deltaTime;
        transform.position += new Vector3(moveX, 0, 0);

        // Khi trôi quá 1 chiều rộng ảnh theo hướng cuộn phải, nhảy lùi lại đúng 1 chiều rộng
        if (transform.position.x >= startPos.x + backgroundImageWidth)
        {
            transform.position -= new Vector3(backgroundImageWidth, 0, 0);
        }
        // Khi trôi quá 1 chiều rộng ảnh theo hướng cuộn trái, nhảy tới đúng 1 chiều rộng
        else if (transform.position.x <= startPos.x - backgroundImageWidth)
        {
            transform.position += new Vector3(backgroundImageWidth, 0, 0);
        }
    }
}
