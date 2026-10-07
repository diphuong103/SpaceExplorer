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

    private void Awake()
    {
        if (Instance == null)
        {
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        UpdatePointsDisplay(GameManager.Instance != null ? GameManager.Instance.CurrentScore : 0);
    }

    public void UpdatePointsDisplay(int score)
    {
        if (PointsText != null)
        {
            PointsText.text = score.ToString();
        }
    }

    public void UpdateEnergySlider(float currentEnergy, float maxEnergy)
    {
        if (maxEnergy <= 0) return; // Tránh lỗi chia cho 0

        if (energySlider != null)
        {
            energySlider.value = currentEnergy / maxEnergy;
        }

        if (energyText != null)
        {
            // Làm tròn số nguyên (Mathf.CeilToInt) để chữ không bị hiển thị số thập phân dài ngoẵng
            energyText.text = $"{Mathf.CeilToInt(currentEnergy)}/{Mathf.CeilToInt(maxEnergy)}";
        }
    }

    public void UpdateHealthText(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0) return; // Tránh lỗi chia cho 0

        if (healthText != null)
        {
            healthSlider.value = Mathf.RoundToInt(currentHealth);
            healthSlider.maxValue = Mathf.RoundToInt(maxHealth);
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }
}
