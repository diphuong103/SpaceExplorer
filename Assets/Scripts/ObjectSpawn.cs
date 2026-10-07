using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawn : MonoBehaviour
{
    [SerializeField] private Transform minPos; // Vị trí spawn tối thiểu
    [SerializeField] private Transform maxPos; // Vị trí spawn tối đa
    [SerializeField] private Camera pointStarCamera;
    [SerializeField, Min(0f)] private float pointStarScreenPadding = 0.1f;
    [SerializeField] private PointStarSpawn[] pointStarSpawns;
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField, Min(0.1f)] private float powerUpSpawnInterval = 5f;
    [SerializeField, Min(1)] private int powerUpInitialSpawnCount = 1;
    [SerializeField, Min(0)] private int powerUpSpawnIncrease = 1;
    [SerializeField, Min(1)] private int powerUpMaxSpawnCount = 3;
    [SerializeField, Min(0.1f)] private float powerUpLifetime = 10f;

    [SerializeField] private int waveNumber = 0;
    [SerializeField] private List<Wave> waves; // Danh sách các wave
    [SerializeField] private GameObject whaleMiniPrefab;
    [SerializeField, Min(0.1f)] private float whaleMiniSpawnInterval = 10f;
    [SerializeField, Min(1)] private int whaleMiniInitialSpawnCount = 1;
    [SerializeField, Min(0)] private int whaleMiniSpawnIncrease = 1;
    [SerializeField, Min(1)] private int whaleMiniMaxSpawnCount = 5;
    [SerializeField, Min(0f)] private float whaleMiniHorizontalSpawnRange = 4f;

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public GameObject[] prefabs; // Đổi thành mảng để chứa nhiều loại Prefabs (Whale1, Whale2,...)

        [HideInInspector] public float spawnTimer;
        public float spawnInterval = 2f; // Thời gian giữa các lần spawn

        public int objectPerWave = 10;
        [HideInInspector] public int spawnedObjectCount;
    }

    [System.Serializable]
    private class PointStarSpawn
    {
        public GameObject prefab;
        [Min(0.1f)] public float spawnInterval = 10f;
        [HideInInspector] public float spawnTimer;
    }

    private float whaleMiniSpawnTimer;
    private float powerUpSpawnTimer;
    private int currentWhaleMiniSpawnCount;
    private int currentPowerUpSpawnCount;
    private bool missingPickupCameraLogged;
    private readonly List<GameObject> activePowerUps = new List<GameObject>();

    private void Start()
    {
        currentWhaleMiniSpawnCount = Mathf.Clamp(
            whaleMiniInitialSpawnCount,
            1,
            Mathf.Max(1, whaleMiniMaxSpawnCount));
        currentPowerUpSpawnCount = Mathf.Clamp(
            powerUpInitialSpawnCount,
            1,
            Mathf.Max(1, powerUpMaxSpawnCount));
    }

    void Update()
    {
        UpdatePointStarSpawning();
        UpdatePowerUpSpawning();
        UpdateWhaleMiniSpawning();

        if (waves == null || waves.Count == 0) return;

        waveNumber = Mathf.Clamp(waveNumber, 0, waves.Count - 1);
        Wave currentWave = waves[waveNumber];

        float boost = PlayerController.Instance != null ? PlayerController.Instance.boost : 1f;
        currentWave.spawnTimer += Time.deltaTime * boost;

        if (currentWave.spawnTimer >= currentWave.spawnInterval)
        {
            currentWave.spawnTimer = 0f;
            SpawnObject();
        }

        // Kiểm tra xem wave hiện tại đã spawn đủ số lượng chưa
        if (currentWave.spawnedObjectCount >= currentWave.objectPerWave)
        {
            // Reset đếm số lượng của wave hiện tại về 0 cho lượt quay lại sau
            currentWave.spawnedObjectCount = 0;

            waveNumber++;
            if (waveNumber >= waves.Count)
            {
                waveNumber = 0; // Quay lại wave đầu tiên nếu đã hết danh sách
            }
        }
    }

    private void UpdatePointStarSpawning()
    {
        if (pointStarSpawns == null || pointStarSpawns.Length == 0)
        {
            return;
        }

        if (pointStarCamera == null)
        {
            pointStarCamera = Camera.main;
        }

        if (pointStarCamera == null)
        {
            if (!missingPickupCameraLogged)
            {
                Debug.LogError("Cannot spawn pickups: assign a camera or tag the game camera as MainCamera.", this);
                missingPickupCameraLogged = true;
            }

            return;
        }

        foreach (PointStarSpawn starSpawn in pointStarSpawns)
        {
            if (starSpawn == null || starSpawn.prefab == null)
            {
                continue;
            }

            starSpawn.spawnTimer += Time.deltaTime;
            float interval = Mathf.Max(0.1f, starSpawn.spawnInterval);
            if (starSpawn.spawnTimer < interval)
            {
                continue;
            }

            starSpawn.spawnTimer -= interval;
            Instantiate(starSpawn.prefab, RandomScreenPosition(starSpawn.prefab), Quaternion.identity);
        }
    }

    private void UpdatePowerUpSpawning()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            return;
        }

        if (pointStarCamera == null)
        {
            pointStarCamera = Camera.main;
        }

        if (pointStarCamera == null)
        {
            if (!missingPickupCameraLogged)
            {
                Debug.LogError("Cannot spawn pickups: assign a camera or tag the game camera as MainCamera.", this);
                missingPickupCameraLogged = true;
            }

            return;
        }

        powerUpSpawnTimer += Time.deltaTime;
        if (powerUpSpawnTimer < Mathf.Max(0.1f, powerUpSpawnInterval))
        {
            return;
        }

        powerUpSpawnTimer = 0f;
        activePowerUps.RemoveAll(powerUp => powerUp == null);

        int availableSlots = Mathf.Max(0, powerUpMaxSpawnCount - activePowerUps.Count);
        int spawnCount = Mathf.Min(currentPowerUpSpawnCount, availableSlots);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            if (prefab != null)
            {
                GameObject powerUp = Instantiate(prefab, RandomScreenPosition(prefab), Quaternion.identity);
                activePowerUps.Add(powerUp);
                Destroy(powerUp, powerUpLifetime);
            }
        }

        if (spawnCount > 0)
        {
            currentPowerUpSpawnCount = Mathf.Min(
                Mathf.Max(1, powerUpMaxSpawnCount),
                currentPowerUpSpawnCount + Mathf.Max(0, powerUpSpawnIncrease));
        }
    }

    private Vector3 RandomScreenPosition(GameObject prefab)
    {
        float spawnZ = transform.position.z;
        float cameraDepth = pointStarCamera.WorldToViewportPoint(
            new Vector3(pointStarCamera.transform.position.x, pointStarCamera.transform.position.y, spawnZ)).z;

        Vector3 bottomLeft = pointStarCamera.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDepth));
        Vector3 topRight = pointStarCamera.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDepth));

        Renderer prefabRenderer = prefab.GetComponentInChildren<Renderer>();
        Vector3 spriteExtents = prefabRenderer != null ? prefabRenderer.bounds.extents : Vector3.zero;
        float horizontalPadding = spriteExtents.x + pointStarScreenPadding;
        float verticalPadding = spriteExtents.y + pointStarScreenPadding;

        float minX = bottomLeft.x + horizontalPadding;
        float maxX = topRight.x - horizontalPadding;
        float minY = bottomLeft.y + verticalPadding;
        float maxY = topRight.y - verticalPadding;

        if (minX > maxX)
        {
            minX = maxX = (bottomLeft.x + topRight.x) * 0.5f;
        }

        if (minY > maxY)
        {
            minY = maxY = (bottomLeft.y + topRight.y) * 0.5f;
        }

        return new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), spawnZ);
    }

    void SpawnObject()
    {
        Wave currentWave = waves[waveNumber];

        if (currentWave.prefabs == null || currentWave.prefabs.Length == 0)
            return;

        // Chọn ngẫu nhiên 1 prefab trong mảng prefabs
        int randomIndex = Random.Range(0, currentWave.prefabs.Length);
        GameObject selectedPrefab = currentWave.prefabs[randomIndex];

        if (selectedPrefab != null)
        {
            Instantiate(selectedPrefab, RandomSpawnPoint(), Quaternion.identity);
            currentWave.spawnedObjectCount++;
        }
    }

    private void UpdateWhaleMiniSpawning()
    {
        if (whaleMiniPrefab == null)
        {
            return;
        }

        float interval = Mathf.Max(0.1f, whaleMiniSpawnInterval);
        whaleMiniSpawnTimer += Time.deltaTime;
        if (whaleMiniSpawnTimer < interval)
        {
            return;
        }

        whaleMiniSpawnTimer -= interval;

        for (int i = 0; i < currentWhaleMiniSpawnCount; i++)
        {
            Instantiate(whaleMiniPrefab, RandomWhaleMiniSpawnPoint(), Quaternion.identity);
        }

        int maximumSpawnCount = Mathf.Max(1, whaleMiniMaxSpawnCount);
        currentWhaleMiniSpawnCount = Mathf.Min(
            maximumSpawnCount,
            currentWhaleMiniSpawnCount + Mathf.Max(0, whaleMiniSpawnIncrease));
    }

    private Vector2 RandomWhaleMiniSpawnPoint()
    {
        Vector2 spawnPoint = RandomSpawnPoint();
        float rightEdge = minPos != null && maxPos != null
            ? Mathf.Max(minPos.position.x, maxPos.position.x)
            : transform.position.x;
        spawnPoint.x = rightEdge + Random.Range(0f, whaleMiniHorizontalSpawnRange);
        return spawnPoint;
    }

    private Vector2 RandomSpawnPoint()
    {
        if (minPos == null || maxPos == null) return transform.position;

        Vector2 spawnPoint;
        spawnPoint.x = Random.Range(minPos.position.x, maxPos.position.x);
        spawnPoint.y = Random.Range(minPos.position.y, maxPos.position.y);

        return spawnPoint;
    }
}