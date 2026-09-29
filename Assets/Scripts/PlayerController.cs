using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float boostMultiplier = 1.5f;

    [Header("Shooting Settings")]
    [Header("Energy Settings")]
    [SerializeField] private float energy;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyRegenerationRate = 10f;

    [SerializeField] private float health;
    [SerializeField] private float maxHealth;

    [SerializeField] private GameObject destroyEffectPrefab; // Prefab của hiệu ứng khi Player chết

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 playerDirection;
    private bool isBoosting = false;
    private bool isDead;

    public float boost => isBoosting ? boostMultiplier : 1f;
    private readonly int moveXHash = Animator.StringToHash("moveX");
    private readonly int moveYHash = Animator.StringToHash("moveY");
    private readonly int boostingHash = Animator.StringToHash("Boosting");

    public bool IsBoosting => isBoosting;

    private void Awake()
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
        HandleInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleEnergy();
        HandleHealth();
    }

    private void HandleInput()
    {
        float directionX = Input.GetAxisRaw("Horizontal");
        float directionY = Input.GetAxisRaw("Vertical");
        playerDirection = new Vector2(directionX, directionY).normalized;

        if (Input.GetMouseButtonDown(1))
        {
            isBoosting = !isBoosting;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = isBoosting ? moveSpeed * boostMultiplier : moveSpeed;
        rb.linearVelocity = playerDirection * currentSpeed;
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

    private void HandleHealth()
    {

    }

    private void UpdateUI()
    {
        // Kiểm tra UIController tồn tại trước khi gọi để tránh lỗi NullReferenceException
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

        bool shotFired = PhaserWeapon.Instance.Shoot();
        if (shotFired && AudioManager.Instance != null)
        {
            AudioManager.Instance.Block();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            TakeDamage(1); // Giảm 10 máu khi va chạm với chướng ngại vật
        }
    }

    private void TakeDamage(float damage)
    {
        if (isDead)
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