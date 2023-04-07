using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceNodeController : NetworkBehaviour
{
    [SerializeField] private ResourceCardType type;
    [SerializeField] private int amt;
    [SerializeField] private int maxUses;
    [SerializeField] private InteractableController interact;
    [SyncVar] private int uses;

    void Start() {
        uses = maxUses;
    }

    public void OnInteract(PlayerController interactor) {
        if (uses <= 0)
            return;
        
        if (amt > interactor.Inventory.AddResource(type, amt)) {
            VFXManager.Instance.CreateVFX(ParticleType.PICKUP, 2 * Vector3.up + transform.position, false, renderBehind: true);
            SpendUse();
            if (uses == 0)
                interact.Interactable = false;
        } else
            VFXManager.Instance.CreateVFX(ParticleType.PICKUP_FAILED, 2 * Vector3.up + transform.position, false, renderBehind: true);
    }

    [Command(requiresAuthority = false)] void SpendUse() {
        uses--;
        if (uses <= 0)
            SetUninteractable();
    }

    [ClientRpc] void SetUninteractable() {
        interact.Interactable = false;
    }
}
