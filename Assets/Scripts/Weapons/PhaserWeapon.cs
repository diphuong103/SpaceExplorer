using UnityEngine;

public class PhaserWeapon : MonoBehaviour
{
    public static PhaserWeapon Instance { get; private set; }

    [SerializeField] private PhaserBullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float speed = 10f;
    [SerializeField, Min(1)] private int damage = 1;

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

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        PhaserBullet bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
        bullet.Initialize(speed, damage);
        bullet.gameObject.SetActive(true);
        return true;
    }
}
