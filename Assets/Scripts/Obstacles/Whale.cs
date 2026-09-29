using UnityEngine;

public class Whale : MonoBehaviour
{
    [SerializeField, Min(1)] private int health = 1;

    void Update()
    {
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.worldSpeed : 1f;
        float boost = PlayerController.Instance != null ? PlayerController.Instance.boost : 1f;
        float moveX = worldSpeed * boost * Time.deltaTime;
        transform.position += new Vector3(-moveX, 0, 0); // Di chuyển
        if(transform.position.x < -10f) // Hủy bỏ khi đi khuất khỏi màn hình bên TRÁI
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= Mathf.Max(0, damage);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
