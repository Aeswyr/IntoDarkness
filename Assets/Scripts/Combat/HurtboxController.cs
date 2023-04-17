using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HurtboxController : MonoBehaviour
{
    [SerializeField] private GameObject owner;
    [SerializeField] private UnityEvent<HitboxController> action;
    private void OnTriggerEnter2D(Collider2D other) {
        HitboxController hitbox = other.GetComponent<HitboxController>();

        if (owner.TryGetComponent(out PlayerController player)) {
            if (!player.isLocalPlayer)
                return;
            if (hitbox.GetTeam() == EntityTeam.PLAYER)
                return;
        }

        if (owner.TryGetComponent(out EnemyController enemy)) {
            if (hitbox.GetTeam() == EntityTeam.ENEMY)
                return;
            if (hitbox.GetOwner().TryGetComponent(out PlayerController sourcePlayer)
                && !sourcePlayer.isLocalPlayer) {
                    return;
            }
        }

        if (hitbox.CanHit(owner))
            action.Invoke(hitbox);
    }
}
