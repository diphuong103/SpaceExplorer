using System.Collections.Generic;
using UnityEngine;

public class PhaserWeapon : MonoBehaviour
{
    public static PhaserWeapon Instance { get; private set; }

    [SerializeField] private PhaserBullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float speed = 10f;
    [SerializeField, Min(1)] private int damage = 1;

    public Transform FirePoint => firePoint != null ? firePoint : transform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (bulletPrefab == null)
        {
            bulletPrefab = GetComponentInChildren<PhaserBullet>(true);
        }

        if (bulletPrefab != null && bulletPrefab.transform.IsChildOf(transform))
        {
            bulletPrefab.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Cannot shoot: assign a PhaserBullet prefab to PhaserWeapon or add one as a child.");
            return false;
        }

        SpawnBullet(GetShotDirection(0f));
        PlayShotSound();
        return true;
    }

    public bool ShootSpread(int bulletCount, float spreadAngle)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Cannot shoot spread: assign a PhaserBullet prefab to PhaserWeapon or add one as a child.");
            return false;
        }

        bulletCount = Mathf.Max(1, bulletCount);
        if (bulletCount == 1)
        {
            SpawnBullet(GetShotDirection(0f));
        }
        else
        {
            float halfAngle = Mathf.Max(0f, spreadAngle) * 0.5f;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)(bulletCount - 1));
                SpawnBullet(GetShotDirection(angle));
            }
        }

        PlayShotSound();
        return true;
    }

    public void DamageTargetsWithLaser(Vector2 direction, float range, int laserDamage)
    {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Vector2 origin = spawnPoint.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction.normalized, Mathf.Max(0f, range));
        HashSet<int> damagedTargets = new HashSet<int>();
        int damagePerHit = Mathf.Max(1, laserDamage);

        foreach (RaycastHit2D hit in hits)
        {
            Asteroid asteroid = hit.collider.GetComponentInParent<Asteroid>();
            if (asteroid != null && damagedTargets.Add(asteroid.GetInstanceID()))
            {
                asteroid.TakeDamage(damagePerHit);
                continue;
            }

            Whale whale = hit.collider.GetComponentInParent<Whale>();
            if (whale != null && damagedTargets.Add(whale.GetInstanceID()))
            {
                whale.TakeDamage(damagePerHit);
                continue;
            }

            WhaleMini whaleMini = hit.collider.GetComponentInParent<WhaleMini>();
            if (whaleMini != null && damagedTargets.Add(whaleMini.GetInstanceID()))
            {
                whaleMini.TakeDamage(damagePerHit);
            }
        }
    }

    public void ConfigureLaserVisual(GameObject laserVisual, float range)
    {
        if (laserVisual == null)
        {
            return;
        }

        Transform spawnPoint = FirePoint;
        Transform laserTransform = laserVisual.transform;
        if (laserTransform.parent != spawnPoint)
        {
            laserTransform.SetParent(spawnPoint, false);
        }

        SpriteRenderer laserRenderer = laserVisual.GetComponentInChildren<SpriteRenderer>();
        laserTransform.localRotation = Quaternion.identity;
        if (laserRenderer == null || laserRenderer.sprite == null)
        {
            laserTransform.localPosition = Vector3.zero;
            return;
        }

        Vector2 spriteSize = laserRenderer.sprite.bounds.size;
        Vector2 spriteCenter = laserRenderer.sprite.bounds.center;
        Vector3 localScale = laserTransform.localScale;
        localScale.x = spriteSize.x > 0f ? Mathf.Max(0f, range) / spriteSize.x : 1f;
        laserTransform.localScale = localScale;
        laserTransform.localPosition = new Vector3(
            -laserRenderer.sprite.bounds.min.x * localScale.x,
            -spriteCenter.y * localScale.y,
            0f);
    }

    private void SpawnBullet(Vector2 direction)
    {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
        PhaserBullet bullet = Instantiate(bulletPrefab, spawnPoint.position, rotation);
        bullet.Initialize(speed, damage, direction);
        bullet.gameObject.SetActive(true);
    }

    private Vector2 GetShotDirection(float angle)
    {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        return Quaternion.Euler(0f, 0f, angle) * spawnPoint.right;
    }

    private void PlayShotSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.shoot, 1f);
        }
    }
}
