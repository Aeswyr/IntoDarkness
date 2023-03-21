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
    [SerializeField] private BoxCollider2D hurtbox;

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
    [SerializeField] private GameObject hitboxPrefab;
    private GameObject activeHitbox = null;
    [SerializeField] private HitboxData swordPrimary;
    [SerializeField] private HitboxData swordSecondary;

    private int facing = 1;
    private bool grounded = false;
    private bool acting = false;
    private bool hitpaused = false;
    private float hitpauseEnd;
    
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


    private int weapon;
    void Start() {
        if (!isLocalPlayer)
            return;

        GameManager.Instance.SetCameraFollow(transform);
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

        bool pGrounded = grounded;
        grounded = colCheck.CheckGrounded();

        if (!pGrounded && grounded) { //landing frame
            VFXManager.Instance.SendVFX(ParticleType.DUST_LANDING, transform.position + 0.5f * Vector3.up, facing == -1);
            
            animator.SetBool("grounded", grounded);
        } else if (pGrounded && !grounded) { // first airborne frame
            animator.SetBool("grounded", grounded);
        }

        hanging = !grounded && colCheck.CheckWallhang(facing);

        int oldFacing = facing;

        if (!acting && Time.time > walljumpLockout && InputHandler.Instance.move.pressed) {
            move.StartAcceleration(InputHandler.Instance.dir);
            if (InputHandler.Instance.dir != 0)
                facing = (int)InputHandler.Instance.dir;
            if (grounded)
                VFXManager.Instance.SendVFX(ParticleType.DUST_SMALL, transform.position + new Vector3(-facing, 0.5f), facing == -1);
        } else if (!acting && Time.time > walljumpLockout && InputHandler.Instance.move.down) {
            move.UpdateMovement(InputHandler.Instance.dir);
            if (InputHandler.Instance.dir != 0)
                facing = (int)InputHandler.Instance.dir;
        } else if (!acting && Time.time > walljumpLockout && InputHandler.Instance.move.released) {
            move.StartDeceleration();
        }

        if (!acting && (grounded || hanging || Time.time < walljumpExpire) && InputHandler.Instance.jump.pressed) {
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
        } else if (!acting && grounded && InputHandler.Instance.dodge.pressed) { // do a roll
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

        sprite.flipX = facing == -1;

        if (oldFacing != facing)
            SendFacing(facing == -1);

        GameManager.Instance.GetLevel().UpdateWaterline(transform.position.x);
    }
    
    private void DoRoll() {
        VFXManager.Instance.SendVFX(ParticleType.DUST_LARGE, transform.position + 0.5f * Vector3.up, facing == -1);
        AttachVFX(ParticlePrefabType.DUST_TRAIL, 1.5f * Vector3.down);
        actionID = 0;
        StartAction();
        move.OverrideCurve(rollSpeed, rollCurve, facing);
    }

    public void SpawnDust() {
        if (!isLocalPlayer || !grounded)
            return;
        VFXManager.Instance.SendVFX(ParticleType.DUST_SMALL, transform.position + new Vector3(-facing, 0.5f), facing == -1);
    }

    private bool bladeCharging = false;
    private bool bladeReady = false;
    private bool CheckAttackBlade() {
        if (weapon != 0)
            return false;
        if (!acting && InputHandler.Instance.primary.pressed) {
            actionID = 1;
            StartAction();
            if (grounded) {
                VFXManager.Instance.SendVFX(ParticleType.DUST_SMALL, transform.position + new Vector3(-facing, 0.5f), facing == -1);
                if (InputHandler.Instance.move.down)
                    move.OverrideCurve(bladeSpeed, bladeCurve, facing);
                else
                    move.StartDeceleration();
            }
            return true;
        } else if (!acting && InputHandler.Instance.secondary.pressed) {
            actionID = 2;
            StartAction();
            bladeCharging = true;
            move.StartDeceleration();
        } else if ((bladeCharging || bladeReady) && grounded && InputHandler.Instance.dodge.pressed) {
            if (InputHandler.Instance.dir != 0)
                facing = (int)InputHandler.Instance.dir;
            bladeCharging = false;
            animator.ResetTrigger("release");
            EndAction();
            DoRoll();
            return true;
        } else if ((bladeCharging || bladeReady) && InputHandler.Instance.secondary.released) {
                if (InputHandler.Instance.dir != 0)
                    facing = (int)InputHandler.Instance.dir;
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
            VFXManager.Instance.SendVFX(ParticleType.DUST_LARGE, transform.position + 0.5f * Vector3.up, facing == -1);
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

    private void CreateActiveHitbox(int type) {
        DestroyActiveHitbox();
        void CreateHitbox(Vector3 position, Vector2 size, HitboxData data) {
            activeHitbox = Instantiate(hitboxPrefab, position, Quaternion.identity);
            HitboxController hitbox = activeHitbox.GetComponent<HitboxController>();
            hitbox.InitializeHitbox(size, EntityTeam.PLAYER, data, stats);
            activeHitbox.transform.SetParent(transform);
        }
        switch (type) {
            case 0:
                CreateHitbox(transform.position + 2 * facing * Vector3.right, new Vector2(4, 3), swordPrimary);
                break;
            case 1:
                CreateHitbox(transform.position + 1.5f * facing * Vector3.right, new Vector2(3, 3), swordSecondary);
                break;
        }
    }

    [Command] public void DoHitstop(float duration) {
        RecieveHitstop(duration);
    }

    [ClientRpc] private void RecieveHitstop(float duration) {
        if (!isLocalPlayer)
            return;
        jump.Pause(Time.time + duration);
        move.Pause(Time.time + duration);
        animator.speed = 0;
        hitpauseEnd = Time.time + duration;
        hitpaused = true;
    }

    public void OnDie() {
        VFXManager.Instance.SendFloatingText("* D E A D *", transform.position, Color.white);
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
        VFXManager.Instance.CreateVFX(type, pos, flip);
    }


    
}