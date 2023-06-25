using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Utility.Singleton;

public class DirectLobby : LobbyBase
{
    //TODO: Set username

    // Singleton
    public static new DirectLobby Instance => SingletonAttacher<DirectLobby>.Instance;

    public const string DEFAULT_IP_ADDRESS = "127.0.0.1";
    public const ushort DEFAULT_PORT = 7777;

    public string IPAddress { get; set; } = DEFAULT_IP_ADDRESS;
    public ushort Port { get; set; } = DEFAULT_PORT;


    public override void CreateLobby()
    {
        StartHost();
    }

    public override void JoinLobby()
    {
        StartClient();
    }

    public override void Setup(bool isApproval)
    {
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport unityTransport)
        {
            unityTransport.SetConnectionData(IPAddress, Port);
        }
        else
        {
            Debug.LogError("NetworkTransport is not UnityTransport.");
        }

        Payloads payload = new()
        {
            username = ""
        };
        Payload = payload;

        base.Setup(isApproval);
    }
}
