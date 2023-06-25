using NetcodeUtility;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject playerPrefab;


    public override void OnNetworkSpawn()
    {
        NetworkObjectSpawner.SpawnAsPlayerObject(playerPrefab, NetworkManager.Singleton.LocalClientId);
    }
}
