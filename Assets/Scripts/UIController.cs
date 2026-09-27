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
    public GameObject pausePanel;

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
       