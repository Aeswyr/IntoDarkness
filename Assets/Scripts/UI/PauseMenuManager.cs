using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManager : Singleton<PauseMenuManager>
{
    [SerializeField] private GameObject menuHolder;

    public void ToggleVisibility(bool state) {
        menuHolder.SetActive(state);
    }
    
    public void OnPressed() {
        Debug.Log("Closing the game");
        Application.Quit();
    }
}
