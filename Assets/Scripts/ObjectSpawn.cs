using UnityEngine;

using System.Collections.Generic;

public class ObjectSpawn : MonoBehaviour
{

    [SerializeField] private Transform minPos; // Vị trí spawn tối thiểu (góc dưới bên phải)
    [SerializeField] private Transform maxPos; // Vị trí spawn tối đa (g

    [SerializeField] private int waveNumber;
    [SerializeField] private List<Wave> waves; // Danh sách các wave

    [System.Serializable]
    class Wave
    {

        public GameObject prefab;
        public float spawnTimer;

        public float spawnInterval = 2f; // Thời gian giữa các lần spawn

        public int objectPerWave;
        public int spawnedObjectCount;

    }

    void Update()
    {
        waves[waveNumber].spawnTimer += Time.deltaTime * PlayerController.Instance.boost; // Tăng tốc spawn khi Player đang Boost
        if (waves[waveNumber].spawnTimer >= waves[waveNumber].spawnInterval)
        {
            waves[waveNumber].spawnTimer = 0f;
            SpawnObject();
        }
        if(waves[waveNumber].spawnedObjectCount >= waves[waveNumber].objectPerWave)
        {
            waveNumber++;
            if (waveNumber >= waves.Count)
            {
                waveNumber = 0; // Quay lại wave đầu tiên nếu đã hết
            }
        }
    }

    void SpawnObject()
{
    var wave = waves[waveNumber];

    if (wave.prefab == null)
        return;

    Instantiate(wave.prefab, RandomSpawnPoint(), Quaternion.identity);
    wave.spawnedObjectCount++;
}

    private Vector2 RandomSpawnPoint()
    {
        Vector2 spawnPoint;

        spawnPoint.x = Random.Range(minPos.position.x, maxPos.position.x);
        spawnPoint.y = Random.Range(minPos.position.y, maxPos.position.y);

        return spawnPoint;
    }
}
