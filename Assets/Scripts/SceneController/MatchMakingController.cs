using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.EnumEx;

public class MatchMakingController : NetworkBehaviour
{
    //TODO: Kick button

    [Serializable]
    struct PlayerInfos
    {
        public bool isReady;
        //public role;
    }

    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject textPlayerListObj;
    [SerializeField]
    private GameObject textPlayerPrefab;
    [SerializeField]
    private GameObject buttonStartObj;
    [SerializeField]
    private GameObject buttonReadyObj;
    [SerializeField]
    private GameObject buttonExitObj;

    private readonly Dictionary<ulong, GameObject> textPlayers = new();
    private SerializedDictionary<ulong, PlayerInfos> playerInfos = new();


    private void Awake()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnFirstClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void Start()
    {
        buttonExitObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            LobbySystem.Instance.CurrentLobby.DisconnectScene = LoadingSceneManager.SceneName.Title;
            LobbySystem.Instance.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        });
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void RegenerateTextPlayers()
    {
        // 一度全て削除
        foreach (var textPlayer in textPlayers)
        {
            Destroy(textPlayer.Value);
        }
        textPlayers.Clear();

        // 再生成
        foreach (var (username, i) in LobbySystem.Instance.CurrentLobby.Usernames.Indexed())
        {
            var textPlayerInst = Instantiate(textPlayerPrefab, canvas.transform);
            textPlayerInst.transform.position = new(
                textPlayerListObj.transform.position.x - 100,
                textPlayerListObj.transform.position.y - 40 - (40 * i),
                0);
            textPlayerInst.GetComponent<TextMeshProUGUI>().text = $"・{username.Value}";
            textPlayers.Add(username.Key, textPlayerInst);
        }
    }

    private void UpdateUI(ulong clientId)
    {
        foreach (var isReadyPlayer in playerInfos)
        {
            textPlayers[isReadyPlayer.Key].GetComponent<TextMeshProUGUI>().color
                = isReadyPlayer.Value.isReady
                ? Color.green
                : Color.red;
        }

        if (IsHost)
        {
            //buttonStartObj.GetComponent<Button>().interactable = isReadyPlayers.All(x => x.Value);
            buttonStartObj.GetComponent<Image>().color
                = playerInfos.All(x => x.Value.isReady)
                ? Color.green
                : Color.red;
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            buttonReadyObj.GetComponentInChildren<TextMeshProUGUI>().text
                = playerInfos[clientId].isReady
                ? "Ready"
                : "Not Ready";
            buttonReadyObj.GetComponent<Image>().color
                = playerInfos[clientId].isReady
                ? Color.green
                : Color.red;
        }
    }

    /// <summary>
    /// The callback to invoke once a client connects. This callback is only ran on the server and on the local client that connects.
    /// </summary>
    /// <param name="cliendId"></param>
    private void OnFirstClientConnected(ulong clientId)
    {
        if (IsHost)
        {
            buttonStartObj.SetActive(true);
            buttonReadyObj.SetActive(false);

            buttonStartObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (playerInfos.All(x => x.Value.isReady) == false) return;
                LoadingSceneManager.Instance.LoadSceneAsync(LoadingSceneManager.SceneName.Game, LoadSceneMode.Single, true).Forget();
            });
        }
        else
        {
            buttonStartObj.SetActive(false);
            buttonReadyObj.SetActive(true);

            buttonReadyObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayerInfos playerinfo = new()
                {
                    isReady = !playerInfos[clientId].isReady
                };
                playerInfos[clientId] = playerinfo;
                PlayerInfoBroadcast(clientId, JsonUtility.ToJson(playerInfos[clientId]));
            });
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnFirstClientConnected;
    }

    /// <summary>
    /// The callback to invoke once a client connects. This callback is only ran on the server and on the local client that connects.
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientConnected(ulong clientId)
    {
        if (IsHost == false) return;

        OnClientConnectedClientRpc();
        PlayerInfos playerInfo = new()
        {
            isReady = clientId == OwnerClientId
        };
        playerInfos.Add(clientId, playerInfo);
        PlayerInfosClientRpc(JsonUtility.ToJson(playerInfos));
    }

    /// <summary>
    /// The callback to invoke when a client disconnects. This callback is only ran on the server and on the local client that disconnects.
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientDisconnect(ulong clientId)
    {
        if (IsHost == false) return;

        OnClientDisconnectClientRpc();
        playerInfos.Remove(clientId);
        PlayerInfosClientRpc(JsonUtility.ToJson(playerInfos));

        if (clientId == OwnerClientId)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    [ClientRpc]
    private void OnClientConnectedClientRpc()
    {
        RegenerateTextPlayers();
    }

    [ClientRpc]
    private void OnClientDisconnectClientRpc()
    {
        RegenerateTextPlayers();
    }

    /// <summary>
    /// Host only. Share all IsReadyPlayers that the host has when connecting to the client.
    /// </summary>
    /// <param name="serializedIsReadyPlayers"></param>
    [ClientRpc]
    private void PlayerInfosClientRpc(string serializedPlayerInfos)
    {
        playerInfos = JsonUtility.FromJson<SerializedDictionary<ulong, PlayerInfos>>(serializedPlayerInfos);
        UpdateUI(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// All clients. Broadcast PlayerInfos for client id.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="playerinfo"></param>
    private void PlayerInfoBroadcast(ulong clientId, string serializedPlayerinfo) => PlayerInfoServerRpc(clientId, serializedPlayerinfo);
    [ServerRpc(RequireOwnership = false)]
    private void PlayerInfoServerRpc(ulong clientId, string serializedPlayerinfo) => PlayerInfoClientRpc(clientId, serializedPlayerinfo);
    [ClientRpc]
    private void PlayerInfoClientRpc(ulong clientId, string serializedPlayerinfo)
    {
        playerInfos[clientId] = JsonUtility.FromJson<PlayerInfos>(serializedPlayerinfo);
        UpdateUI(clientId);
    }
}
