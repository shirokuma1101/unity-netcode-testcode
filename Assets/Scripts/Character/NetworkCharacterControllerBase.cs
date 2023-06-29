using NGOManager;
using UnityEngine;

public class NetworkCharacterControllerBase : NetworkObjectBase
{
    public Vector3 InputVector { get; protected set; }
    public Vector3 Velocity { get; protected set; }

    protected Animator animator;
    protected Rigidbody rb;

    [SerializeField]
    protected float animationSpeed = 1.0f;
    [SerializeField]
    protected float forwardSpeed = 1.0f;
    [SerializeField]
    protected float backwardSpeed = 1.0f;
    [SerializeField]
    protected float turnSpeed = 1.0f;


    public override void OnStart()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        animator.speed = animationSpeed;
    }
}
