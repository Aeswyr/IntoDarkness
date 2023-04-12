using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicUIManager : Singleton<CinematicUIManager>
{
    [SerializeField] private Animator animator;

    public void ToggleBars(bool toggle) {
        if (toggle)
            animator.SetTrigger("bars_enable");
        else
            animator.SetTrigger("bars_disable");
    }
}
