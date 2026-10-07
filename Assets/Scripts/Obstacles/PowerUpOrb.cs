using UnityEngine;

public class PowerUpOrb : MonoBehaviour
{
    private bool collected;

    private void Awake()
    {
        CircleCollider2D pickupCollider = GetComponent<CircleCollider2D>();
        if (pickupCollider == null)
        {
            pickupCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        pickupCollider.isTrigger = true;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            pickupCollider.offset = spriteBounds.center;
            pickupCollider.radius = Mathf.Max(spriteBounds.extents.x, spriteBounds.extents.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        collected = true;
        Destroy(gameObject);
    }
}
