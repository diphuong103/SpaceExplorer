using UnityEngine;

public class PointStar : MonoBehaviour
{
    [SerializeField, Min(0)] private int pointValue = 1;
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

        if (GameManager.Instance == null)
        {
            Debug.LogError("Cannot collect point star: GameManager is missing from the scene.", this);
            return;
        }

        GameManager.Instance.AddScore(pointValue);
        Destroy(gameObject);
    }
}
