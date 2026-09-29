using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float worldSpeed = 1f; // Tốc độ di chuyển của thế giới


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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.pause);
        }
        Time.timeScale = 0f; // Đóng băng thời gian trong game

        if (UIController.Instance != null && UIController.Instance.pausePanel != null)
        {
            UIController.Instance.pausePanel.SetActive(true); // Mở Menu Pause

        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.resume);
        }
        Time.timeScale = 1f; // Cho thời gian chạy lại bình thường

        if (UIController.Instance != null && UIController.Instance.pausePanel != null)
        {
            UIController.Instance.pausePanel.SetActive(false); // Ẩn Menu Pause
        }
    }

    public void MainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit(); // Thoát game
    }

    public void GameOver()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

}
