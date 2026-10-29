using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static int LastRunScore { get; private set; }

    public float worldSpeed = 1f; // Tốc độ di chuyển của thế giới
    [SerializeField, Min(1)] private int stageTwoScoreThreshold = 50;
    [SerializeField, Min(0f)] private float stageTransitionDuration = 2.5f;
    [SerializeField, Min(0.1f)] private float stageTwoRushDuration = 15f;
    [SerializeField, Min(1f), Tooltip("Hệ số tăng máu quái và tiểu hành tinh khi vào Level 2.")]
    private float stageTwoEnemyHealthMultiplier = 1.5f;
    [SerializeField, Min(1f), Tooltip("Hệ số tăng sát thương va chạm và đòn đánh của quái ở Level 2.")]
    private float stageTwoEnemyDamageMultiplier = 1.5f;
    [SerializeField] private TMP_FontAsset stageTransitionFont;
    [SerializeField] private GameObject stageOneBackgroundRoot;
    [SerializeField] private Sprite stageTwoBackground;
    [SerializeField] private Sprite stageTwoLeftStation;
    [SerializeField] private Sprite stageTwoRightStation;
    [SerializeField] private ObjectSpawn objectSpawner;
    public int CurrentScore => currentScore;
    public bool IsStageTwoActive => hasEnteredStageTwo;
    public float StageTwoEnemyHealthMultiplier => hasEnteredStageTwo ? stageTwoEnemyHealthMultiplier : 1f;
    public float StageTwoEnemyDamageMultiplier => hasEnteredStageTwo ? stageTwoEnemyDamageMultiplier : 1f;

    private int currentScore;
    private bool hasEnteredStageTwo;
    private GameObject stageTransitionCanvas;

    // Đăng ký singleton và khởi tạo điểm khi GameManager đầu tiên được tạo.
    private void Awake()
    {
        if (Instance == null)
        {
            // Các component khác truy cập trạng thái điểm và scene qua singleton này.
            Instance = this;
            currentScore = 0;
            LastRunScore = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Xóa singleton nếu instance hiện tại bị hủy.
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Cộng điểm sau khi áp dụng buff, cập nhật HUD và kiểm tra mốc Stage 2.
    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // Điểm có thể đến từ sao/orb hoặc phá quái; buff điểm áp dụng tập trung tại đây.
        int scoreMultiplier = PlayerController.Instance != null
            ? PlayerController.Instance.ScoreMultiplier
            : 1;
        currentScore += amount * Mathf.Max(1, scoreMultiplier);
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdatePointsDisplay(currentScore);
        }

        if (!hasEnteredStageTwo && currentScore >= stageTwoScoreThreshold)
        {
            StartCoroutine(EnterStageTwoRoutine());
        }
    }

    // Tạm dừng spawn, chuyển nền sang Stage 2 rồi bắt đầu đợt cao trào.
    private IEnumerator EnterStageTwoRoutine()
    {
        if (stageOneBackgroundRoot == null)
        {
            Debug.LogError("Cannot enter Stage 2: assign the Stage 1 background root in the GameManager Inspector.", this);
            yield break;
        }

        if (stageTwoBackground == null)
        {
            Debug.LogError("Cannot enter Stage 2: assign its background sprite in the GameManager Inspector.", this);
            yield break;
        }

        if (stageTwoLeftStation == null || stageTwoRightStation == null)
        {
            Debug.LogError("Cannot enter Stage 2: assign both station sprites in the GameManager Inspector.", this);
            yield break;
        }

        if (objectSpawner == null)
        {
            Debug.LogError("Cannot enter Stage 2: assign the ObjectSpawn component in the GameManager Inspector.", this);
            yield break;
        }

        Camera gameCamera = Camera.main;
        if (gameCamera == null || !gameCamera.orthographic)
        {
            Debug.LogError("Cannot enter Stage 2: an orthographic MainCamera is required to fit the background.", this);
            yield break;
        }

        // Tạm dừng spawn để người chơi nhận biết thay đổi bối cảnh trước đợt cao trào.
        objectSpawner.SetStageTransitionPaused(true);
        hasEnteredStageTwo = true;
        ApplyStageTwoStatsToActiveEnemies();

        // Tắt parallax Level 1 rồi dựng ba lớp Level 2 theo camera và các sprite đã gán trong Inspector.
        stageOneBackgroundRoot.SetActive(false);
        GameObject parallaxObject = new GameObject("Stage 2 Parallax");
        StageTwoParallax parallax = parallaxObject.AddComponent<StageTwoParallax>();
        if (!parallax.Initialize(
                gameCamera,
                stageTwoBackground,
                stageTwoLeftStation,
                stageTwoRightStation,
                worldSpeed))
        {
            Destroy(parallaxObject);
            stageOneBackgroundRoot.SetActive(true);
            objectSpawner.SetStageTransitionPaused(false);
            hasEnteredStageTwo = false;
            yield break;
        }

        ShowStageTransitionBanner();
        yield return new WaitForSeconds(stageTransitionDuration);

        objectSpawner.SetStageTransitionPaused(false);
        objectSpawner.BeginStageTwoRush(stageTwoRushDuration);

        if (stageTransitionCanvas != null)
        {
            Destroy(stageTransitionCanvas);
        }
    }

    // Áp dụng hệ số máu/sát thương Stage 2 cho các kẻ địch đang tồn tại.
    private void ApplyStageTwoStatsToActiveEnemies()
    {
        // Kẻ địch đã tồn tại lúc chạm mốc cũng nhận cùng hệ số với các đợt spawn mới.
        foreach (Asteroid asteroid in FindObjectsByType<Asteroid>(FindObjectsSortMode.None))
        {
            asteroid.ApplyStageTwoHealthMultiplier(stageTwoEnemyHealthMultiplier);
        }

        foreach (Whale whale in FindObjectsByType<Whale>(FindObjectsSortMode.None))
        {
            whale.ApplyStageTwoHealthMultiplier(stageTwoEnemyHealthMultiplier);
        }

        foreach (WhaleMini whaleMini in FindObjectsByType<WhaleMini>(FindObjectsSortMode.None))
        {
            whaleMini.ApplyStageTwoMultipliers(
                stageTwoEnemyHealthMultiplier,
                stageTwoEnemyDamageMultiplier);
        }

        foreach (SoundRing soundRing in FindObjectsByType<SoundRing>(FindObjectsSortMode.None))
        {
            soundRing.ApplyStageTwoDamageMultiplier(stageTwoEnemyDamageMultiplier);
        }
    }

    // Tạo Canvas và tiêu đề thông báo chuyển sang Stage 2.
    private void ShowStageTransitionBanner()
    {
        if (stageTransitionFont == null)
        {
            Debug.LogError("Cannot show the Stage 2 banner: assign its TMP font in the GameManager Inspector.", this);
            return;
        }

        // Dùng Canvas phủ màn hình để banner luôn nằm trên gameplay và nền Level 2.
        stageTransitionCanvas = new GameObject(
            "Stage Transition Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        Canvas canvas = stageTransitionCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = stageTransitionCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateStageTransitionLabel(
            "Stage Transition Title",
            "LEVEL UP",
            48f,
            new Vector2(0.25f, 0.56f),
            new Vector2(0.75f, 0.68f));
        CreateStageTransitionLabel(
            "Stage Transition Level",
            "LEVEL 2",
            80f,
            new Vector2(0.2f, 0.36f),
            new Vector2(0.8f, 0.58f));
    }

    // Tạo nhãn TextMeshPro căn theo vùng anchor đã chỉ định trên Canvas.
    private void CreateStageTransitionLabel(
        string objectName,
        string text,
        float fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject labelObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(stageTransitionCanvas.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = anchorMin;
        labelRect.anchorMax = anchorMax;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = stageTransitionFont;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.outlineWidth = 0.2f;
        label.outlineColor = Color.black;
        label.raycastTarget = false;
    }

    private bool isPaused = false;

    // Theo dõi phím Escape để chuyển đổi giữa trạng thái tạm dừng và tiếp tục.
    void Update()
    {
        // Nhấn nút ESC để Bật/Tắt Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Tạm dừng thời gian, phát âm thanh và hiển thị bảng Pause.
    public void PauseGame()
    {
        isPaused = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.pause);
        }
        // Dừng cả Update, physics và bộ đếm sử dụng deltaTime.
        Time.timeScale = 0f;

        if (UIController.Instance != null && UIController.Instance.pausePanel != null)
        {
            UIController.Instance.pausePanel.SetActive(true); // Mở Menu Pause

        }
    }

    // Tiếp tục thời gian chơi, phát âm thanh và ẩn bảng Pause.
    public void ResumeGame()
    {
        isPaused = false;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.resume);
        }
        Time.timeScale = 1f;

        if (UIController.Instance != null && UIController.Instance.pausePanel != null)
        {
            UIController.Instance.pausePanel.SetActive(false); // Ẩn Menu Pause
        }
    }

    // Khôi phục thời gian rồi tải scene menu chính.
    public void MainMenu()
    {
        isPaused = false;
        // Khôi phục tốc độ thời gian trước khi Unity nạp scene menu.
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Yêu cầu Unity đóng ứng dụng khi chạy bản build.
    public void QuitGame()
    {
        Application.Quit(); // Thoát game
    }

    // Lưu điểm cuối lượt, khôi phục thời gian và tải scene Game Over.
    public void GameOver()
    {
        isPaused = false;
        // Lưu điểm cuối vào static trước khi scene gameplay bị hủy khi chuyển cảnh.
        LastRunScore = currentScore;
        // Khôi phục tốc độ thời gian trước khi Unity nạp scene kết thúc.
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

}
