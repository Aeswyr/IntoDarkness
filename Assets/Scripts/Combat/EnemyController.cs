using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private StatController stats;
    [SerializeField] private MovementHandler move;
    [SerializeField] private SpriteRenderer sprite;

    bool acting;

    int actionID;
    int facing = 1;
    float actionDelay = 0.15f;
    float actionTimer;

    [Header("Combat Data")]
    [SerializeField] private GameObject hitboxPrefab;
    private GameObject activeHitbox = null;
    [SerializeField] private HitboxData data;
    [SerializeField] private AnimationCurve attackMotion;

    bool hitpaused;
    float hitpauseEnd;
    public void OnDie() {
        if (isServer)
            NetworkServer.Destroy(gameObject);
        else
            SyncDie();
    }

    [Command(requiresAuthority = false)] private void SyncDie() {
        NetworkServer.Destroy(gameObject);
    }



    public void DoHitstop(float duration) {
        if (isServer) 
            RecieveHitstop(duration);
        else 
            SendHitstop(duration);
    }

    void FixedUpdate() {
        if (!isServer)
            return;

        if (hitpaused && Time.time > hitpauseEnd) {
            hitpaused = false;
            animator.speed = 1;
        }

        var closestPlayer = GetClosestPlayer();
        var dist = 0 - transform.position.x;
        if (closestPlayer != null)
            dist = closestPlayer.transform.position.x - transform.position.x;

        int oldFacing = facing;


        if (!acting && (Mathf.Abs(dist) > 5 || Time.time < actionTimer)) {
            facing = (int)Mathf.Sign(dist);
            move.UpdateMovement(facing);
        } else if (!acting && Time.time >= actionTimer) {
            move.OverrideCurve(40, attackMotion, facing);
            actionID = 0;
            StartAction();
        }


        if (oldFacing != facing)
            RecieveFacing(facing == -1);
    }

    private PlayerController GetClosestPlayer() {
        PlayerController target = null;

        foreach (var player in WorldManager.Instance.GetPlayers())
            if ((target == null && !player.IsDead()) || (!player.IsDead() &&
                Mathf.Abs(transform.position.x - player.transform.position.x) < Mathf.Abs(transform.position.x - target.transform.position.x)))
                target = player;

        return target;
    }

    [Command(requiresAuthority = false)] private void SendHitstop(float duration) {
        RecieveHitstop(duration);
    }

    [ClientRpc] void RecieveFacing(bool facing) {
        sprite.flipX = facing;
    }

    [ClientRpc] private void RecieveHitstop(float duration) {
        if (!isServer)
            return;

        hitpaused = true;
        hitpauseEnd = Time.time + duration;
        move.Pause(hitpauseEnd);

        animator.speed = 0;
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
            hitbox.InitializeHitbox(size, EntityTeam.ENEMY, data, stats);
            activeHitbox.transform.SetParent(transform);
        }
        switch (type) {
            case 0:
                CreateHitbox(transform.position + 2 * facing * Vector3.right, new Vector2(4, 3), data);
                break;
        }
    }

    private void StartAction() {
        animator.SetInteger("action_id", actionID);
        acting = true;
        animator.SetBool("acting", acting);
        animator.SetTrigger("act");
    }

    private void EndAction() {
        actionTimer = Time.time + actionDelay;

        acting = false;
        animator.SetBool("acting", false);

        switch (actionID) {
            case 0:
            case 1:
            case 2:
            case 3:
                move.ResetCurves();
                move.ForceStop();
                break;
        }

        DestroyActiveHitbox();
    }
}
