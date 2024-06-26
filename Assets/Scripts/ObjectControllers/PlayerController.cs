using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private MovementHandler move;
    [SerializeField] private JumpHandler jump;
    [SerializeField] private GroundedCheck colCheck;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;
    [SerializeField] private StatController stats;
    [SerializeField] private InventoryController inventory;
    public InventoryController Inventory {
        get => inventory;
    }
    [SerializeField] private BoxCollider2D hurtbox;
    [SerializeField] private BoxCollider2D interactbox;

    [Header("Action Data")]
    [SerializeField] private float rollSpeed;
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField] private float bladeSpeed;
    [SerializeField] private AnimationCurve bladeCurve;
    [SerializeField] private float bladeHSpeed;
    [SerializeField] private AnimationCurve bladeHCurve;
    [Space]
    [SerializeField] private float walljumpWindow;
    [SerializeField] private float walljumpSpeed;
    [SerializeField] private float walljumpDuration;

    [Header("Combat Data")]

    [SerializeField] private float knockbackSpeed;
    [SerializeField] private AnimationCurve knockbackCurve;
    [SerializeField] private float deathSpeed;
    [SerializeField] private AnimationCurve deathCurve;
    [SerializeField] private GameObject hitboxPrefab;
    private GameObject activeHitbox = null;
    [SerializeField] private HitboxData swordPrimary;
    [SerializeField] private HitboxData swordSecondary;
    

    private int facing = 1;
    private bool grounded = false;
    private bool acting = false;
    private bool hitpaused = false;
    private float hitpauseEnd;

    private bool hitstunned = false;
    private bool dead = false;

    private bool paused = false;
    
    private bool m_hanging = false;
    private bool hanging {
        get {return m_hanging;}
        set {
            if (m_hanging && !value) {
                jump.ResetGravity();
                walljumpExpire = Time.time + walljumpWindow;
                animator.SetBool("hanging", value);
            } else if (!m_hanging && value) {
                wallDir = facing * -1;
                jump.SetGravity(1);
                jump.ForceLanding();
                jump.ForceVelocity(0);
                animator.SetBool("hanging", value);
            }
            m_hanging = value;
        }
    }

    private int actionID = 0;
    private float walljumpExpire;

    private float walljumpLockout;
    private int wallDir;
    private bool wallJumped;

    private int hitboxId;
    private int weapon;
    void Start() {
        WorldManager.Instance.AddPlayer(this);

        if (!isLocalPlayer)
            return;

        GameManager.Instance.SetCameraFollow(transform);

        interactbox.enabled = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (hitpaused && Time.time > hitpauseEnd) {
            hitpaused = false;
            animator.speed = 1;
        }

        if (hitstunned)
            return;

        bool pGrounded = grounded;
        grounded = colCheck.CheckGrounded();

        if (!pGrounded && grounded) { //landing frame
            VFXManager.Instance.SyncVFX(ParticleType.DUST_LANDING, transform.position + 0.5f * Vector3.up, facing == -1);
            
            animator.SetBool("grounded", grounded);
        } else if (pGrounded && !grounded) { // first airborne frame
            animator.SetBool("grounded", grounded);
        }

        hanging = !grounded && colCheck.CheckWallhang(facing);

        int oldFacing = facing;

        if (InputHandler.Instance.menu.pressed) {

            if (restPoint != null)
                restPoint.OnInteract(this);
            else
            {
                paused = !paused;

                if (paused) {
                    PauseMenuManager.Instance.ToggleVisibility(true);
                    if (!acting)
                        move.StartDeceleration();
                } else
                    PauseMenuManager.Instance.ToggleVisibility(false);
            }
        }

        if (paused)
            return;

        if (!acting && Time.time > walljumpLockout && InputHandler.Instance.move.pressed) {
            move.StartAcceleration(InputHandler.Instance.dir);
            if (InputHandler.Instance.dir != 0)
                facing = (int)InputHandler.Instance.dir;
            if (grounded)
                VFXManager.Instance.SyncVFX(ParticleType.DUST_SMALL, transform.position + new Vector3(-facing, 0.5f), facing == -1);
        } else if (!acting && Time.time > walljumpLockout && InputHandler.Instance.move.down) {
            move.UpdateMovement(InputHandler.Instance.dir);
            if (InputHandler.Instance.dir != 0)
                facing = (int)InputHandler.Instance.dir;
        } else if (!acting && Time.time > walljumpLockout && InputHandler.Instance.move.released) {
            move.StartDeceleration();
        }

        if (dead) {
            if (InputHandler.Instance.interact.pressed) {
                OnRevive(Vector3.zero);
                WorldManager.Instance.ChangeFire(-2000);
            }

            FinalizeFrame();
            return;
        }
            

        if (!acting && grounded && InputHandler.Instance.interact.pressed) {
            RaycastHit2D[] results = new RaycastHit2D[1];
            if (interactbox.Cast(Vector2.zero, results) > 0)
                results[0].transform.GetComponent<InteractableController>().Interact(this);
        } else if (!acting && (grounded || hanging || Time.time < walljumpExpire) && InputHandler.Instance.jump.pressed) {
            walljumpExpire = 0;
            hanging = false;

            jump.StartJump();
            AttachVFX(ParticlePrefabType.DUST_TRAIL, 1.5f * Vector3.down);
            
            if (!grounded) {
                walljumpLockout = Time.time + walljumpDuration;
                move.OverrideCurve(walljumpSpeed, rollCurve, wallDir);
                facing = wallDir;
                wallJumped = true;
            }

            animator.SetBool("grounded", false);
            animator.SetTrigger("jump");
        } else if (!acting && grounded && stats.CanSpendExert() && InputHandler.Instance.dodge.pressed) { // do a roll
            DoRoll();
        } else if (CheckAttackBlade()) { // sword
        } else if (CheckAttackBow()) { // bow
        } else if (CheckAttackShield()) { // shield
        } else if (CheckAttackTome()) { // tome
        }

        if (Time.time > walljumpLockout && wallJumped) {
            move.ResetCurves();
            wallJumped = false;
            if (InputHandler.Instance.dir == 0)
                move.StartDeceleration();
        }

        void FinalizeFrame() {
            sprite.flipX = facing == -1;

            if (oldFacing != facing)
                SendFacing(facing == -1);
        }

        FinalizeFrame();
    }
    
    private void DoRoll() {
        stats.SpendExert();
        VFXManager.Instance.SyncVFX(ParticleType.DUST_LARGE, transform.position + 0.5f * Vector3.up, facing == -1);
        AttachVFX(ParticlePrefabType.DUST_TRAIL, 1.5f * Vector3.down);
        actionID = 0;
        StartAction();
        move.OverrideCurve(rollSpeed, rollCurve, facing);
    }

    private void StartRoll() {
        stats.SetInvuln(true);
    }

    private void EndRoll() {
        stats.SetInvuln(false);
    }

    public void OnHit(int dmg, GameObject other) {
        if (!isLocalPlayer)
            return;

        if (restPoint != null)
            restPoint.OnInteract(this);

        EndAction();

        hitstunned = true;
        animator.SetTrigger("hurt");
        
            move.ForceStop();
        move.OverrideCurve(knockbackSpeed, knockbackCurve, Mathf.Sign(transform.position.x - other.transform.position.x));
    
        ResetValues();
    }

    private void OnHitEnd() {
        if (!isLocalPlayer)
            return;

        hitstunned = false;
        move.ResetCurves();

        if (InputHandler.Instance.move.down)
            move.StartAcceleration(InputHandler.Instance.dir);
    }

