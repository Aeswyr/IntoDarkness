using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StatController : NetworkBehaviour
{
    [SerializeField] private int Ingenuity; // Skill scaling
    [SerializeField] private int Might; // Weapon scaling
    [SerializeField] private int Vitality; // HP scaling
    [SerializeField] private int Endurance; // Stamina scaling
    [SerializeField] private int Awareness; // Resource/Luck scaling

    private int maxHealth;
    [SyncVar] private int health;

    void Start() {
        maxHealth = Vitality * 2;
        health = maxHealth;
    }

    public void OnHit(HitboxController hit) {
        HitboxData data = hit.GetData();
        StatController stats = hit.GetStats();

        int amt = data.BaseDamage + (int)(stats.Ingenuity * data.IngenuityScale) + (int)(stats.Might * data.MightScale);

        Color color = Color.red;
        if (data.IsHeal)
            color = Color.yellow;
        VFXManager.Instance.SendFloatingText(amt.ToString(), transform.position + 2 * Vector3.up, color);

        health -= amt;
        {
            float hitstopDuration = 0.1f;
            // hitstop on the target who was hit
            if (TryGetComponent(out EnemyController enemy)) {
                enemy.DoHitstop(hitstopDuration);
            } else if (TryGetComponent(out PlayerController player)) {
                player.DoHitstop(hitstopDuration);
            } 
            //hitstop on the source of the damage
            if (hit.GetOwner().TryGetComponent(out PlayerController sourcePlayer)) {
                sourcePlayer.DoHitstop(hitstopDuration);
            } else if (hit.GetOwner().TryGetComponent(out EnemyController sourceEnemy)) {
                sourceEnemy.DoHitstop(hitstopDuration);
            }
        }

        if (health <= 0) {
            if (TryGetComponent(out PlayerController player)) {
                player.OnDie();
            } else if (TryGetComponent(out EnemyController enemy)) {
                enemy.OnDie();
            }
        }
    }
}
