using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class NetworkPlayerController : NetworkCharacterControllerBase
{
    [SerializeField]
    private GameObject followCameraPrefab;


    public override async void OnOwnerStart()
    {
        await UniTask.Delay(10 * 1000);
        var followCameraInst = Instantiate(followCameraPrefab, transform);
    }

    public override void OnUpdate()
    {
        Debug.Log("NetworkPlayerController.OnUpdate");
    }

    public class ActionStateBase : GenericNetworkStateMachine.StateBase
    {
        public new NetworkPlayerController Owner { get; private set; }


        public override void Initialize(GenericNetworkStateMachine genericNetworkStateMachine)
        {
            Owner = genericNetworkStateMachine.GetComponent<NetworkPlayerController>();
        }
    }

    [Serializable]
    public class ASPlayerIdle : ActionStateBase
    {
        public override void OnUpdate()
        {
            Debug.Log("ASPlayerIdle.OnUpdate");
        }
    }
}
