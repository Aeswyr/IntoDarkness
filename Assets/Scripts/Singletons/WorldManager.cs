using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WorldManager : NetworkSingleton<WorldManager>
{
    [Header("EnemyPrefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    
    float spawnDelay = 10;
    float spawnTime;

    public void Start() {
        spawnTime = -spawnDelay;
    }

    void FixedUpdate()
    {
        
        if (!isServer)
            return;

        if (Time.time - spawnTime > spawnDelay) {
            spawnTime = Time.time;
            SpawnEnemy(EnemyType.BASIC, new Vector2(Random.Range(-16, 16), 0));
        }
    }

    void SpawnEnemy(EnemyType type, Vector2 pos) {
        if (!isServer)
            return;

        NetworkServer.Spawn(Instantiate(enemyPrefabs[(int)type], pos, Quaternion.identity));
    }
}

public enum EnemyType {
    BASIC,
}
