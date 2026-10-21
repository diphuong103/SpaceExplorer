using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawn : MonoBehaviour
{
    // Các tham chiếu này được gán trong Inspector để xác định vùng spawn và camera của màn chơi.
    [SerializeField] private Transform minPos; // Vị trí spawn tối thiểu
    [SerializeField] private Transform maxPos; // Vị trí spawn tối đa
    [SerializeField] private Camera pointStarCamera;
    [SerializeField, Min(0f)] private float pointStarScreenPadding = 0.1f;
    [SerializeField, Min(1)] private int maxActivePointStars = 4;
    [SerializeField, Min(1f)] private float difficultyRampDuration = 120f;
    [SerializeField] private PointStarSpawn[] pointStarSpawns;
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField, Min(0.1f)] private float powerUpSpawnInterval = 15f;
    [SerializeField, Min(1)] private int powerUpInitialSpawnCount = 2;
    [SerializeField, Min(0)] private int powerUpSpawnIncrease = 1;
    [SerializeField, Min(1)] private int powerUpMaxSpawnCount = 3;
    [SerializeField, Min(0.1f)] private float powerUpLifetime = 10f;
    [SerializeField, Range(0.1f, 1f)] private float stageTwoRushPowerUpIntervalMultiplier = 0.5f;

    [SerializeField] private int waveNumber = 0;
    [SerializeField] private List<Wave> waves; // Danh sách các wave
    [SerializeField] private GameObject whaleMiniPrefab;
    [SerializeField, Min(0.1f)] private float whaleMiniSpawnInterval = 10f;
    [SerializeField, Min(1)] private int whaleMiniInitialSpawnCount = 1;
    [SerializeField, Min(0)] private int whaleMiniSpawnIncrease = 1;
    [SerializeField, Min(1)] private int whaleMiniMaxSpawnCount = 3;
    [SerializeField, Min(1)] private int whaleMiniMaxActiveCount = 6;
    [SerializeField, Min(0f)] private float whaleMiniHorizontalSpawnRange = 4f;
    [SerializeField, Range(0.1f, 1f), Tooltip("Hệ số thời gian giữa các lần spawn ở Level 2; nhỏ hơn sẽ spawn dày hơn.")]
    private float stageTwoSpawnIntervalMultiplier = 0.8f;
    [SerializeField, Range(0.1f, 1f)] private float stageTwoRushSpawnIntervalMultiplier = 0.5f;

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
        public GameObject prefab = null;
        [Min(0.1f)] public float spawnInterval = 10f;
        [HideInInspector] public float spawnTimer;
    }

    private float whaleMiniSpawnTimer;
    private float powerUpSpawnTimer;
    private float difficultyElapsedTime;
    private float stageTwoRushRemaining;
    private int currentPowerUpSpawnCount;
    private bool isStageTransitionPaused;
    private bool missingPickupCameraLogged;
    private bool invalidPowerUpPrefabLogged;
    private readonly List<GameObject> activePointStars = new List<GameObject>();
    private readonly List<GameObject> activePowerUps = new List<GameObject>();
    private readonly List<GameObject> activeWhaleMinis = new List<GameObject>();

    private void Start()
    {
        // Chuẩn hóa số orb ban đầu theo giới hạn cấu hình trong Inspector.
        currentPowerUpSpawnCount = Mathf.Clamp(
            powerUpInitialSpawnCount,
            1,
            Mathf.Max(1, powerUpMaxSpawnCount));

        if (powerUpPrefabs == null)
        {
            return;
        }

        foreach (GameObject prefab in powerUpPrefabs)
        {
            if (prefab != null && prefab.GetComponentInChildren<PowerUpOrb>(true) == null)
            {
                Debug.LogError($"Power-up prefab '{prefab.name}' does not contain a PowerUpOrb component.", prefab);
                invalidPowerUpPrefabLogged = true;
            }
        }
    }

    void Update()
    {
        if (isStageTransitionPaused)
        {
            return;
        }

        // Tiến độ khó dùng Time.deltaTime nên sẽ tạm dừng đồng bộ với Time.timeScale của Unity.
        difficultyElapsedTime += Time.deltaTime;
        stageTwoRushRemaining = Mathf.Max(0f, stageTwoRushRemaining - Time.deltaTime);
        UpdatePointStarSpawning();
        UpdatePowerUpSpawning();
        UpdateWhaleMiniSpawning();

        if (waves == null || waves.Count == 0)
        {
            return;
        }

        waveNumber = Mathf.Clamp(waveNumber, 0, waves.Count - 1);
        Wave currentWave = waves[waveNumber];
        if (currentWave == null || !HasValidPrefab(currentWave))
        {
            // Bỏ qua cấu hình wave rỗng để các wave hợp lệ phía sau vẫn tiếp tục chạy.
            AdvanceWave();
            return;
        }

        currentWave.spawnTimer += Time.deltaTime;
        float spawnInterval = GetWaveSpawnInterval(currentWave);
        if (currentWave.spawnTimer < spawnInterval)
        {
            return;
        }

        currentWave.spawnTimer -= spawnInterval;
        if (SpawnObject(currentWave))
        {
            // Đếm số lần tạo thành công, không tính lần spawn thất bại do thiếu prefab hợp lệ.
            currentWave.spawnedObjectCount++;
            if (currentWave.spawnedObjectCount >= GetWaveObjectTarget(currentWave))
            {
                currentWave.spawnedObjectCount = 0;
                AdvanceWave();
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

        activePointStars.RemoveAll(star => star == null);
        foreach (PointStarSpawn starSpawn in pointStarSpawns)
        {
            if (starSpawn == null || starSpawn.prefab == null)
            {
                continue;
            }

            starSpawn.spawnTimer += Time.deltaTime;
            float interval = GetPointStarSpawnInterval(starSpawn);
            if (starSpawn.spawnTimer < interval)
            {
                continue;
            }

            starSpawn.spawnTimer = 0f;
            if (activePointStars.Count >= Mathf.Max(1, maxActivePointStars))
            {
                continue;
            }

            // Instantiate prefab tại tọa độ thế giới được tính từ khung hình camera hiện tại.
            GameObject star = Instantiate(
                starSpawn.prefab,
                RandomScreenPosition(starSpawn.prefab),
                Quaternion.identity);
            activePointStars.Add(star);
        }
    }

    private float GetPointStarSpawnInterval(PointStarSpawn starSpawn)
    {
        float baseInterval = Mathf.Max(0.1f, starSpawn.spawnInterval);
        float minimumInterval = Mathf.Max(3f, baseInterval - 2f);
        // Trong thời gian ramp, sao xuất hiện nhanh dần tối đa 2 giây, nhưng không dưới 3 giây.
        return Mathf.Lerp(baseInterval, minimumInterval, GetDifficultyProgress());
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
        float spawnInterval = Mathf.Max(0.1f, powerUpSpawnInterval);
        if (IsStageTwoRushActive())
        {
            // Trong đợt cao trào, orb xuất hiện theo chu kỳ ngắn hơn; giới hạn active vẫn được giữ nguyên.
            spawnInterval = Mathf.Max(0.1f, spawnInterval * stageTwoRushPowerUpIntervalMultiplier);
        }

        if (powerUpSpawnTimer < spawnInterval)
        {
            return;
        }

        powerUpSpawnTimer = 0f;
        activePowerUps.RemoveAll(powerUp => powerUp == null);

        List<GameObject> availablePrefabs = new List<GameObject>();
        HashSet<PowerUpType> availableTypes = new HashSet<PowerUpType>();
        foreach (GameObject prefab in powerUpPrefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            PowerUpOrb orb = prefab.GetComponentInChildren<PowerUpOrb>(true);
            if (orb == null)
            {
                if (!invalidPowerUpPrefabLogged)
                {
                    Debug.LogError($"Power-up prefab '{prefab.name}' does not contain a PowerUpOrb component.", prefab);
                    invalidPowerUpPrefabLogged = true;
                }

                continue;
            }

            if (orb.Type != PowerUpType.None && availableTypes.Add(orb.Type))
            {
                availablePrefabs.Add(prefab);
            }
        }

        int availableSlots = Mathf.Max(0, powerUpMaxSpawnCount - activePowerUps.Count);
        int spawnCount = Mathf.Min(currentPowerUpSpawnCount, availableSlots, availablePrefabs.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            int prefabIndex = Random.Range(0, availablePrefabs.Count);
            GameObject prefab = availablePrefabs[prefabIndex];
            availablePrefabs.RemoveAt(prefabIndex);

            // Mỗi loại orb chỉ xuất hiện tối đa một lần trong cùng một đợt spawn.
            GameObject powerUp = Instantiate(prefab, RandomScreenPosition(prefab), Quaternion.identity);
            activePowerUps.Add(powerUp);
            Destroy(powerUp, powerUpLifetime);
        }

        if (spawnCount > 0)
        {
            // Tăng số orb ở các đợt sau, nhưng không vượt giới hạn cấu hình.
            currentPowerUpSpawnCount = Mathf.Min(
                Mathf.Max(1, powerUpMaxSpawnCount),
                currentPowerUpSpawnCount + Mathf.Max(0, powerUpSpawnIncrease));
        }
    }

    private Vector3 RandomScreenPosition(GameObject prefab)
    {
        // Chuyển góc viewport sang tọa độ thế giới tại cùng mặt phẳng Z với spawner.
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

    private bool SpawnObject(Wave currentWave)
    {
        if (currentWave.prefabs == null || currentWave.prefabs.Length == 0)
        {
            return false;
        }

        int validPrefabCount = 0;
        foreach (GameObject prefab in currentWave.prefabs)
        {
            if (prefab != null)
            {
                validPrefabCount++;
            }
        }

        if (validPrefabCount == 0)
        {
            return false;
        }

        // Chọn ngẫu nhiên một prefab hợp lệ trong wave để tạo biến thể quái xuất hiện.
        int selectedIndex = Random.Range(0, validPrefabCount);
        GameObject selectedPrefab = null;
        foreach (GameObject prefab in currentWave.prefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            if (selectedIndex == 0)
            {
                selectedPrefab = prefab;
                break;
            }

            selectedIndex--;
        }

        Instantiate(selectedPrefab, RandomSpawnPoint(), Quaternion.identity);
        return true;
    }

    private float GetWaveSpawnInterval(Wave wave)
    {
        float baseInterval = Mathf.Max(0.1f, wave.spawnInterval);
        float minimumInterval = baseInterval;
        // Với thông số Level1, tốc độ spawn tối đa tăng khoảng 1.54 lần cho asteroid và 1.43 lần cho Whale.
        if (IsAsteroidWave(wave))
        {
            minimumInterval = Mathf.Min(baseInterval, 0.65f);
        }
        else if (IsWhaleWave(wave))
        {
            minimumInterval = Mathf.Min(baseInterval, 3.5f);
        }

        float interval = Mathf.Lerp(baseInterval, minimumInterval, GetDifficultyProgress());
        if (GameManager.Instance != null && GameManager.Instance.IsStageTwoActive)
        {
            interval *= stageTwoSpawnIntervalMultiplier;
        }

        return IsStageTwoRushActive()
            ? Mathf.Max(0.1f, interval * stageTwoRushSpawnIntervalMultiplier)
            : Mathf.Max(0.1f, interval);
    }

    private int GetWaveObjectTarget(Wave wave)
    {
        int baseCount = Mathf.Max(1, wave.objectPerWave);
        if (!IsAsteroidWave(wave))
        {
            return baseCount;
        }

        // Wave asteroid tăng thêm một mục tiêu mỗi phút, tối đa hai mục tiêu so với cấu hình gốc.
        int elapsedDifficultySteps = Mathf.FloorToInt(difficultyElapsedTime / 60f);
        return baseCount + Mathf.Clamp(elapsedDifficultySteps, 0, 2);
    }

    private static bool IsAsteroidWave(Wave wave)
    {
        return WaveContains<Asteroid>(wave);
    }

    private static bool IsWhaleWave(Wave wave)
    {
        return WaveContains<Whale>(wave) || WaveContains<WhaleMini>(wave);
    }

    private static bool WaveContains<T>(Wave wave) where T : Component
    {
        if (wave.prefabs == null)
        {
            return false;
        }

        foreach (GameObject prefab in wave.prefabs)
        {
            if (prefab != null && prefab.GetComponentInChildren<T>(true) != null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasValidPrefab(Wave wave)
    {
        if (wave.prefabs == null)
        {
            return false;
        }

        foreach (GameObject prefab in wave.prefabs)
        {
            if (prefab != null)
            {
                return true;
            }
        }

        return false;
    }

    private void AdvanceWave()
    {
        waveNumber = (waveNumber + 1) % waves.Count;
    }

    private float GetDifficultyProgress()
    {
        return Mathf.Clamp01(difficultyElapsedTime / Mathf.Max(1f, difficultyRampDuration));
    }

    private void UpdateWhaleMiniSpawning()
    {
        if (whaleMiniPrefab == null)
        {
            return;
        }

        float interval = Mathf.Max(0.1f, whaleMiniSpawnInterval);
        float minimumInterval = Mathf.Min(interval, 7f);
        interval = Mathf.Lerp(interval, minimumInterval, GetDifficultyProgress());
        if (GameManager.Instance != null && GameManager.Instance.IsStageTwoActive)
        {
            interval *= stageTwoSpawnIntervalMultiplier;
        }

        if (IsStageTwoRushActive())
        {
            interval = Mathf.Max(0.1f, interval * stageTwoRushSpawnIntervalMultiplier);
        }

        whaleMiniSpawnTimer += Time.deltaTime;
        if (whaleMiniSpawnTimer < interval)
        {
            return;
        }

        whaleMiniSpawnTimer -= interval;

        activeWhaleMinis.RemoveAll(whaleMini => whaleMini == null);
        int maximumActiveCount = Mathf.Max(1, whaleMiniMaxActiveCount);
        int availableSlots = Mathf.Max(0, maximumActiveCount - activeWhaleMinis.Count);
        int spawnCount = Mathf.Min(GetCurrentWhaleMiniSpawnCount(), availableSlots);
        for (int i = 0; i < spawnCount; i++)
        {
            // Prefab quái nhỏ được tạo ngoài mép phải để đi vào vùng chơi từ bên ngoài màn hình.
            GameObject whaleMini = Instantiate(
                whaleMiniPrefab,
                RandomWhaleMiniSpawnPoint(),
                Quaternion.identity);
            activeWhaleMinis.Add(whaleMini);
        }
    }

    private int GetCurrentWhaleMiniSpawnCount()
    {
        int increasePerStep = Mathf.Max(0, whaleMiniSpawnIncrease);
        // Số WhaleMini tăng theo từng phút, sau đó bị chặn bởi giới hạn Inspector.
        int elapsedDifficultySteps = Mathf.FloorToInt(difficultyElapsedTime / 60f);
        int maximumSpawnCount = Mathf.Max(1, whaleMiniMaxSpawnCount);
        return Mathf.Clamp(
            whaleMiniInitialSpawnCount + elapsedDifficultySteps * increasePerStep,
            1,
            maximumSpawnCount);
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

        // minPos/maxPos trong Inspector xác định hình chữ nhật vùng sinh quái.
        Vector2 spawnPoint;
        spawnPoint.x = Random.Range(minPos.position.x, maxPos.position.x);
        spawnPoint.y = Random.Range(minPos.position.y, maxPos.position.y);

        return spawnPoint;
    }

    public void SetStageTransitionPaused(bool paused)
    {
        isStageTransitionPaused = paused;
    }

    public void BeginStageTwoRush(float duration)
    {
        stageTwoRushRemaining = Mathf.Max(0f, duration);
    }

    private bool IsStageTwoRushActive()
    {
        return stageTwoRushRemaining > 0f;
    }
}