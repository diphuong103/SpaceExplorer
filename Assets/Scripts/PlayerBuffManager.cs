using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBuffManager : MonoBehaviour
{
    [Serializable]
    private class BuffSetting
    {
        [Tooltip("Chọn loại orb. Tên trong danh sách đã được nhóm theo chức năng.")]
        public PowerUpType type;
        [Tooltip("Thời lượng buff; với hồi máu/năng lượng, đây là thời gian hiển thị phản hồi trên HUD.")]
        [Min(0f)] public float duration = 8f;
        [Tooltip("Hệ số tốc độ di chuyển khi hiệu ứng đang hoạt động.")]
        [Min(0f)] public float speedMultiplier = 1f;
        [Tooltip("Lượng năng lượng được hồi ngay khi nhặt orb.")]
        [Min(0f)] public float energyRestoreAmount;
        [Tooltip("Bán kính gây sát thương xung quanh Player khi nhặt orb năng lượng.")]
        [Min(0.1f)] public float energyPulseRadius = 2.5f;
        [Tooltip("Sát thương gây lên mỗi mục tiêu trong phạm vi khi nhặt orb năng lượng.")]
        [Min(1)] public int energyPulseDamage = 1;
        [Tooltip("Lượng máu được hồi ngay khi nhặt orb.")]
        [Min(0f)] public float healthRestoreAmount;
        [Tooltip("Hệ số nhân điểm khi Quantum Score đang hoạt động.")]
        [Min(1)] public int scoreMultiplier = 2;
        [Tooltip("Số viên đạn mỗi lần bắn khi hiệu ứng bắn chùm đang hoạt động.")]
        [Min(1)] public int spreadBulletCount = 3;
        [Tooltip("Tổng góc tỏa ra của loạt đạn chùm.")]
        [Range(0f, 180f)] public float spreadAngle = 30f;
        [Tooltip("Sát thương mỗi lần laser chạm mục tiêu.")]
        [Min(1)] public int laserDamage = 1;
        [Tooltip("Khoảng thời gian giữa các lần gây sát thương của laser.")]
        [Min(0.01f)] public float laserDamageInterval = 0.5f;
        [Tooltip("Chiều dài tia laser theo đơn vị trong game.")]
        [Min(0f)] public float laserRange = 16f;
        [Tooltip("Prefab hoặc GameObject hiệu ứng hình ảnh.")]
        public GameObject visualEffect = null;
        [Tooltip("Icon hiển thị trên Canvas của Player khi hiệu ứng đang hoạt động.")]
        public Sprite icon = null;
        [Tooltip("Vị trí local của hiệu ứng so với Player.")]
        public Vector3 visualEffectLocalPosition = Vector3.zero;
        [Tooltip("Ẩn hiệu ứng hình ảnh này khi đang tăng tốc bằng chuột phải.")]
        public bool hideVisualWhileMouseBoosting;
    }

    [SerializeField] private BuffSetting[] buffs =
    {
        new BuffSetting { type = PowerUpType.WarpDrive, duration = 8f, speedMultiplier = 2f, hideVisualWhileMouseBoosting = true },
        new BuffSetting { type = PowerUpType.Energy, duration = 2f, energyRestoreAmount = 25f, energyPulseRadius = 2.5f, energyPulseDamage = 1 },
        new BuffSetting { type = PowerUpType.Heal, duration = 2f, healthRestoreAmount = 2f },
        new BuffSetting { type = PowerUpType.Shield },
        new BuffSetting { type = PowerUpType.Slow },
        new BuffSetting { type = PowerUpType.NovaSpread, spreadBulletCount = 3, spreadAngle = 30f },
        new BuffSetting { type = PowerUpType.QuantumScore, duration = 8f, scoreMultiplier = 2 },
        new BuffSetting { type = PowerUpType.ViperHoming },
        new BuffSetting { type = PowerUpType.HyperionBeam, duration = 8f, laserDamage = 1, laserDamageInterval = 0.5f, laserRange = 16f },
        new BuffSetting { type = PowerUpType.Wipe }
    };

    [Header("Buff HUD")]
    [SerializeField, Min(1f)] private float iconSize = 48f;
    [SerializeField, Min(0f)] private float iconSpacing = 8f;
    [SerializeField] private Vector2 screenMargin = new Vector2(24f, 24f);
    [SerializeField, Range(0f, 1f)] private float countdownOverlayOpacity = 0.7f;

    private float[] remainingDurations;
    private float[] attackTimers;
    private GameObject[] runtimeVisualEffects;
    private bool[] visualEffectIsPrefab;
    private bool[] missingVisualWarningLogged;
    private bool[] missingLaserWeaponWarningLogged;
    private BuffIndicator[] buffIndicators;
    private PlayerController player;

    private sealed class BuffIndicator
    {
        public RectTransform rectTransform;
        public Image countdownOverlay;
    }

    public float SpeedMultiplier
    {
        get
        {
            float multiplier = 1f;
            for (int i = 0; i < buffs.Length; i++)
            {
                if (remainingDurations[i] > 0f)
                {
                    multiplier *= Mathf.Max(0f, buffs[i].speedMultiplier);
                }
            }

            return multiplier;
        }
    }

    public int SpreadBulletCount => GetBuffSetting(PowerUpType.NovaSpread)?.spreadBulletCount ?? 3;
    public float SpreadAngle => GetBuffSetting(PowerUpType.NovaSpread)?.spreadAngle ?? 30f;
    public int ScoreMultiplier => IsActive(PowerUpType.QuantumScore)
        ? Mathf.Max(1, GetBuffSetting(PowerUpType.QuantumScore)?.scoreMultiplier ?? 2)
        : 1;

    private void Awake()
    {
        // Hỗ trợ đặt component này trực tiếp trên Player hoặc trên object con của prefab.
        player = GetComponentInParent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerBuffManager must be attached to the Player or one of its child objects.", this);
            enabled = false;
            return;
        }

        if (buffs == null)
        {
            Debug.LogError("PlayerBuffManager has no buff settings configured.", this);
            buffs = Array.Empty<BuffSetting>();
        }

        remainingDurations = new float[buffs.Length];
        attackTimers = new float[buffs.Length];
        runtimeVisualEffects = new GameObject[buffs.Length];
        visualEffectIsPrefab = new bool[buffs.Length];
        missingVisualWarningLogged = new bool[buffs.Length];
        missingLaserWeaponWarningLogged = new bool[buffs.Length];
        buffIndicators = new BuffIndicator[buffs.Length];

        for (int i = 0; i < buffs.Length; i++)
        {
            BuffSetting buff = buffs[i];
            if (buff.visualEffect != null)
            {
                // Prefab được Instantiate lúc chạy; object có sẵn trong scene phải là con Player.
                if (!buff.visualEffect.scene.IsValid())
                {
                    visualEffectIsPrefab[i] = true;
                }
                else if (buff.visualEffect == player.gameObject || buff.visualEffect == gameObject)
                {
                    Debug.LogError($"The visual effect for {buff.type} cannot reference the Player or PlayerBuffManager.", this);
                    buff.visualEffect = null;
                }
                else if (!buff.visualEffect.transform.IsChildOf(player.transform))
                {
                    Debug.LogError($"The visual effect for {buff.type} must be a child of the Player, or a prefab asset.", this);
                    buff.visualEffect = null;
                }
                else
                {
                    runtimeVisualEffects[i] = buff.visualEffect;
                    buff.visualEffect.SetActive(false);
                }
            }
        }

        CreateBuffCanvas();
    }

    private void Update()
    {
        for (int i = 0; i < buffs.Length; i++)
        {
            if (remainingDurations[i] > 0f)
            {
                remainingDurations[i] = Mathf.Max(0f, remainingDurations[i] - Time.deltaTime);
            }

            UpdateVisual(i);
            UpdateAttack(i);
            UpdateBuffIndicator(i);
        }
    }

    public void Activate(PowerUpType type)
    {
        if (type == PowerUpType.None)
        {
            return;
        }

        int index = FindBuffIndex(type);
        if (index < 0)
        {
            Debug.LogWarning($"No buff settings are configured for {type}.", this);
            return;
        }

        BuffSetting buff = buffs[index];
        switch (type)
        {
            case PowerUpType.HyperionBeam:
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLaserActivation();
                }
                break;
            case PowerUpType.Shield:
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayShieldActivation();
                }
                break;
            case PowerUpType.Energy:
                player.RestoreEnergy(buff.energyRestoreAmount);
                DamageNearbyTargetsWithEnergyPulse(buff);
                break;
            case PowerUpType.Heal:
                player.RestoreHealth(buff.healthRestoreAmount);
                break;
        }

        if (buff.duration > 0f)
        {
            remainingDurations[index] = buff.duration;
            attackTimers[index] = type == PowerUpType.HyperionBeam
                ? Mathf.Max(0.01f, buff.laserDamageInterval)
                : 0f;
            UpdateVisual(index);
        }
        else if (type == PowerUpType.Shield || type == PowerUpType.WarpDrive)
        {
            Debug.LogWarning($"Buff duration for {type} must be greater than zero.", this);
        }
    }

    private void DamageNearbyTargetsWithEnergyPulse(BuffSetting buff)
    {
        float radius = buff.energyPulseRadius > 0f ? buff.energyPulseRadius : 2.5f;
        int damage = Mathf.Max(1, buff.energyPulseDamage);
        // OverlapCircleAll truy vấn collider; HashSet tránh gây sát thương nhiều lần lên cùng mục tiêu.
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, radius);
        HashSet<int> damagedTargets = new HashSet<int>();

        foreach (Collider2D hit in hits)
        {
            Asteroid asteroid = hit.GetComponentInParent<Asteroid>();
            if (asteroid != null && damagedTargets.Add(asteroid.GetInstanceID()))
            {
                asteroid.TakeDamage(damage);
                continue;
            }

            Whale whale = hit.GetComponentInParent<Whale>();
            if (whale != null && damagedTargets.Add(whale.GetInstanceID()))
            {
                whale.TakeDamage(damage);
                continue;
            }

            WhaleMini whaleMini = hit.GetComponentInParent<WhaleMini>();
            if (whaleMini != null && damagedTargets.Add(whaleMini.GetInstanceID()))
            {
                whaleMini.TakeDamage(damage);
            }
        }
    }

    public bool IsActive(PowerUpType type)
    {
        int index = FindBuffIndex(type);
        return index >= 0 && remainingDurations[index] > 0f;
    }

    private int FindBuffIndex(PowerUpType type)
    {
        for (int i = 0; i < buffs.Length; i++)
        {
            if (buffs[i].type == type)
            {
                return i;
            }
        }

        return -1;
    }

    private BuffSetting GetBuffSetting(PowerUpType type)
    {
        int index = FindBuffIndex(type);
        return index >= 0 ? buffs[index] : null;
    }

    private void UpdateAttack(int index)
    {
        if (remainingDurations[index] <= 0f || buffs[index].type != PowerUpType.HyperionBeam)
        {
            return;
        }

        BuffSetting buff = buffs[index];
        attackTimers[index] += Time.deltaTime;
        if (attackTimers[index] < Mathf.Max(0.01f, buff.laserDamageInterval))
        {
            return;
        }

        attackTimers[index] = 0f;
        if (PhaserWeapon.Instance == null)
        {
            if (!missingLaserWeaponWarningLogged[index])
            {
                Debug.LogError("Cannot use laser buff: PhaserWeapon is missing from the scene.", this);
                missingLaserWeaponWarningLogged[index] = true;
            }

            return;
        }

        Transform origin = PhaserWeapon.Instance.FirePoint;
        Vector2 direction = origin != null ? (Vector2)origin.right : Vector2.right;
        PhaserWeapon.Instance.DamageTargetsWithLaser(direction, buff.laserRange, buff.laserDamage);
    }

    private void UpdateVisual(int index)
    {
        BuffSetting buff = buffs[index];
        bool shouldBeActive = remainingDurations[index] > 0f
            && !(buff.hideVisualWhileMouseBoosting && player != null && player.IsBoosting);

        GameObject visualEffect = runtimeVisualEffects[index];
        if (visualEffect == null && shouldBeActive && visualEffectIsPrefab[index])
        {
            visualEffect = Instantiate(buff.visualEffect, player.transform);
            visualEffect.transform.localPosition = buff.visualEffectLocalPosition;
            visualEffect.transform.localRotation = Quaternion.identity;
            runtimeVisualEffects[index] = visualEffect;

            if (buff.type == PowerUpType.HyperionBeam && PhaserWeapon.Instance != null)
            {
                PhaserWeapon.Instance.ConfigureLaserVisual(visualEffect, buff.laserRange);
            }
        }

        if (visualEffect == null)
        {
            if (shouldBeActive
                && RequiresAssignedVisualEffect(buff.type)
                && !missingVisualWarningLogged[index])
            {
                Debug.LogWarning($"No visual effect is assigned for {buff.type} on PlayerBuffManager.", this);
                missingVisualWarningLogged[index] = true;
            }

            return;
        }

        if (visualEffect.activeSelf != shouldBeActive)
        {
            visualEffect.SetActive(shouldBeActive);
        }

        if (shouldBeActive && buff.type == PowerUpType.HyperionBeam && PhaserWeapon.Instance != null)
        {
            PhaserWeapon.Instance.ConfigureLaserVisual(visualEffect, buff.laserRange);
        }
    }

    private static bool RequiresAssignedVisualEffect(PowerUpType type)
    {
        return type != PowerUpType.WarpDrive
            && type != PowerUpType.NovaSpread
            && type != PowerUpType.QuantumScore;
    }

    private void CreateBuffCanvas()
    {
        int iconCount = 0;
        for (int i = 0; i < buffs.Length; i++)
        {
            if (buffs[i].icon != null)
            {
                iconCount++;
            }
        }

        if (iconCount == 0)
        {
            return;
        }

        // Tạo HUD lúc chạy để icon buff không cần một GameObject riêng trong scene.
        GameObject canvasObject = new GameObject("Buff Effect Canvas", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = Vector2.zero;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        for (int i = 0; i < buffs.Length; i++)
        {
            if (buffs[i].icon == null)
            {
                continue;
            }

            GameObject iconObject = new GameObject(
                $"Buff Icon - {buffs[i].type}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            iconObject.transform.SetParent(canvasObject.transform, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.zero;
            iconRect.pivot = Vector2.zero;

            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = buffs[i].icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            GameObject overlayObject = new GameObject(
                "Countdown Overlay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            overlayObject.transform.SetParent(iconObject.transform, false);

            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlay = overlayObject.GetComponent<Image>();
            overlay.sprite = buffs[i].icon;
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Radial360;
            overlay.fillOrigin = (int)Image.Origin360.Top;
            overlay.fillClockwise = false;
            overlay.color = new Color(0f, 0f, 0f, countdownOverlayOpacity);
            overlay.preserveAspect = true;
            overlay.raycastTarget = false;

            buffIndicators[i] = new BuffIndicator
            {
                rectTransform = iconRect,
                countdownOverlay = overlay
            };
        }
    }

    private void UpdateBuffIndicator(int index)
    {
        BuffIndicator indicator = buffIndicators[index];
        if (indicator == null)
        {
            if (remainingDurations[index] > 0f && !missingVisualWarningLogged[index])
            {
                Debug.LogWarning($"No HUD icon is assigned for {buffs[index].type} on PlayerBuffManager.", this);
                missingVisualWarningLogged[index] = true;
            }

            return;
        }

        bool isActive = remainingDurations[index] > 0f;
        GameObject iconObject = indicator.rectTransform.gameObject;
        if (iconObject.activeSelf != isActive)
        {
            iconObject.SetActive(isActive);
        }

        if (isActive)
        {
            indicator.countdownOverlay.fillAmount = Mathf.Clamp01(
                remainingDurations[index] / buffs[index].duration);

            int activeIconIndex = 0;
            for (int i = 0; i < buffIndicators.Length; i++)
            {
                if (buffIndicators[i] == null || remainingDurations[i] <= 0f)
                {
                    continue;
                }

                float offset = activeIconIndex * (iconSize + iconSpacing);
                buffIndicators[i].rectTransform.anchoredPosition = new Vector2(offset + screenMargin.x, screenMargin.y);
                activeIconIndex++;
            }
        }
    }
}
