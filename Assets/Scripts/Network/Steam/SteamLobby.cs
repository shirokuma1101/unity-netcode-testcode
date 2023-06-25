using Cysharp.Threading.Tasks;
using Netcode.Transports;
using Steamworks;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using Utility.Singleton;

/// <summary>
/// https://partner.steamgames.com/doc/features
/// </summary>
public class SteamLobby : LobbyBase
{
    // Singleton
    public static new SteamLobby Instance => SingletonAttacher<SteamLobby>.Instance;

    public ulong LobbyID { get; set; }
    public ELobbyType LobbyType { get; set; }

    private const string HOST_ADDRESS_KEY = "HostAddress";
    private CallResult<LobbyCreated_t> lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    private Callback<LobbyEnter_t> lobbyEnter;


    public override void CreateLobby()
    {
        SteamAPICall_t createLobby = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 0);
        lobbyCreated.Set(createLobby);
    }

    public override void JoinLobby()
    {
        SteamMatchmaking.JoinLobby((CSteamID)LobbyID);
    }

    public override void Setup(bool isApproval)
    {
        if (SteamManager.Instance.Initialized)
        {
            lobbyCreated = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            Payloads payload = new()
            {
                username = SteamFriends.GetPersonaName()
            };
            Payload = payload;
        }
        else
        {
            Debug.LogError("SteamManager is not initialized.");
        }

        base.Setup(isApproval);
    }

    private void OnCreateLobby(LobbyCreated_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_eResult != EResult.k_EResultOK || bIOFailure) return;

        // Set host address
        SteamMatchmaking.SetLobbyData(
            new CSteamID(pCallback.m_ulSteamIDLobby),
            HOST_ADDRESS_KEY,
            SteamUser.GetSteamID().ToString());

        LobbyID = pCallback.m_ulSteamIDLobby;

        // Start host
        StartHost();

        // Set lobby settings
        SteamMatchmaking.SetLobbyMemberLimit((CSteamID)LobbyID, MaxMembers);
        SteamMatchmaking.SetLobbyType((CSteamID)LobbyID, LobbyType);
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Failed to enter the lobby
        if ((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            var regex = new Regex(@"^k_EChatRoomEnterResponse");
            var response = regex.Replace(((EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse).ToString(), "");
            Debug.LogError($"Failed to enter the lobby. Response: {response}");
            return;
        }

        // Get host address
        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HOST_ADDRESS_KEY);

        // If the host address is the same as the local address, return
        if (hostAddress == SteamUser.GetSteamID().ToString()) return;

        LobbyID = callback.m_ulSteamIDLobby;

        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is SteamNetworkingSocketsTransport steamNetworkingSocketsTransport)
        {
            steamNetworkingSocketsTransport.ConnectToSteamID = ulong.Parse(hostAddress);
        }
        else
        {
            Debug.LogError("NetworkTransport is not SteamNetworkingSocketsTransport.");
        }

        // Start client
        StartClient();
    }
}
