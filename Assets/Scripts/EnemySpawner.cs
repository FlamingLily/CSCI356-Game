using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject target;
    public NavMeshSurface navMesh;

    List<string> spawnQueue = new List<string>();
    float lastSpawnTime = 0f;

    // Update is called once per frame
    void Update()
    {
        if (spawnQueue.Count > 0 && Time.time > lastSpawnTime + 1f)
        {
            SpawnEnemy(spawnQueue[0]);
            spawnQueue.RemoveAt(0);
            lastSpawnTime = Time.time;
        }
    }

    public void AddToSpawnQueue(string TypeToSpawn)
    {
        spawnQueue.Add(TypeToSpawn);
    }

    public void SpawnEnemy(string TypeToSpawn)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, transform.position + new Vector3(2f, 0f, Random.Range(-2f, 2f)), transform.rotation);
        newEnemy.GetComponent<AIBehaviour>().SetType(TypeToSpawn);
        newEnemy.GetComponent<AIBehaviour>().SetWaveController(transform.parent.gameObject);
    }
}