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
    // Start is called once before the first execution of Update after the MonoBehaviour is created

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

    public void Fire()
    {
        PlaySound(fire);
    }

    public void Block()
    {
        PlaySound(hit);
    }
}
