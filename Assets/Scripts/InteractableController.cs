using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableController : MonoBehaviour
{   

    [SerializeField] private bool interactable = true;
    public bool Interactable {
        get {return interactable;}
        set {
            interactable = value;
            if (value == false && overlapping) {
                overlapping = false;
                if (onExit != null)
                    onExit.Invoke();
                if (outlineOnOverlap) {
                    List<Material> mats = new();
                    sprite.GetSharedMaterials(mats);
                    for (int i = 0; i < mats.Count; i++) {
                        if (mats[i] == outlineMat) {
                            mats.RemoveAt(i);
                            i--;
                        }
                    }   
                    sprite.materials = mats.ToArray();
                }
            }
        }
    }
    [SerializeField] private UnityEvent<PlayerController> action;
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;
    [Header("Outline")]
    [SerializeField] private bool outlineOnOverlap;
    bool overlapping = false;
    [SerializeField] private Material outlineMat;
    [SerializeField] private SpriteRenderer sprite;
    private void OnTriggerEnter2D(Collider2D other) {
        if (!interactable)
            return;
        
        overlapping = true;
        if (onEnter != null)
            onEnter.Invoke();
        if (outlineOnOverlap) {
            List<Material> mats = new();
            sprite.GetSharedMaterials(mats);
            mats.Add(outlineMat);
            sprite.materials = mats.ToArray();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!interactable)
            return;

        overlapping = false;
        if (onExit != null)
            onExit.Invoke();
        if (outlineOnOverlap) {
            List<Material> mats = new();
            sprite.GetSharedMaterials(mats);
            for (int i = 0; i < mats.Count; i++) {
                if (mats[i] == outlineMat) {
                    mats.RemoveAt(i);
                    i--;
                }
            }   
            sprite.materials = mats.ToArray();
        }
    }

    public void Interact(PlayerController interactor) {
        if (!interactable)
            return;
        if (action != null)
            action.Invoke(interactor);
    }
}
