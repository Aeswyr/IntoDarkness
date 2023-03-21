using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    public void OnDie() {
        NetworkServer.Destroy(gameObject);
    }

    [Command(requiresAuthority = false)] public void DoHitstop(float duration) {
        RecieveHitstop(duration);
    }

    private void RecieveHitstop(float duration) {

    }
}
