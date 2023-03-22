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

    public void DoHitstop(float duration) {
        if (isServer) 
            RecieveHitstop(duration);
        else 
            SendHitstop(duration);
    }

    [Command(requiresAuthority = false)] private void SendHitstop(float duration) {
        RecieveHitstop(duration);
    }

    private void RecieveHitstop(float duration) {

    }
}
