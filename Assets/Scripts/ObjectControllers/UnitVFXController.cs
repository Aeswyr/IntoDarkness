using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitVFXController : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer sprite;

    public void SyncAfterimage(float duration) {
        if (isServer) 
            RecieveAfterimage(duration);
        else 
            SendAfterimage(duration);
    }
    [Command] private void SendAfterimage(float duration) {
        RecieveAfterimage(duration);
    }

    [ClientRpc] private void RecieveAfterimage(float duration) {
        GameObject afterimage = Instantiate(VFXManager.Instance.GetAfterimagePrefab(), transform.position, Quaternion.identity);

        var renderer = afterimage.GetComponent<SpriteRenderer>();
        var target = sprite;
        renderer.sprite = target.sprite;
        renderer.material = target.material;
        renderer.flipX = target.flipX;
        
        afterimage.GetComponent<DestroyAfterDelay>().Init(duration);
    }
}
