using Cysharp.Threading.Tasks;
using NGOManager;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject playerPrefab;


    public override void OnNetworkSpawn()
    {
        NetworkObjectSpawner.SpawnAsPlayerObjectAsync(playerPrefab, NetworkManager.Singleton.LocalClientId).Forget();
    }
}
