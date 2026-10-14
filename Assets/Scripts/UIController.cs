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
        if (maxEnergy <= 0f)
        {
            return;
        }

        float clampedEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        if (energySlider != null)
        {
            energySlider.minValue = 0f;
            energySlider.maxValue = 1f;
            energySlider.value = clampedEnergy / maxEnergy;
        }

        if (energyText != null)
        {
            energyText.text = $"{FormatResourceValue(clampedEnergy)}/{FormatResourceValue(maxEnergy)}";
        }
    }

    public void UpdateHealthText(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f)
        {
            return;
        }

        float clampedHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = clampedHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{FormatResourceValue(clampedHealth)}/{FormatResourceValue(maxHealth)}";
        }
    }

    private static string FormatResourceValue(float value)
    {
        return value.ToString("0.#");
    }
}
