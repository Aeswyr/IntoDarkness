using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestPointController : MonoBehaviour
{
    bool interacting;

    public void OnInteract(PlayerController interactor) {
        if (interacting) {
            interacting = false;
            interactor.ToggleRest(interacting, this);
            CinematicUIManager.Instance.ToggleBars(interacting);
            PlayerHUDManager.Instance.ToggleHUD(!interacting);
        } else {
            interacting = true;
            interactor.ToggleRest(interacting, this);
            CinematicUIManager.Instance.ToggleBars(interacting);
            PlayerHUDManager.Instance.ToggleHUD(!interacting);
        }
    }
}
