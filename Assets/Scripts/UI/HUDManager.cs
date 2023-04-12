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
            OnGameJoined();
    }
    
    public void Print(string output) {
        debugOutput.text = output;
    }

    public void OnGameJoined() {
        hostUI.SetActive(false);
        CinematicUIManager.Instance.ToggleBars(false);
    }
}
