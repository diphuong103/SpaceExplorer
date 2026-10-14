using UnityEngine;

public class PointStar : MonoBehaviour
{
    [Header("Score")]
    [SerializeField, Min(0), Tooltip("Điểm cộng khi Player nhặt sao này.")]
    private int pointValue = 1;
    [SerializeField, Min(0.1f)] private float visibleDuration = 10f;

    public int PointValue => pointValue;

    private void Start()
    {
        Destroy(gameObject, visibleDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(pointValue);
        }
        else
        {
            Debug.LogError("Cannot award points: GameManager is missing from the scene.", this);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPointPickup();
        }
        else
        {
            Debug.LogError("Cannot play point pickup sound: AudioManager is missing from the scene.", this);
        }

        Destroy(gameObject);
    }
}
