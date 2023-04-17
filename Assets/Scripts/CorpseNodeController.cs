using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CorpseNodeController : NetworkBehaviour
{
    public void OnInteract(PlayerController interactor) {
        if (isServer)
            RecieveInteract();
        else
            SendInteract();
    }

    [Command] private void SendInteract() {
        RecieveInteract();
    }

    [ClientRpc] private void RecieveInteract() {

    }
}
