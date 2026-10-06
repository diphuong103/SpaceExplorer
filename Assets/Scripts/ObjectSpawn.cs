using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawn : MonoBehaviour
{
    [SerializeField] private Transform minPos; // Vị trí spawn tối thiểu
    [SerializeField] private Transform maxPos; // Vị trí spawn tối đa

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

    private float whaleMiniSpawnTimer;
    private int currentWhaleMiniSpawnCount;

    private void Start()
    {
        currentWhaleMiniSpawnCount = Mathf.Clamp(
            whaleMiniInitialSpawnCount,
            1,
            Mathf.Max(1, whaleMiniMaxSpawnCount));
    }

    void Update()
    {
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