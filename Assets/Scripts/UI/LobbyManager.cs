using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.Events;

public class LobbyManager : Singleton<LobbyManager>
{
    [SerializeField] private NetworkManager networkManager;
    private Callback<LobbyCreated_t> createCallback;
    private Callback<GameLobbyJoinRequested_t> joinCallback;
    private Callback<LobbyEnter_t> enterCallback;

    void Start()
    {
        if (!SteamManager.Initialized)
            return;

        HUDManager.Instance.Print("Steam linked successfully");
        createCallback = Callback<LobbyCreated_t>.Create(LobbyCreated);
        joinCallback = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoined);
        enterCallback = Callback<LobbyEnter_t>.Create(LobbyEntered);
    }

    public bool Host() {
        if (!SteamManager.Initialized)
            return false;
        HUDManager.Instance.Print("Hosting game");
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);

        return true;
    }

    private void LobbyCreated(LobbyCreated_t data) {
        if (data.m_eResult != EResult.k_EResultOK)
            return;

        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(data.m_ulSteamIDLobby), "CSID", SteamUser.GetSteamID().ToString());
    }

    private void LobbyJoined(GameLobbyJoinRequested_t data) {

        SteamMatchmaking.JoinLobby(data.m_steamIDLobby);

    }

    private void LobbyEntered(LobbyEnter_t data) {
        if (NetworkServer.active)
            return;

        string hostAddr = SteamMatchmaking.GetLobbyData(new CSteamID(data.m_ulSteamIDLobby), "CSID");

        networkManager.networkAddress = hostAddr;
        networkManager.StartClient();
        
        HUDManager.Instance.OnGameJoined();
    }

    
}
