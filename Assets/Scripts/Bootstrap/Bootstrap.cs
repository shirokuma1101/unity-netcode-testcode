using NGOManager.Utility.Singleton;
using System;
using UnityEngine;
using Utility.EnumEx;

public class Bootstrap : SingletonPersistent<Bootstrap>
{
    public enum Platforms
    {
        Direct         = AvailableConnectionType.Direct,
        Epic           = AvailableConnectionType.Direct | AvailableConnectionType.Epic,
        NintendoSwitch = AvailableConnectionType.NintendoSwitch,
        PlayStation4   = AvailableConnectionType.PlayStation4,
        PlayStation5   = AvailableConnectionType.PlayStation5,
        Steam          = AvailableConnectionType.Direct | AvailableConnectionType.Steam,
        Xbox           = AvailableConnectionType.Direct | AvailableConnectionType.Xbox,
        //todo: Android, iOS
    }

    public enum AvailableConnectionType
    {
        Direct         = 1 << 0,
        Epic           = 1 << 1,
        NintendoSwitch = 1 << 2,
        PlayStation4   = 1 << 3,
        PlayStation5   = 1 << 4,
        Steam          = 1 << 5,
        Xbox           = 1 << 6,
        //todo: Android, iOS
    }

    [field: SerializeField]
    public Platforms Platform { get; private set; } = Platforms.Direct;

    public GameObject NetworkManager { get; private set; }

    [SerializeField]
    private GameObject networkManagerPrefab;


    /// <summary>
    /// Network API Initialization
    /// </summary>
    public void InitializeNetworkAPI(int platformsIndex)
    {
        if (NetworkManager != null)
        {
            throw new Exception("NetworkManager is already initialized!");
        }

        NetworkManager = Instantiate(networkManagerPrefab);

        foreach (var (e, i) in ((Platforms[])Enum.GetValues(typeof(Platforms))).Indexed())
        {
            if (i == platformsIndex)
            {
                Platform = e;
                break;
            }
        }

        switch (Platform)
        {
            case Platforms.Direct:
                break;
            case Platforms.Steam:
                NetworkManager.AddComponent<SteamAPIManager>();
                break;
        }
    }
}
