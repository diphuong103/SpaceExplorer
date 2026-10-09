using System.Collections;
using UnityEngine;

public class Boom : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField, Min(0.1f)] private float fallbackDuration = 2f;

    private IEnumerator Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        yield return null;

        float duration = fallbackDuration;
        if (animator != null && animator.runtimeAnimatorController != null && animator.layerCount > 0)
        {
            float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            if (animationDuration > 0f)
            {
                duration = animationDuration;
            }
            else
            {
                Debug.LogWarning($"Explosion '{name}' has no valid animation duration; using fallback duration.", this);
            }
        }
        else
        {
            Debug.LogWarning($"Explosion '{name}' has no Animator; using fallback duration.", this);
        }

        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
