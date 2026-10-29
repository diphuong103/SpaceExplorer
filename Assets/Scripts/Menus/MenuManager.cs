using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text gameOverScoreText;

    // Hiển thị điểm của lượt chơi vừa kết thúc trong scene Game Over.
    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "GameOver")
        {
            return;
        }

        // Dùng tham chiếu Inspector nếu đã gán; nếu chưa, tìm TMP object theo tên trong Canvas.
        if (gameOverScoreText == null)
        {
            GameObject scoreObject = GameObject.Find("ScoreOver");
            if (scoreObject != null)
            {
                gameOverScoreText = scoreObject.GetComponent<TMP_Text>();
            }
        }

        if (gameOverScoreText == null)
        {
            Debug.LogError("Game Over score text is missing. Assign a TMP_Text or name the object 'ScoreOver'.", this);
            return;
        }

        gameOverScoreText.text = GameManager.LastRunScore.ToString();
    }

    // Khôi phục thời gian và tải scene gameplay khi người chơi bắt đầu lượt mới.
    public void NewGame()
    {
        // Nút Play gọi hàm này qua Button.OnClick trong Inspector để mở scene gameplay.
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level1");
    }

    // Yêu cầu Unity đóng ứng dụng khi chạy bản build.
    public void QuitGame()
    {
        // Application.Quit chỉ đóng ứng dụng ở bản build; trong Unity Editor lệnh này không đóng Editor.
        Application.Quit();
    }
}
