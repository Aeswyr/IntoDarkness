using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StatController : NetworkBehaviour
{
    [SerializeField] private int Ingenuity; // Skill scaling
    [SerializeField] private int Might; // Weapon scaling
    [SerializeField] private int Precision; // Luck scaling

    [SerializeField] private int maxHealth;
    [SyncVar] private int health;

    [SerializeField] private int maxFocus;
    private int focus;
    [SerializeField] private int maxExert;
    private int exert;
    private float exertTimer;

    bool invuln;

    void Awake() {
        health = maxHealth;

        exert = maxExert;

        focus = maxFocus;
    }

    void Start() {
        if (isLocalPlayer) {
            PlayerHUDManager.Instance.RefreshHealthBar(maxHealth, health);
            PlayerHUDManager.Instance.RefreshExertBar(maxExert, exert);
            PlayerHUDManager.Instance.RefreshFocusBar(maxFocus, focus);
        }
    }

    void FixedUpdate() {
        
        if (exert < maxExert && Time.time > exertTimer) {
            RestoreExert(1);
            exertTimer = 2f + Time.time;
        }
    }

    public void OnHit(HitboxController hit) {
        HitboxData data = hit.GetData();
        StatController stats = hit.GetStats();

        int amt = data.BaseDamage + (int)(stats.Ingenuity * data.IngenuityScale) + (int)(stats.Might * data.MightScale);

        Color color = Color.red;
        if (data.IsHeal)
            color = Color.yellow;

        if (!data.IsHeal && invuln) {
            if (gameObject.TryGetComponent(out UnitVFXController vfxController)) 
            vfxController.SyncAfterimage(0.5f);
            return;
        }
            
        VFXManager.Instance.SendFloatingText(amt.ToString(), transform.position + 2 * Vector3.up, color);
        bool flip = false;
        if (hit.GetOwner().TryGetComponent(out SpriteRenderer sprite))
            flip = sprite.flipX;
        VFXManager.Instance.SyncVFX(ParticleType.HITSPARK_DEFAULT, transform.position, flip);

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

        if (isLocalPlayer)
            PlayerHUDManager.Instance.RefreshHealthBar(maxHealth, health);
    }

    public bool CanSpendExert() {
        return exert > 0;
    }

    public void SpendExert() {
        exert--;

        exertTimer = 2f + Time.time;

        if (isLocalPlayer)
            PlayerHUDManager.Instance.RefreshExertBar(maxExert, exert);
    }

    public void RestoreExert(int amt) {
        exert += amt;
        if (exert >= maxExert) {
            exert = maxExert;
        }

        if (isLocalPlayer)
            PlayerHUDManager.Instance.RefreshExertBar(maxExert, exert);
    }

    public void SetInvuln(bool state) {
        invuln = state;
    }
}
