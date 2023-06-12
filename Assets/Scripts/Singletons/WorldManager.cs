using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WorldManager : NetworkSingleton<WorldManager>
{
    [Header("EnemyPrefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<GameObject> interactablePrefabs;
    
    List<PlayerController> players = new List<PlayerController>();
    float spawnDelay = 5;
    float spawnTime;
    [SyncVar] long tick;

    [SyncVar(hook = nameof(RefreshFire))] int fireLevel;
    int maxFire = 7500; // 2.5m of fire
    [SerializeField] long dayDuration;
    [SerializeField] long nightDuration;

    public void Start() {
        spawnTime = -spawnDelay;
        fireLevel = maxFire;
    }

    void FixedUpdate()
    {
        PlayerHUDManager.Instance.SetTime(tick * Time.fixedDeltaTime);

        if (!isServer)
            return;

        tick = (tick + 1) % (50 * (dayDuration + nightDuration));

        if (fireLevel > 0)
            fireLevel--;

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

    [Server] public GameObject SpawnCorpse(Vector3 pos, bool flipX, PlayerController player) {
        GameObject corpse = Instantiate(interactablePrefabs[(int)InteractableType.CORPSE], pos, Quaternion.identity);
        corpse.GetComponent<SpriteRenderer>().flipX = flipX;
        NetworkServer.Spawn(corpse);
        corpse.GetComponent<CorpseNodeController>().SetTarget(player);
        return corpse;
    }

    public void AddPlayer(PlayerController player) {
        players.Add(player);
    }

    public List<PlayerController> GetPlayers() {
        return players;
    }

    public int GetFire() {
        return fireLevel;
    }

    public void ChangeFire(int amt) {
        if (!isServer)
            SyncChangeFire(amt);
        else
            this.fireLevel += amt;
    }

    [Command] private void SyncChangeFire(int amt) {
        this.fireLevel += amt;
    }

    private void RefreshFire(int oldFire, int newFire) {
        PlayerHUDManager.Instance.RefreshFireBar(maxFire, fireLevel);
    }
}

public enum EnemyType {
    BASIC,
}

public enum InteractableType {
    RESOURCE, CORPSE,
}
