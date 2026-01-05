using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpwan : MonoBehaviour
{
    public GameObject npcPrefab;
    public int spawnCount = 5;
    public float spawnRange = 150f;

    void Start()
    {
        SpawnRandomNPCs();
    }

    void SpawnRandomNPCs()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPos = GetRandomPosition();
            Instantiate(npcPrefab, randomPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-spawnRange, spawnRange);
        float y = Random.Range(100, spawnRange);
        float z = Random.Range(-spawnRange, spawnRange);

        return new Vector3(x, y, z);
    }
}