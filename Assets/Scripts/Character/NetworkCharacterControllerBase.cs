using UnityEngine;

public class NetworkCharacterControllerBase : NetworkObjectBase
{
    public Vector3 Velocity { get; protected set; }

    protected Animator animator;
    protected Rigidbody rb;
}
