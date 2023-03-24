using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VFXManager : NetworkSingleton<VFXManager>
{
    [Header("Simple Particles")]
    [SerializeField] private GameObject template;
    [SerializeField] private AnimationClip[] anims;

    [Header("Particle Systems")]
    [SerializeField] private List<GameObject> particles;

    [Header("Floating Text")]
    [SerializeField] private GameObject textPrefab;

    public void SyncVFX(ParticleType type, Vector3 pos, bool flip) {
        if (isServer)
            RecieveVFX(type, pos, flip);
        else
            SendVFX(type, pos, flip);
    }
    [Command(requiresAuthority = false)] private void SendVFX(ParticleType type, Vector3 pos, bool flip) {
        RecieveVFX(type, pos, flip);
    }

    [ClientRpc] private void RecieveVFX(ParticleType type, Vector3 pos, bool flip) {
        CreateVFX(type, pos, flip);
    }

    public void CreateVFX(ParticleType type, Vector3 pos, bool flip) {
        GameObject particle = Instantiate(template, pos, Quaternion.identity);

        Animator animator = particle.GetComponent<Animator>();
        AnimatorOverrideController animController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animController["particle"] = anims[(int)type];
        animator.runtimeAnimatorController = animController;

        particle.GetComponent<DestroyAfterDelay>().Init(anims[(int)type].length);

        particle.GetComponent<SpriteRenderer>().flipX = flip;
    }

    public void SyncPrefabVFX(ParticlePrefabType type, Vector3 pos) {
        if (isServer)
            RecievePrefabVFX(type, pos);
        else
            SendPrefabVFX(type, pos);
    }
    [Command(requiresAuthority = false)] private void SendPrefabVFX(ParticlePrefabType type, Vector3 pos) {
        RecievePrefabVFX(type, pos);
    }

    [ClientRpc] private void RecievePrefabVFX(ParticlePrefabType type, Vector3 pos) {
        CreatePrefabVFX(type, pos, null);
    }

    public void CreatePrefabVFX(ParticlePrefabType type, Vector3 pos, Transform parent = null) {
        GameObject particle;
        if (parent != null) {
            particle = Instantiate(particles[(int)type], parent);
            particle.transform.localPosition = pos;
        } else
            particle = Instantiate(particles[(int)type], pos, Quaternion.identity);
    }

    [Command(requiresAuthority = false)] public void SendFloatingText(string text, Vector3 pos, Color color) {
        RecieveFloatingText(text, pos, color);
    }

    [ClientRpc] private void RecieveFloatingText(string text, Vector3 pos, Color color) {
        CreateFloatingText(text, pos, color);
    }

    public void CreateFloatingText(string text, Vector3 pos, Color color) {
        GameObject textFloater = Instantiate(textPrefab, pos, Quaternion.identity);

        FloatingTextController controller = textFloater.GetComponent<FloatingTextController>();
    
        controller.SetText(text, color);
    }
}

public enum ParticleType {
    DUST_SMALL, DUST_LARGE, DUST_LANDING, HITSPARK_DEFAULT
}

public enum ParticlePrefabType {
    DUST_TRAIL,
}
