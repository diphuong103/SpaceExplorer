using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float worlSpeed; // Tốc độ di chuyển của thế giới

        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool isPaused = false;

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

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Đóng băng thời gian trong game

        if (UIController.Instance != null && UIController.Instance.pausePanel != null)
        {
            UIController.Instance.pausePanel.SetActive(true); // Mở Menu Pause
            
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Cho thời gian chạy lại bình thường

        if (UIController.Instance != null && UIController.Instance.pausePanel != null)
        {
            UIController.Instance.pausePanel.SetActive(false); // Ẩn Menu Pause
        }
    }

}
