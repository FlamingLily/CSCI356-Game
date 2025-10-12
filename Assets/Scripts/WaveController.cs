using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GameObject;

public class WaveController : MonoBehaviour
{

    int currentWave = 0;

    Dictionary<string, int> enemiesThisWave = new Dictionary<string, int>()
    {
        {"Soldier", 0},
        {"Tank", 0},
        {"Cloaker", 0},
        {"Warper", 0},
        {"Swarmer", 0},
        {"Melee", 0}
    };
    int enemiesRemaining = 0;
    int totalSpawners;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        totalSpawners = transform.childCount;
        currentWave = 1;
        SetupWave();
        StartWave();

    }

    // Update is called once per frame
    void Update()
    {
        if (enemiesRemaining <= 0)
        {
            currentWave++;
            SetupWave();
            StartWave();
        }
    }

    void GetEnemiesRemaining()
    {
        enemiesRemaining = enemiesThisWave["Soldier"] + enemiesThisWave["Melee"] + enemiesThisWave["Swarmer"] + enemiesThisWave["Cloaker"] + enemiesThisWave["Warper"] + enemiesThisWave["Tank"];
    }
    void SetupWave()
    {
        switch (currentWave)
        {
            case 1:
                enemiesThisWave["Soldier"] = 10;
                GetEnemiesRemaining();
                break;
            case 2:
                enemiesThisWave["Soldier"] = 15;
                enemiesThisWave["Melee"] = 5;
                GetEnemiesRemaining();
                break;
            case 3:
                enemiesThisWave["Soldier"] = 10;
                enemiesThisWave["Melee"] = 10;
                enemiesThisWave["Swarmer"] = 10;
                GetEnemiesRemaining();
                break;
            case 4:
                enemiesThisWave["Soldier"] = 10;
                enemiesThisWave["Melee"] = 10;
                enemiesThisWave["Swarmer"] = 10;
                enemiesThisWave["Cloaker"] = 5;
                GetEnemiesRemaining();
                break;
            case 5:
                enemiesThisWave["Soldier"] = 10;
                enemiesThisWave["Melee"] = 10;
                enemiesThisWave["Swarmer"] = 10;
                enemiesThisWave["Cloaker"] = 5;
                enemiesThisWave["Warper"] = 5;
                GetEnemiesRemaining();
                break;
            case 6:
                enemiesThisWave["Soldier"] = 10;
                enemiesThisWave["Melee"] = 10;
                enemiesThisWave["Swarmer"] = 10;
                enemiesThisWave["Cloaker"] = 5;
                enemiesThisWave["Warper"] = 5;
                enemiesThisWave["Tank"] = 1;
                GetEnemiesRemaining();
                break;
            default: //Random
                enemiesThisWave["Soldier"] = Random.Range(5, 10 + currentWave);
                enemiesThisWave["Melee"] = Random.Range(5, 10 + currentWave);
                enemiesThisWave["Swarmer"] = Random.Range(5, 10 + currentWave);
                enemiesThisWave["Cloaker"] = Random.Range(0, 5 + currentWave / 2);
                enemiesThisWave["Warper"] = Random.Range(0, 5 + currentWave / 2);
                enemiesThisWave["Tank"] = Random.Range(0, 1 + currentWave / 5);
                GetEnemiesRemaining();
                break;

        }
    }
    void StartWave()
    {
        Debug.Log("Starting wave " + currentWave + " with " + enemiesRemaining + " enemies.");
        for (int i = 0; i < enemiesRemaining; i++)
        {
            switch (Random.Range(0, 6))
            {
                case 0:
                    if (enemiesThisWave["Soldier"] > 0)
                    {
                        enemiesThisWave["Soldier"]--;
                        ActivateSpawner("Soldier");
                    }
                    else
                    {
                        i--;
                    }
                    break;
                case 1:
                    if (enemiesThisWave["Melee"] > 0)
                    {
                        enemiesThisWave["Melee"]--;
                        ActivateSpawner("Melee");
                    }
                    else
                    {
                        i--;
                    }
                    break;
                case 2:
                    if (enemiesThisWave["Swarmer"] > 0)
                    {
                        enemiesThisWave["Swarmer"]--;
                        ActivateSpawner("Swarmer");
                    }
                    else
                    {
                        i--;
                    }
                    break;
                case 3:
                    if (enemiesThisWave["Cloaker"] > 0)
                    {
                        enemiesThisWave["Cloaker"]--;
                        ActivateSpawner("Cloaker");
                    }
                    else
                    {
                        i--;
                    }
                    break;
                case 4:
                    if (enemiesThisWave["Warper"] > 0)
                    {
                        enemiesThisWave["Warper"]--;
                        ActivateSpawner("Warper");
                    }
                    else
                    {
                        i--;
                    }
                    break;
                case 5:
                    if (enemiesThisWave["Tank"] > 0)
                    {
                        enemiesThisWave["Tank"]--;
                        ActivateSpawner("Tank");
                    }
                    else
                    {
                        i--;
                    }
                    break;
                default:
                    Debug.Log("Unknown enemy type to spawn");
                    break;
            }
        }
    }

    void ActivateSpawner(string EnemyToSpawn)
    {
        Debug.Log("Spawning " + EnemyToSpawn);
        int spawnerInt;
        int checkCycles = 0; //sanity
        bool spawnerFound = false;
        Transform thisSpawner = null;
        while (!spawnerFound)
        {
            spawnerInt = Random.Range(0, totalSpawners);
            thisSpawner = transform.GetChild(spawnerInt);
            if (Vector3.Distance(thisSpawner.position, FindGameObjectWithTag("Player").transform.position) > 50f)
            {
                spawnerFound = true;
                break;
            }
            checkCycles++;
            if (checkCycles > 100)
            {
                Debug.LogWarning("Too many attempts to find a valid spawner.");
                spawnerFound = true;
                break;
            }
        }
        thisSpawner.GetComponent<EnemySpawner>().AddToSpawnQueue(EnemyToSpawn);
    }
    public void EnemyKilled()
    {
        enemiesRemaining--;
        Debug.Log("Enemy killed, " + enemiesRemaining + " remaining.");
    }
}