// reset all non-persistent move values at once
    public void ResetValues() {
        bladeCharging = false;
        bladeReady = false;
    }
    public void SpawnDust() {
        if (!isLocalPlayer || !grounded)
            return;
        VFXManager.Instance.SyncVFX(ParticleType.DUST_SMALL, transform.position + new Vector3(-facing, 0.5f), facing == -1);
    }


    RestPointController restPoint = null;
    public void ToggleRest(bool toggle, RestPointController restPoint) {
        acting = toggle;
        animator.SetBool("resting", toggle);
        move.ForceStop();
        if (toggle)
            this.restPoint = restPoint;
        else
            this.restPoint = null;
    }
    private bool bladeCharging = false;
    private bool bladeReady = false;
    private bool CheckAttackBlade() {
        if (weapon != 0)
            return false;
        if (!acting && InputHandler.Instance.primary.pressed) {
            hitboxId = 0;
            actionID = 1;
            StartAction();
            if (grounded) {
                VFXManager.Instance.SyncVFX(ParticleType.DUST_SMALL, transform.position + new Vector3(-facing, 0.5f), facing == -1);
                if (InputHandler.Instance.move.down)
                    move.OverrideCurve(bladeSpeed, bladeCurve, facing);
                else
                    move.StartDeceleration();
            }
            return true;
        } else if (!acting && stats.CanSpendExert() && InputHandler.Instance.secondary.pressed) {
            actionID = 2;
            StartAction();
            bladeCharging = true;
            move.StartDeceleration();
        } else if ((bladeCharging || bladeReady) && grounded && InputHandler.Instance.dodge.pressed) {
            if (InputHandler.Instance.dir != 0)
                facing = (int)InputHandler.Instance.dir;
            bladeCharging = false;
            bladeReady = false;
            animator.ResetTrigger("release");
            EndAction();
            DoRoll();
            return true;
        } else if ((bladeCharging || bladeReady) && InputHandler.Instance.secondary.released) {
                if (InputHandler.Instance.dir != 0)
                    facing = (int)InputHandler.Instance.dir;
                hitboxId = 1;
                animator.SetTrigger("release");
                return true;
        } else if (bladeReady || bladeCharging) {
            if (InputHandler.Instance.dir != 0)
                facing = (int)InputHandler.Instance.dir;
        }
        return false;
    }

    private void ReadyBladeH() {
        if (!isLocalPlayer)
            return;
        bladeCharging = false;
        bladeReady = true;
    }
    private void TriggerBladeH() {
        if (!isLocalPlayer)
            return;
        if (grounded)
            VFXManager.Instance.SyncVFX(ParticleType.DUST_LARGE, transform.position + 0.5f * Vector3.up, facing == -1);
        stats.SpendExert();
        bladeCharging = false;
        bladeReady = false;
        jump.SetGravity(0);
        move.OverrideCurve(bladeHSpeed, bladeHCurve, facing);
    }

    private bool CheckAttackTome() {
        return false;
    }

    private bool CheckAttackBow() {
        return false;
    }

    private bool CheckAttackShield() {
        return false;
    }

    [Command] private void SendFacing(bool flip) {
        RecieveFacing(flip);
    }

    [ClientRpc] private void RecieveFacing(bool flip) {
        sprite.flipX = flip;
    }

    private void StartAction() {
        animator.SetInteger("action_id", actionID);
        acting = true;
        animator.SetBool("acting", acting);
        animator.SetTrigger("act");
    }

    private void EndAction() {
        acting = false;
        animator.SetBool("acting", acting);

        switch (actionID) {
            case 0:
            case 1:
            case 2:
            case 3:
                move.ResetCurves();
                if (!InputHandler.Instance.move.down)
                    move.ForceStop();
                if (actionID == 2)
                    jump.ResetGravity();
                break;
        }

        DestroyActiveHitbox();
    }

    private void DestroyActiveHitbox() {
        if (activeHitbox != null) {
            Destroy(activeHitbox);
            activeHitbox = null;
        }
    }

    private void CreateActiveHitbox() {
        DestroyActiveHitbox();
        void CreateHitbox(Vector3 position, Vector2 size, HitboxData data) {
            activeHitbox = Instantiate(hitboxPrefab, position, Quaternion.identity);
            HitboxController hitbox = activeHitbox.GetComponent<HitboxController>();
            hitbox.InitializeHitbox(size, EntityTeam.PLAYER, data, stats);
            activeHitbox.transform.SetParent(transform);
        }
        switch (hitboxId) {
            case 0:
                CreateHitbox(transform.position + 2 * facing * Vector3.right, new Vector2(4, 3), swordPrimary);
                break;
            case 1:
                CreateHitbox(transform.position + 1.5f * facing * Vector3.right, new Vector2(3, 3), swordSecondary);
                break;
        }
    }

    public void DoHitstop(float duration) {
        if (isServer) 
            RecieveHitstop(duration);
        else 
            SendHitstop(duration);
    }

    [Command] private void SendHitstop(float duration) {
        RecieveHitstop(duration);
    }

    [ClientRpc] private void RecieveHitstop(float duration) {
        if (!isLocalPlayer)
            return;
        hitpauseEnd = Time.time + duration;
        jump.Pause(hitpauseEnd);
        move.Pause(hitpauseEnd);
        animator.speed = 0;
        hitpaused = true;
    }

    public void OnDie(int dmg, GameObject other) {
        if (!isLocalPlayer)
            return;

        if (restPoint != null)
            restPoint.OnInteract(this);

        EndAction();

        hitstunned = true;
        dead = true;
        if (!isServer)
            SyncDead(dead);
        stats.SetInvuln(true);

        animator.SetTrigger("dead");

        if (!InputHandler.Instance.move.down)
            move.ForceStop();
        move.OverrideCurve(deathSpeed, deathCurve, Mathf.Sign(transform.position.x - other.transform.position.x));
    
        ResetValues();
    }

    public void EndDie() {
        if (!isLocalPlayer)
            return;

        move.ResetCurves();
        move.ForceStop();
        hitstunned = false;
        if (isServer)
            WorldManager.Instance.SpawnCorpse(transform.position, sprite.flipX, this);
        else 
            SyncDie();
        animator.SetBool("ghosting", true);
    }

    [Command] private void SyncDie() {
        WorldManager.Instance.SpawnCorpse(transform.position, sprite.flipX, this);
    }

    [Command] private void SyncDead(bool value) {
        dead = value;
    }

    public void OnRevive(Vector3 pos) {
        if (!isLocalPlayer)
            return;
        transform.position = pos;

        dead = false;
        if (!isServer)
            SyncDead(dead);

        stats.SetHealth(1);
        stats.SetInvuln(false);

        animator.SetBool("ghosting", false);
    }

    [Server, ClientRpc] public void SyncRevive(Vector3 pos) {
        OnRevive(pos);
    }

    public bool IsDead() {
        return dead;
    }

//  commands and rpcs for attaching various particle effects to the player prefab. For unattached particles
//  use the VFXManager SendVFX flow
    [Command] private void AttachVFX(ParticlePrefabType type, Vector3 pos) {
        RecieveAttachVFX(type, pos);
    }
    [ClientRpc] private void RecieveAttachVFX(ParticlePrefabType type, Vector3 pos) {
        VFXManager.Instance.CreatePrefabVFX(type, pos, transform);
    }
    [Command] private void AttachVFX(ParticleType type, Vector3 pos, bool flip) {
        RecieveAttachVFX(type, pos, flip);
    }
    [ClientRpc] private void RecieveAttachVFX(ParticleType type, Vector3 pos, bool flip) {
        VFXManager.Instance.CreateVFX(type, pos, flip, transform);
    }
}
