using NGOManager;
using System;
using UnityEngine;

public class NetworkPlayerController : NetworkCharacterControllerBase
{
    [SerializeField]
    private GameObject followCameraPrefab;


    public override void OnOwnerStart()
    {
        base.OnOwnerStart();

        var followCameraInst = Instantiate(followCameraPrefab, transform);
    }

    public override void OnOwnerFixedUpdate()
    {
        InputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        animator.SetFloat("Speed", InputVector.z);
        animator.SetFloat("Direction", InputVector.x);

        Velocity = new Vector3(0, 0, InputVector.z);
        Velocity = transform.TransformDirection(Velocity);
        if (InputVector.z > 0.1)
        {
            Velocity *= forwardSpeed;
        }
        else if (InputVector.z < -0.1)
        {
            Velocity *= backwardSpeed;
        }

        transform.position += Velocity * Time.fixedDeltaTime;
        transform.Rotate(0, InputVector.x * turnSpeed * Time.fixedDeltaTime, 0);
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
