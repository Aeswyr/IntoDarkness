using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WorldManager : NetworkSingleton<WorldManager>
{
    [Header("EnemyPrefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    
    List<PlayerController> players = new List<PlayerController>();
    float spawnDelay = 5;
    float spawnTime;
    [SyncVar] long tick;
    [SerializeField] long dayDuration;
    [SerializeField] long nightDuration;

    public void Start() {
        spawnTime = -spawnDelay;
    }

    void FixedUpdate()
    {
        PlayerHUDManager.Instance.SetTime(tick * Time.fixedDeltaTime);

        if (!isServer)
            return;

        tick = (tick + 1) % (50 * (dayDuration + nightDuration));

        if (Time.time - spawnTime > spawnDelay && tick > dayDuration * 0/*50*/) {
            spawnTime = Time.time;
            SpawnEnemy(EnemyType.BASIC, new Vector2(Random.Range(-16, 16), 0));
        }
    }

    void SpawnEnemy(EnemyType type, Vector2 pos) {
        if (!isServer)
            return;

        NetworkServer.Spawn(Instantiate(enemyPrefabs[(int)type], pos, Quaternion.identity));
    }

    public void AddPlayer(PlayerController player) {
        players.Add(player);
    }

    public List<PlayerController> GetPlayers() {
        return players;
    }
}

public enum EnemyType {
    BASIC,
}
