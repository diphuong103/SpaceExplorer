using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource ice;
    public AudioSource fire;
    public AudioSource hit;


    public AudioSource pause;
    public AudioSource resume;

    
    public AudioSource Boom2;
    public AudioSource HitRock;

    public AudioSource shoot;

    [SerializeField] private AudioClip pointPickupClip;
    [SerializeField] private AudioClip powerUpPickupClip;
    [SerializeField] private AudioClip laserActivationClip;
    [SerializeField] private AudioClip shieldActivationClip;
    [SerializeField] private AudioClip enemyHitClip;
    [SerializeField] private AudioClip enemyDestroyedClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Cannot play sound: AudioSource is not assigned.");
            return;
        }

        audioSource.Play();
    }

    public void PlayModifiedSound(AudioSource audioSource, float pitch)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Cannot play sound: AudioSource is not assigned.");
            return;
        }

        audioSource.pitch = pitch;
        audioSource.Play();
    }

    public void PlayPointPickup()
    {
        PlayOneShot(pointPickupClip);
    }

    public void PlayPowerUpPickup()
    {
        PlayOneShot(powerUpPickupClip);
    }

    public void PlayLaserActivation()
    {
        PlayOneShot(laserActivationClip);
    }

    public void PlayShieldActivation()
    {
        PlayOneShot(shieldActivationClip);
    }

    public void PlayEnemyHit()
    {
        PlayOneShot(enemyHitClip);
    }

    public void PlayEnemyDestroyed()
    {
        PlayOneShot(enemyDestroyedClip);
    }

    public void PlayAsteroidHit()
    {
        PlayOneShot(enemyHitClip);
    }

    public void PlayAsteroidDestroyed()
    {
        PlayOneShot(enemyDestroyedClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Cannot play sound: an AudioClip is not assigned.", this);
            return;
        }

        if (shoot == null)
        {
            Debug.LogError("Cannot play sound effect: assign the shared effects AudioSource.", this);
            return;
        }

        shoot.PlayOneShot(clip);
    }

    public void Fire()
    {
        PlaySound(fire);
    }

    public void Block()
    {
        PlaySound(hit);
    }
}


// Test git 