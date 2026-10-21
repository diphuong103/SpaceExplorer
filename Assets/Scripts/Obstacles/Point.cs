using UnityEngine;

public class PointStar : MonoBehaviour
{
    [Header("Score")]
    [SerializeField, Min(0), Tooltip("Điểm cộng khi Player nhặt sao này.")]
    private int pointValue = 1;
    [SerializeField, Min(0.1f)] private float visibleDuration = 10f;

    public int PointValue => pointValue;

    private bool collected;

    private void Start()
    {
        // Tự hủy sao sau thời gian hiển thị để vật phẩm không tồn tại vô hạn trong scene.
        Destroy(gameObject, visibleDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        // Collider của Player có thể nằm ở object con, nên tìm controller trên cả parent.
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        // Đánh dấu trước khi cộng điểm/phát âm thanh để nhiều collider không xử lý sao này lặp lại.
        collected = true;

        if (GameManager.Instance != null)
        {
            // GameManager áp dụng hệ số buff và cập nhật HUD tại một nơi duy nhất.
            GameManager.Instance.AddScore(pointValue);
        }
        else
        {
            Debug.LogError("Cannot award points: GameManager is missing from the scene.", this);
        }

        if (AudioManager.Instance != null)
        {
            // Dùng AudioManager chung thay vì gắn AudioSource riêng cho từng prefab sao.
            AudioManager.Instance.PlayPointPickup();
        }
        else
        {
            Debug.LogError("Cannot play point pickup sound: AudioManager is missing from the scene.", this);
        }

        Destroy(gameObject);
    }
}
