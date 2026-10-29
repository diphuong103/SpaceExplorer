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

    // Khởi tạo singleton âm thanh và loại bỏ bản sao thừa trong scene.
    private void Awake()
    {
        if (Instance == null)
        {
            // Cho phép các component trong scene dùng chung AudioSource và AudioClip.
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Phát AudioSource được truyền vào nếu tham chiếu đã được gán.
    public void PlaySound(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Cannot play sound: AudioSource is not assigned.");
            return;
        }

        audioSource.Play();
    }

    // Đặt cao độ rồi phát AudioSource được truyền vào.
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

    // Phát hiệu ứng âm thanh khi người chơi nhặt sao.
    public void PlayPointPickup()
    {
        PlayOneShot(pointPickupClip);
    }

    // Phát hiệu ứng âm thanh khi người chơi nhặt power-up.
    public void PlayPowerUpPickup()
    {
        PlayOneShot(powerUpPickupClip);
    }

    // Phát hiệu ứng âm thanh khi kích hoạt laser.
    public void PlayLaserActivation()
    {
        PlayOneShot(laserActivationClip);
    }

    // Phát hiệu ứng âm thanh khi kích hoạt khiên.
    public void PlayShieldActivation()
    {
        PlayOneShot(shieldActivationClip);
    }

    // Phát hiệu ứng âm thanh khi kẻ địch bị trúng đòn.
    public void PlayEnemyHit()
    {
        PlayOneShot(enemyHitClip);
    }

    // Phát hiệu ứng âm thanh khi kẻ địch bị tiêu diệt.
    public void PlayEnemyDestroyed()
    {
        PlayOneShot(enemyDestroyedClip);
    }

    // Phát hiệu ứng âm thanh khi thiên thạch bị trúng đòn.
    public void PlayAsteroidHit()
    {
        PlayOneShot(enemyHitClip);
    }

    // Phát hiệu ứng âm thanh khi thiên thạch bị phá hủy.
    public void PlayAsteroidDestroyed()
    {
        PlayOneShot(enemyDestroyedClip);
    }

    // Phát một clip trên AudioSource hiệu ứng dùng chung.
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

        // PlayOneShot cho phép hiệu ứng chồng âm trên AudioSource đã gán trong Inspector.
        shoot.PlayOneShot(clip);
    }

    // Phát âm thanh hiệu ứng lửa đã gán trong Inspector.
    public void Fire()
    {
        PlaySound(fire);
    }

    // Phát âm thanh va chạm/đòn bắn đã gán trong Inspector.
    public void Block()
    {
        PlaySound(hit);
    }
}