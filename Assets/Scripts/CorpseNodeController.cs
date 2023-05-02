using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CorpseNodeController : NetworkBehaviour
{
    private PlayerController target;
    public void OnInteract(PlayerController interactor) {
        if (isServer)
            RecieveInteract();
        else
            SendInteract();
    }

    [Command(requiresAuthority = false)] private void SendInteract() {
        RecieveInteract();
    }

    private void RecieveInteract() {
        if (target != null) {
            target.SyncRevive(transform.position);
        }
        NetworkServer.Destroy(gameObject);
    }

    public void SetTarget(PlayerController target) {
        this.target = target;
    }

    void FixedUpdate() {
        if (isServer && target != null && !target.IsDead())
            NetworkServer.Destroy(gameObject);
    }
}
