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

    private int maxFocus;
    private int focus;
    private int maxStamina;
    private int stamina;
    private float staminaRestore;

    bool staminaLocked = false;

    bool invuln;

    void Awake() {
        maxHealth = Vitality * 2;
        health = maxHealth;

        maxStamina = Endurance * 20;
        stamina = maxStamina;

        maxFocus = Awareness * 2;
        focus = maxFocus;
    }

    void Start() {
        if (isLocalPlayer) {
            PlayerHUDManager.Instance.RefreshHealthBar(maxHealth, health);
            PlayerHUDManager.Instance.RefreshStaminaBar(maxStamina, stamina, staminaLocked);
            PlayerHUDManager.Instance.RefreshFocusBar(maxFocus, focus);
        }
    }

    void FixedUpdate() {
        if (Time.time > staminaRestore) {
            if (staminaLocked)
                RestoreStamina(maxStamina / 100);
            else
                RestoreStamina(maxStamina / 100);
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

    public bool CanSpendStamina() {
        return stamina > 0 && !staminaLocked;
    }

    public void SpendStamina(int amt) {
        stamina -= amt;

        staminaRestore = Time.time + 0.75f;

        if (stamina <= 0) {
            stamina = 0;
            staminaLocked = true;

            staminaRestore = Time.time;
        }

        if (isLocalPlayer)
            PlayerHUDManager.Instance.RefreshStaminaBar(maxStamina, stamina, staminaLocked);
    }

    public void RestoreStamina(int amt) {
        stamina += amt;
        if (stamina >= maxStamina) {
            stamina = maxStamina;
            staminaLocked = false;
        }

        if (isLocalPlayer)
            PlayerHUDManager.Instance.RefreshStaminaBar(maxStamina, stamina, staminaLocked);
    }

    public void SetInvuln(bool state) {
        invuln = state;
    }
}
