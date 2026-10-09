using System;
using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
    [Serializable]
    private class BuffSetting
    {
        public PowerUpType type;
        [Min(0f)] public float duration = 8f;
        [Min(0f)] public float speedMultiplier = 1f;
        public GameObject visualEffect;
        public Vector3 visualEffectLocalPosition;
        public bool hideVisualWhileMouseBoosting;
    }

    [SerializeField] private BuffSetting[] buffs =
    {
        new BuffSetting { type = PowerUpType.WarpDrive, duration = 8f, speedMultiplier = 2f, hideVisualWhileMouseBoosting = true },
        new BuffSetting { type = PowerUpType.Energy },
        new BuffSetting { type = PowerUpType.Heal },
        new BuffSetting { type = PowerUpType.Shield },
        new BuffSetting { type = PowerUpType.Slow },
        new BuffSetting { type = PowerUpType.NovaSpread },
        new BuffSetting { type = PowerUpType.QuantumScore },
        new BuffSetting { type = PowerUpType.ViperHoming },
        new BuffSetting { type = PowerUpType.HyperionBeam },
        new BuffSetting { type = PowerUpType.Wipe }
    };

    private float[] remainingDurations;
    private GameObject[] runtimeVisualEffects;
    private bool[] visualEffectIsPrefab;
    private bool[] missingVisualWarningLogged;
    private PlayerController player;

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

    private void Awake()
    {
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
        runtimeVisualEffects = new GameObject[buffs.Length];
        visualEffectIsPrefab = new bool[buffs.Length];
        missingVisualWarningLogged = new bool[buffs.Length];

        for (int i = 0; i < buffs.Length; i++)
        {
            BuffSetting buff = buffs[i];
            if (buff.visualEffect != null)
            {
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
        if (buff.duration <= 0f)
        {
            Debug.LogWarning($"Buff duration for {type} must be greater than zero.", this);
            return;
        }

        remainingDurations[index] = buff.duration;
        UpdateVisual(index);
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
        }

        if (visualEffect == null)
        {
            if (shouldBeActive && !missingVisualWarningLogged[index])
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
    }
}
