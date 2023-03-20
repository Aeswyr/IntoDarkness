using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : Singleton<HUDManager>
{
    [SerializeField] private LobbyManager lobby;
    [SerializeField] private GameObject hostUI;
    [SerializeField] private TextMeshProUGUI debugOutput;

    public void HostPressed() {
        if (lobby.Host())
            hostUI.SetActive(false);
    }

    void FixedUpdate() {
        if (InputHandler.Instance.menu.pressed) {
            Debug.Log("Game Closing");
            Application.Quit();
        }
    }

    public void Print(string output) {
        debugOutput.text = output;
    }
}
