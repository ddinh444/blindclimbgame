using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class PhysicsContinousMovement : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Reads input data from the left hand controller. Input Action must be a Value action type (Vector 2).")]
    XRInputValueReader<Vector2> LeftHandMoveInput = new XRInputValueReader<Vector2>("Left Hand Move");
    [SerializeField]
    [Tooltip("Max speed the player can go.")]
    private float maxSpeed = 3.5f;
    [SerializeField]
    [Tooltip("How fast the player is able to reach their max speed.")]
    private float acceleration = 2f;
    [SerializeField]
    [Range(0,1)]
    [Tooltip("How much control the player has in the air. 0 = no control at all, 1 = same amount of control as on the ground.")]
    private float airControl = 0.5f;
    [SerializeField]
    [Tooltip("How far from the player's position the ground checking ray goes.")]
    private float groundCheckRayDistance = 0.25f;

    private Camera cam;
    private Rigidbody playerRigidbody;
    [SerializeField]
    [Tooltip("Collider that the player uses. This script moves the collider to follow the camera's x and z coordinates so that moving in real life affects the in-game collider position.")]
    private Collider playerCollider;

    void Start()
    {
        cam = Camera.main;
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {   
        Vector3 temp = cam.transform.position;
        temp.y = playerCollider.transform.position.y;
        playerCollider.transform.position = temp;

        float control = 1;
        if (!IsGrounded())
        {
            control = airControl;
        }

        Vector3 moveDir = ComputeDesiredMoveDirection(LeftHandMoveInput.ReadValue());
        Vector3 targetSpeed = moveDir * maxSpeed;
        Vector3 velDiff = targetSpeed - playerRigidbody.linearVelocity;
        velDiff.y = 0;
        playerRigidbody.AddForce(velDiff * acceleration * control, ForceMode.Acceleration);
    }

    Vector3 ComputeDesiredMoveDirection(Vector2 input)
    {
        Vector2 inputMove = Vector2.ClampMagnitude(input, 1f);

        Transform inputSource = cam.transform;
        Vector3 forward = inputSource.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = inputSource.right;
        right.y = 0;
        right.Normalize();

        Vector3 moveDirection = Vector3.ClampMagnitude(inputMove.y * forward + inputMove.x * right, 1);
        return moveDirection;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1 + groundCheckRayDistance);
    }
}
