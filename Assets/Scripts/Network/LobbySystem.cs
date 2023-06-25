using Netcode.Transports;
using RapidGUI;
using System;
using Unity.Multiplayer.Tools.NetStatsMonitor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Utility.Singleton;

public class LobbySystem : Singleton<LobbySystem>
{
    public Bootstrap.AvailableConnectionType ConnectionType
    {
        get => connectionType;
        set
        {
            if (TransportApplied)
            {
                throw new Exception("Transport is applied!");
            }
            connectionType = value;
        }
    }
    public LobbyBase CurrentLobby { get; private set; }
    public bool TransportApplied { get; private set; } = false;

    private Bootstrap.AvailableConnectionType connectionType = Bootstrap.AvailableConnectionType.Direct;

    /* Debug */
    [SerializeField]
    private NetStatsMonitorConfiguration netStatsMonitorConfiguration;

    private RuntimeNetStatsMonitor runtimeNetStatsMonitor;
    private UnityTransport.SimulatorParameters simulatorParameters;


    private void Start()
    {
        DebugGUIManager.Instance.Launchers.Add("Network Debug", () =>
        {
            using (new GUILayout.HorizontalScope(GUILayout.MinWidth(200.0f)))
            {
                if (ConnectionType == Bootstrap.AvailableConnectionType.Direct)
                {
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Packet (Unity Transport Only)");
                        if (CurrentLobby == null)
                        {
                            simulatorParameters.PacketDelayMS = RGUI.Field(simulatorParameters.PacketDelayMS, "> DelayMS");
                            simulatorParameters.PacketJitterMS = RGUI.Field(simulatorParameters.PacketJitterMS, "> JitterMS");
                            simulatorParameters.PacketDropRate = RGUI.Field(simulatorParameters.PacketDropRate, "> DropRate");
                        }
                        else
                        {
                            GUILayout.Label("> DelayMS: " + simulatorParameters.PacketDelayMS);
                            GUILayout.Label("> JitterMS: " + simulatorParameters.PacketJitterMS);
                            GUILayout.Label("> DropRate: " + simulatorParameters.PacketDropRate);
                        }
                    }
                }
                if (ConnectionType == Bootstrap.AvailableConnectionType.Steam)
                {
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Steam Info");
                        if (CurrentLobby == null)
                        {

                        }
                        else
                        {
                            if (GUILayout.Button("> Copy Lobby ID"))
                            {
                                GUIUtility.systemCopyBuffer = SteamLobby.Instance.LobbyID.ToString();
                            }
                        }
                    }
                }
            }
        });

        runtimeNetStatsMonitor = gameObject.AddComponent<RuntimeNetStatsMonitor>();
        runtimeNetStatsMonitor.Configuration = netStatsMonitorConfiguration;
        runtimeNetStatsMonitor.Position.PositionLeftToRight = 1;
        runtimeNetStatsMonitor.ApplyConfiguration();
    }

    public void Setup(bool isApproval = false)
    {
        CurrentLobby.Setup(isApproval);
    }

    public void StartHost()
    {
        CurrentLobby.CreateLobby();
    }

    public void StartClient()
    {
        CurrentLobby.JoinLobby();
    }

    public void DisconnectClient(ulong clientID)
    {
        CurrentLobby.DisconnectClient(clientID);
    }

    public void ApplyTransport()
    {
        if (TransportApplied)
        {
            throw new Exception("Transport is applied!");
        }
        TransportApplied = true;

        if (ConnectionType == 0)
        {
            ConnectionType = (Bootstrap.AvailableConnectionType)Bootstrap.Instance.Platform;
        }

        switch (ConnectionType)
        {
            case Bootstrap.AvailableConnectionType.Direct:
                var ut = Bootstrap.Instance.NetworkManager.AddComponent<UnityTransport>();
                Bootstrap.Instance.NetworkManager.GetComponent<NetworkManager>().NetworkConfig.NetworkTransport = ut;
                ut.SetDebugSimulatorParameters(simulatorParameters.PacketDelayMS, simulatorParameters.PacketJitterMS, simulatorParameters.PacketDropRate);
                CurrentLobby = gameObject.AddComponent<DirectLobby>();
                break;
            case Bootstrap.AvailableConnectionType.Steam:
                var st = Bootstrap.Instance.NetworkManager.AddComponent<SteamNetworkingSocketsTransport>();
                Bootstrap.Instance.NetworkManager.GetComponent<NetworkManager>().NetworkConfig.NetworkTransport = st;
                CurrentLobby = gameObject.AddComponent<SteamLobby>();
                break;
        }
    }
}
