using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text PointsText;
    public GameObject pausePanel;

    // Khởi tạo singleton HUD và ẩn bảng tạm dừng khi bắt đầu.
    private void Awake()
    {
        if (Instance == null)
        {
            // Gameplay component gửi dữ liệu về HUD thông qua singleton của scene.
            Instance = this;
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Xóa singleton khi instance HUD hiện tại bị hủy.
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Đồng bộ số điểm trên HUD với điểm của lượt chơi hiện tại.
    private void Start()
    {
        UpdatePointsDisplay(GameManager.Instance != null ? GameManager.Instance.CurrentScore : 0);
    }

    // Hiển thị tổng điểm hiện tại lên văn bản HUD.
    public void UpdatePointsDisplay(int score)
    {
        if (PointsText != null)
        {
            PointsText.text = score.ToString();
        }
    }

    // Cập nhật thanh và nhãn năng lượng theo giá trị hiện tại/tối đa.
    public void UpdateEnergySlider(float currentEnergy, float maxEnergy)
    {
        if (maxEnergy <= 0f)
        {
            return;
        }

        float clampedEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        if (energySlider != null)
        {
            // Slider năng lượng dùng tỷ lệ 0..1 để không phụ thuộc vào maxEnergy.
            energySlider.minValue = 0f;
            energySlider.maxValue = 1f;
            energySlider.value = clampedEnergy / maxEnergy;
        }

        if (energyText != null)
        {
            energyText.text = $"{FormatResourceValue(clampedEnergy)}/{FormatResourceValue(maxEnergy)}";
        }
    }

    // Cập nhật thanh máu và nhãn máu theo giá trị hiện tại/tối đa.
    public void UpdateHealthText(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f)
        {
            return;
        }

        float clampedHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthSlider != null)
        {
            // Slider máu dùng giá trị thực; nhãn hiển thị current/max tương ứng.
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = clampedHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{FormatResourceValue(clampedHealth)}/{FormatResourceValue(maxHealth)}";
        }
    }

    // Định dạng giá trị tài nguyên, bỏ phần thập phân không cần thiết.
    private static string FormatResourceValue(float value)
    {
        return value.ToString("0.#");
    }
}
