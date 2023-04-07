using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InventoryController : NetworkBehaviour
{
    Dictionary<ResourceCardType, int> resources = new();
    [SerializeField] int resourceCap = 5;
    public int AddResource(ResourceCardType type, int amt) {
        if (!resources.ContainsKey(type))
            resources.Add(type, 0);

        int toAdd = Mathf.Min(resourceCap - resources[type], amt);

        resources[type] += toAdd;
        if (isLocalPlayer)
            PlayerHUDManager.Instance.AddResource(type, toAdd);

        return amt - toAdd;
    }

}

public enum ResourceCardType {
    DEFAULT, WOOD, STONE
}