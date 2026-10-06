using UnityEngine;

public class SoundRing : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0f)] private float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private bool hasHitPlayer;

    private void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

        if (hasHitPlayer)
        {
            return;
        }

        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 4f);
        foreach (Collider2D overlappingCollider in overlappingColliders)
        {
            if (TryHitPlayer(overlappingCollider))
            {
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHitPlayer(collision);
    }

    private bool TryHitPlayer(Collider2D collision)
    {
        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if (hasHitPlayer || player == null)
        {
            return false;
        }

        hasHitPlayer = true;
        player.ApplySoundWaveHit(Vector2.left);
        return true;
    }
}