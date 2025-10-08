using UnityEngine;
using Unity.AI.Navigation;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject target;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private string TypeToSpawn = "Demo";

    private float spawnTime = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= spawnTime)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        GameObject newEnemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)), transform.rotation);
        spawnTime = Time.time + 10f;
        newEnemy.GetComponent<AIBehaviour>().SetTarget(target);
        newEnemy.GetComponent<AIBehaviour>().SetNavMesh(navMesh);
        newEnemy.GetComponent<AIBehaviour>().SetType(TypeToSpawn);
    }
}