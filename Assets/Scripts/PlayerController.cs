using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float boostMultiplier = 1.5f;

    [Header("Energy Settings")]
    [SerializeField] private float energy;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyRegenerationRate = 10f;

    [SerializeField] private float health;
    [SerializeField] private float maxHealth;

    [Header("Sound Wave Hit")]
    [SerializeField, Min(0f)] private float soundWaveKnockbackSpeed = 6f;
    [SerializeField, Min(0f)] private float soundWaveKnockbackDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float soundWaveFlashDuration = 0.2f;
    [SerializeField] private Color soundWaveFlashColor = new Color(0.5f, 1f, 1f, 1f);

    [SerializeField] private GameObject destroyEffectPrefab; // Prefab của hiệu ứng khi Player chết

    [SerializeField] private ParticleSystem engineEffect; // Prefab của hiệu ứng động cơ

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 playerDirection;
    private bool isBoosting = false;
    private bool isDead;
    private SpriteRenderer spriteRenderer;
    private Color originalSpriteColor;
    private Coroutine hitFlashCoroutine;
    private Vector2 soundWaveKnockbackVelocity;
    private float soundWaveKnockbackRemaining;
    private PlayerBuffManager buffManager;

    public float boost => isBoosting ? boostMultiplier : 1f;
    private readonly int moveXHash = Animator.StringToHash("moveX");
    private readonly int moveYHash = Animator.StringToHash("moveY");
    private readonly int boostingHash = Animator.StringToHash("Boosting");

    public bool IsBoosting => isBoosting;
    public int ScoreMultiplier => buffManager != null ? buffManager.ScoreMultiplier : 1;

    private void Awake()
    {
        // Buff manager có thể được gắn trực tiếp lên Player hoặc một object con trong prefab.
        PlayerBuffManager[] buffManagers = GetComponentsInChildren<PlayerBuffManager>(true);
        for (int i = 0; i < buffManagers.Length; i++)
        {
            if (buffManagers[i].transform != transform)
            {
                buffManager = buffManagers[i];
                break;
            }
        }

        if (buffManager == null)
        {
            buffManager = GetComponent<PlayerBuffManager>();
        }

        if (buffManagers.Length > 1)
        {
            Debug.LogWarning("Multiple PlayerBuffManager components found. Using the manager on a child object when available.", this);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }

        if (Instance == null)
        {
            Instance = this;
            if (engineEffect != null)
            {
                ParticleSystem.MainModule engineMain = engineEffect.main;
                engineMain.loop = true;
                engineEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        energy = maxEnergy;
        health = maxHealth;

        UpdateUI();
    }

    private void Update()
    {
        // Đọc input và cập nhật Animator trong Update; xử lý vật lý riêng ở FixedUpdate.
        HandleInput();
        UpdateWarpDriveEffect();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleEnergy();
    }

    private void HandleInput()
    {
        // Horizontal/Vertical lấy cấu hình Input Manager mặc định, tương ứng phím mũi tên hoặc WASD.
        float directionX = Input.GetAxisRaw("Horizontal");
        float directionY = Input.GetAxisRaw("Vertical");
        playerDirection = new Vector2(directionX, directionY).normalized;

        // Chuột phải bật/tắt tăng tốc; năng lượng bị tiêu hao trong FixedUpdate khi trạng thái này bật.
        if (Input.GetMouseButtonDown(1))
        {
            isBoosting = !isBoosting;
        }

        // Chuột trái gọi PhaserWeapon để tạo đạn laser tại FirePoint được gán trong Inspector.
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void HandleMovement()
    {
        float buffMultiplier = buffManager != null ? buffManager.SpeedMultiplier : 1f;
        float currentSpeed = moveSpeed * boost * buffMultiplier;
        Vector2 knockbackVelocity = Vector2.zero;

        if (soundWaveKnockbackRemaining > 0f && soundWaveKnockbackDuration > 0f)
        {
            float knockbackStrength = soundWaveKnockbackRemaining / soundWaveKnockbackDuration;
            knockbackVelocity = soundWaveKnockbackVelocity * knockbackStrength;
            soundWaveKnockbackRemaining = Mathf.Max(0f, soundWaveKnockbackRemaining - Time.fixedDeltaTime);
        }

        // Gán vận tốc Rigidbody2D trong nhịp vật lý để chuyển động ổn định theo Unity.
        rb.linearVelocity = playerDirection * currentSpeed + knockbackVelocity;
    }

    private void UpdateWarpDriveEffect()
    {
        bool shouldPlay = buffManager != null
            && buffManager.IsActive(PowerUpType.WarpDrive)
            && !isBoosting;

        if (engineEffect == null)
        {
            return;
        }

        if (shouldPlay && !engineEffect.isPlaying)
        {
            engineEffect.Play(true);
        }
        else if (!shouldPlay && engineEffect.isPlaying)
        {
            engineEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void ActivatePowerUp(PowerUpType type)
    {
        if (buffManager == null)
        {
            Debug.LogError("Cannot activate power-up: PlayerBuffManager is missing from the Player.", this);
            return;
        }

        buffManager.Activate(type);
    }

    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f || isDead)
        {
            return;
        }

        energy = Mathf.Min(energy + amount, maxEnergy);
        UpdateUI();
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f || isDead)
        {
            return;
        }

        health = Mathf.Min(health + amount, maxHealth);
        UpdateUI();
    }

    public void ApplySoundWaveHit(Vector2 knockbackDirection, float damage = 0f)
    {
        if (isDead || (buffManager != null && buffManager.IsActive(PowerUpType.Shield)))
        {
            return;
        }

        soundWaveKnockbackVelocity = knockbackDirection.normalized * soundWaveKnockbackSpeed;
        soundWaveKnockbackRemaining = soundWaveKnockbackDuration;
        if (damage > 0f)
        {
            TakeDamage(damage);
        }

        if (isDead)
        {
            return;
        }

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
        }

        hitFlashCoroutine = StartCoroutine(SoundWaveHitFlashRoutine());
    }

    private IEnumerator SoundWaveHitFlashRoutine()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        spriteRenderer.color = soundWaveFlashColor;
        yield return new WaitForSeconds(soundWaveFlashDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }

        hitFlashCoroutine = null;
    }

    private void HandleEnergy()
    {
        if (isBoosting)
        {
            energy -= Time.fixedDeltaTime * 10f; // Trừ năng lượng khi tăng tốc
            if (energy <= 0f)
            {
                energy = 0f;
                isBoosting = false;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Fire();
                }
            }
        }
        else
        {
            energy += energyRegenerationRate * Time.fixedDeltaTime;  // Tăng năng lượng khi không tăng tốc
            if (energy > maxEnergy)
            {
                energy = maxEnergy;
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // UI có thể khởi tạo sau Player trong scene, nên kiểm tra singleton trước khi cập nhật.
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateEnergySlider(energy, maxEnergy);
            UIController.Instance.UpdateHealthText(health, maxHealth);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetFloat(moveXHash, playerDirection.x);
        animator.SetFloat(moveYHash, playerDirection.y);
        animator.SetBool(boostingHash, isBoosting);
    }

     private void Shoot()
    {
        if (PhaserWeapon.Instance == null)
        {
            Debug.LogError("Cannot shoot: PhaserWeapon is missing from the scene.");
            return;
        }

        bool shotFired = buffManager != null && buffManager.IsActive(PowerUpType.NovaSpread)
            ? PhaserWeapon.Instance.ShootSpread(buffManager.SpreadBulletCount, buffManager.SpreadAngle)
            : PhaserWeapon.Instance.Shoot();
        if (shotFired && AudioManager.Instance != null)
        {
            AudioManager.Instance.Block();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Tag Obstacles phải được gán cho collider của quái/chướng ngại vật trong prefab Unity.
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            float damage = GameManager.Instance != null
                ? GameManager.Instance.StageTwoEnemyDamageMultiplier
                : 1f;
            TakeDamage(damage); // Va chạm với chướng ngại vật gây sát thương cao hơn ở Level 2.
        }
    }

    private void TakeDamage(float damage)
    {
        if (isDead || (buffManager != null && buffManager.IsActive(PowerUpType.Shield)))
        {
            return;
        }

        health -= damage;
        if (health < 0f)
        {
            health = 0f;
        }

        UpdateUI();

        if (health <= 0f)
        {
            // Khi HP về 0, hiệu ứng chết được tạo trước khi GameManager chuyển sang scene GameOver.
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);

        Debug.Log("Player has died!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("Cannot show Game Over: GameManager.Instance is missing.");
        }
    }


}