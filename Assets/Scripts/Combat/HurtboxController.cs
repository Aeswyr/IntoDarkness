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
            if (!enemy.isServer)
                return;
            if (hitbox.GetTeam() == EntityTeam.ENEMY)
                return;
        }

        
        action.Invoke(hitbox);
    }
}
