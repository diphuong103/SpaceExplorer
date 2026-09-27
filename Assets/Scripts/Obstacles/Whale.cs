using UnityEngine;

public class Whale : MonoBehaviour
{
    void Update()
    {
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.worlSpeed : 1f;
        float boost = PlayerController.Instance != null ? PlayerController.Instance.boost : 1f;
        float moveX = worldSpeed * boost * Time.deltaTime;
        transform.position += new Vector3(-moveX, 0, 0); // Di chuyển
        if(transform.position.x < -10f) // Hủy bỏ khi đi khuất khỏi màn hình bên TRÁI
        {
            Destroy(gameObject);
        }
    }
}
