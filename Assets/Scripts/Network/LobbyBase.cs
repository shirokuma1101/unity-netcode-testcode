using Cysharp.Threading.Tasks;
using NGOManager.Utility.Singleton;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// An abstract class that summarizes common processing when implementing lobbies
/// Since MonoBehaviour is inherited, it can be inherited as it is.
/// </summary>
[Serializable]
public abstract class LobbyBase : SingletonNetworkPersistent<LobbyBase>
{
    [Serializable]
    public struct Payloads
    {
        public string username;
        public string password;
    }

    public const int MIN_MEMBERS = 2;
    public const int MAX_MEMBERS = 16;

    /// <summary>
    /// Max number of members in the lobby.
    /// Throws an "ArgumentOutOfRangeException" if the number is set to less than the number of clients already connected.
    /// Also, values less than 2 or greater than 16 are clamped.
    /// </summary>
    public int MaxMembers
    {
        get => maxMembers;
        set
        {
            if (NetworkManager.Singleton && IsHost)
            {
                if (NetworkManager.Singleton.ConnectedClients.Count > value)
                {
                    throw new ArgumentOutOfRangeException(nameof(maxMembers), "MaxMembers must be greater than the number of connected clients.");
                }
            }
            maxMembers = ClampMaxMembers(value);
        }
    }
    /// <summary>
    /// Payload used when connecting to the host.
    /// </summary>
    public Payloads Payload { get; set; }
    /// <summary>
    /// List of usernames of clients connected to the host.
    /// </summary>
    public SerializedDictionary<ulong, string> Usernames { get; private set; } = new();
    /// <summary>
    /// Disconnect scene to be loaded when disconnected from the host.
    /// </summary>
    public LoadingSceneManager.SceneName DisconnectScene { get; set; } = LoadingSceneManager.SceneName.Title;

    private int maxMembers = MIN_MEMBERS;


    public abstract void CreateLobby();
    public abstract void JoinLobby();

    public virtual void Setup(bool isApproval)
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(Payload));
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        if (isApproval && NetworkManager.Singleton.ConnectionApprovalCallback == null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }
    }

    public virtual void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public virtual void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public virtual void DisconnectClient(ulong clientId)
    {
        DisconnectClientServerRpc(clientId);
    }

    public static int ClampMaxMembers(int members)
    {
        if (members < MIN_MEMBERS)
        {
            return MIN_MEMBERS;
        }
        else if (members > MAX_MEMBERS)
        {
            return MAX_MEMBERS;
        }
        return members;
    }

    protected virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // True until the connection is approved
        response.Pending = true;

        string username;

        // Limit max members
        if (NetworkManager.Singleton.ConnectedClients.Count >= MaxMembers)
        {
            response.Approved = false;
            response.Pending = false;
            response.Reason = "Too many players";
            return;
        }
        // Check if the scene is MatchMaking
        if (LoadingSceneManager.Instance.ActiveSceneInLobby != LoadingSceneManager.SceneName.MatchMaking)
        {
            response.Approved = false;
            response.Pending = false;
            response.Reason = "Not MatchMaking Scene";
            return;
        }
        // Get Payload
        if (request.Payload.Length != 0)
        {
            var payloads = JsonUtility.FromJson<Payloads>(System.Text.Encoding.UTF8.GetString(request.Payload));

            // Payload validation

            // Check if the username is empty
            if (string.IsNullOrEmpty(payloads.username))
            {
                payloads.username = $"Player{request.ClientNetworkId}";
            }
            if (string.IsNullOrEmpty(Payload.password) == false)
            {
                if (string.IsNullOrEmpty(payloads.password))
                {
                    response.Approved = false;
                    response.Pending = false;
                    response.Reason = "Password is empty";
                    return;
                }
                if (Payload.password != payloads.password)
                {
                    response.Approved = false;
                    response.Pending = false;
                    response.Reason = "Password is incorrect";
                    return;
                }
            }

            username = payloads.username;
        }
        else
        {
            response.Approved = false;
            response.Pending = false;
            response.Reason = "Payload is empty";
            return;
        }

        // Accept the connection
        response.Approved = true;

        // Add username to the list
        Usernames.Add(request.ClientNetworkId, username);

        // Set the player object to be created on the client
        response.CreatePlayerObject = false;
        // Prefab hash value of the PlayerObject to create.
        // If null, Prefab registered in NetworkManager is used.
        response.PlayerPrefabHash = null;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;

        // Connection approved
        response.Pending = false;
    }

    protected virtual void OnClientConnected(ulong clientId)
    {
        UsernamesClientRpc(JsonUtility.ToJson(Usernames));
    }

    protected virtual void OnClientDisconnect(ulong clientId)
    {
        if (string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason) == false)
        {
            Debug.Log(NetworkManager.Singleton.DisconnectReason);
        }
        if (IsHost && clientId != NetworkManager.Singleton.LocalClientId)
        {
            Usernames.Remove(clientId);
            UsernamesClientRpc(JsonUtility.ToJson(Usernames));
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.Shutdown();
            LoadingSceneManager.Instance.LoadSceneAsync(DisconnectScene, LoadSceneMode.Single, false).Forget();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisconnectClientServerRpc(ulong clientId)
    {
        ClientRpcParams clientRpcParams = new()
        {
            Send = new()
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ShutdownClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void ShutdownClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnClientDisconnect(NetworkManager.Singleton.LocalClientId);
    }

    [ClientRpc]
    private void UsernamesClientRpc(string serializedUsernames)
        => Usernames = JsonUtility.FromJson<SerializedDictionary<ulong, string>>(serializedUsernames);
}
