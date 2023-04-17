using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    EntityTeam team;
    HitboxData hitData;
    StatController sourceStats;
    List<GameObject> targetsHit = new();

    [SerializeField] private BoxCollider2D hitbox;

    public void InitializeHitbox(Vector2 size, EntityTeam team, HitboxData hitData, StatController sourceStats) {
        this.team = team;
        this.hitData = hitData;
        hitbox.size = size;
        this.sourceStats = sourceStats;
        hitbox.enabled = true;
    }

    public EntityTeam GetTeam() {
        return team;
    }

    public HitboxData GetData() {
        return hitData;
    }

    public StatController GetStats() {
        return sourceStats;
    }

    public GameObject GetOwner() {
        return sourceStats.gameObject;
    }

    public bool CanHit(GameObject target) {
        if (targetsHit.Contains(target))
            return false;

        targetsHit.Add(target);
        return true;
    }


}

public enum EntityTeam {
    PLAYER, ENEMY, NONE
}
