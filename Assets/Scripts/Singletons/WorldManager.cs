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
    [SerializeField] GameObject sun;
    [SerializeField] long dayDuration;
    [SerializeField] long nightDuration;
    private long tickDayDuration, tickNightDuration;

    public void Start() {
        spawnTime = -spawnDelay;
        fireLevel = maxFire;
        tickDayDuration = dayDuration * 50;
        tickNightDuration = nightDuration * 50;
    }

    void FixedUpdate()
    {
        //PlayerHUDManager.Instance.SetTime(tick * Time.fixedDeltaTime);
        float mod = 1;
        if (tick > tickDayDuration)
            mod = ((tick - tickDayDuration) - (tickNightDuration / 2f)) / (tickNightDuration / 2f);
        else
            mod = (tick - (tickDayDuration / 2f)) / (tickDayDuration / 2f);
        
        sun.transform.localPosition = new Vector2(mod * 35, Mathf.Abs(mod) * -10 + 20);

        if (!isServer)
            return;

        tick = (tick + 1) % (tickDayDuration + tickNightDuration);

        if (fireLevel > 0)
            fireLevel--;

        if (Time.time - spawnTime > spawnDelay ) {
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

    public PlayerController GetLocalPLayer() {
        foreach (var player in players) {
            if (player.isLocalPlayer)
                return player;
        }
        return null;
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
