using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;         // Tốc độ di chuyển cơ bản
    [SerializeField] private float boostMultiplier = 1.5f; // Hệ số nhân khi tăng tốc

    [Header("Shooting Settings")]
    [SerializeField] private GameObject laserPrefab;       // Prefab đạn Laser
    [SerializeField] private Transform firePoint;          // Vị trí nòng đạn

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 playerDirection;
    private bool isBoosting = false;

    // Tối ưu hiệu năng Animator bằng ID Hash thay vì truyền chuỗi String
    private readonly int moveXHash = Animator.StringToHash("moveX");
    private readonly int moveYHash = Animator.StringToHash("moveY");
    private readonly int boostingHash = Animator.StringToHash("Boosting");

    void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        // 1. Di chuyển bằng phím A, W, S, D
        float directionX = Input.GetAxisRaw("Horizontal");
        float directionY = Input.GetAxisRaw("Vertical");
        playerDirection = new Vector2(directionX, directionY).normalized;

        // 2. Click Chuột Phải (Nút 1) để Bật/Tắt (Toggle) Tăng tốc
        if (Input.GetMouseButtonDown(1))
        {
            isBoosting = !isBoosting;
        }

        // 3. Click Chuột Trái (Nút 0) để Bắn đạn Laser
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Cập nhật các thông số Animator khớp chuẩn với Parameter trên Inspector
        animator.SetFloat(moveXHash, playerDirection.x);
        animator.SetFloat(moveYHash, playerDirection.y);
        animator.SetBool(boostingHash, isBoosting);
    }

    private void HandleMovement()
    {
        // Tính tốc độ vật lý
        float currentSpeed = isBoosting ? moveSpeed * boostMultiplier : moveSpeed;
        rb.linearVelocity = playerDirection * currentSpeed;
    }

    private void Shoot()
    {
        if (laserPrefab == null) return;

        // Tự động dùng vị trí Player nếu chưa gắn FirePoint trên Inspector
        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
        Instantiate(laserPrefab, spawnPosition, Quaternion.identity);
    }
}