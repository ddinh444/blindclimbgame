using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
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

    [SerializeField] private float fallDmgVelocityThreshold = 12f;
    [SerializeField] private float slowMultiplier = 0.85f;
    [SerializeField] private float fallDmgPenaltyDuration = 15f;
    [SerializeField] private float fallDmgStaminaDuration = 10f;
    [SerializeField] private float fallDmgTimeToStartBreathingHeavy = 5f;
    [SerializeField] private AudioSource fallDmgHurtNoise;
    private float penaltyTimer = 0f;
    private float lastVerticalVelocity = 0f;
    private bool wasGroundedLastFrame = true;

    [SerializeField] private float staminaDuration = 50.0f;
    [SerializeField] private float timeToStartBreathingHeavy = 40.0f;
    [SerializeField] private AudioSource heavyBreath;
    [SerializeField] private HapticImpulsePlayer leftControllerHaptics;
    [SerializeField] private HapticImpulsePlayer rightControllerHaptics;
    public bool hasStamina { get; private set; } = true;
    private float airTimer = 0f;

    private Camera cam;
    private Rigidbody playerRigidbody;
    [SerializeField]
    private CapsuleCollider bodyCollider;
    [SerializeField]private LayerMask groundLayer;

    void Start()
    {
        cam = Camera.main;
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {   
        bool grounded = IsGrounded();

        //check for fall damage
        if(grounded && !wasGroundedLastFrame)
        {
            HandleLanding(lastVerticalVelocity);
        }

        float currentStaminaDuration = staminaDuration;
        float heavyBreathTime = timeToStartBreathingHeavy;

        //hurt from fall damage
        if(penaltyTimer > 0)
        {
            penaltyTimer -= Time.fixedDeltaTime;
            currentStaminaDuration = fallDmgStaminaDuration;
            heavyBreathTime = fallDmgTimeToStartBreathingHeavy;
        }

        //determine control of movement and air timer
        float control = 1;
        if (!grounded)
        {
            control = airControl;

            airTimer += Time.fixedDeltaTime;
        }
        else
        {
            airTimer = 0;
        }

        //check for stamina depletion
        if(airTimer >= currentStaminaDuration)
        {
            hasStamina = false;
        }
        else
        {
            hasStamina = true;
        }

        //about to run out of stamina
        if(airTimer >= heavyBreathTime || penaltyTimer > 0)
        {
            if (!heavyBreath.isPlaying)
            {
                heavyBreath.Play();
            }
            //leftControllerHaptics.SendHapticImpulse(0.25f, 0.5f);
            //rightControllerHaptics.SendHapticImpulse(0.25f, 0.5f);
            if(HapticSingleton.Instance != null)
            {
                HapticSingleton.Instance.SendBothImpulses(0.25f, 0.5f);
            }
        }
        else
        {
            heavyBreath.Stop();
        }

        //movement
        Vector3 moveDir = ComputeDesiredMoveDirection(LeftHandMoveInput.ReadValue());
        float weightNormalized = Mathf.InverseLerp(0, 10, Inventory.Instance.GetTotalWeight());
        float weightMultiplier = 1 - Mathf.Lerp(0,0.65f, weightNormalized);
        Vector3 targetSpeed = penaltyTimer > 0 ? moveDir * maxSpeed : moveDir * maxSpeed * slowMultiplier * weightMultiplier;
        Vector3 velDiff = targetSpeed - playerRigidbody.linearVelocity;
        velDiff.y = 0;
        playerRigidbody.AddForce(velDiff * control * acceleration, ForceMode.Acceleration);

        lastVerticalVelocity = playerRigidbody.linearVelocity.y;
        wasGroundedLastFrame = grounded;
    }

    void HandleLanding(float impactVelocity)
    {
        if(Math.Abs(impactVelocity) > fallDmgVelocityThreshold)
        {
            penaltyTimer = fallDmgPenaltyDuration;
            fallDmgHurtNoise.Play();
            GameEvents.TriggerFallDmgNoise(transform.position);
            EcholocationSingleton.Instance.AddSound(transform.position);
            if(HapticSingleton.Instance != null)
            {
                HapticSingleton.Instance.SendBothImpulses(1, 0.3f, 1000);
            }
        }

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
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);
        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.1f;
        bool hasHit = Physics.SphereCast(start, bodyCollider.radius, Vector3.down, out RaycastHit info, rayLength, groundLayer);
        Debug.DrawRay(start, Vector3.down * rayLength, Color.green, Time.fixedDeltaTime);
        return hasHit;
    }
}
