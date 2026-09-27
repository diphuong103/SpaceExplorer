using UnityEngine;  

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;         
    [SerializeField] private float boostMultiplier = 1.5f; 

    [Header("Shooting Settings")]
    [SerializeField] private GameObject laserPrefab;       
    [SerializeField] private Transform firePoint;          

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
        if (laserPrefab == null) return;

        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
        Instantiate(laserPrefab, spawnPosition, Quaternion.identity);
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
        // Xử lý khi Player chết (ví dụ: hiển thị màn hình Game Over)
        if (destroyEffectPrefab != null)
        {
            gameObject.SetActive(false); // Ẩn Player trước khi tạo hiệu ứng
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }   

        Debug.Log("Player has died!");
        // Có thể thêm logic để reset game hoặc load lại scene
    }
}