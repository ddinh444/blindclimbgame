using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

/// <summary>
/// This script controls a physics hand separate from the XR rig 
/// that follows the actual controller position/rotation.
/// It applies a spring force to the player based off the diff
/// btwn the real controller position and the physics hand position
/// so that if the player pushes against a wall in game, they are pushed
/// back. Code derived from https://www.youtube.com/watch?v=5D2bN7xL5us
/// </summary>
public class PhysicsHand : MonoBehaviour
{
    [Header("Hand Controls")]
    Rigidbody handRigidbody;
    [SerializeField] Transform controllerTransform;
    [SerializeField] float frequency = 50f;
    [SerializeField] float damping = 1f;
    [SerializeField] float rotFrequency = 100f;
    [SerializeField] float rotDamping = 0.9f;
    [Header("Player Pushback Controls")]
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] float springiness = 50f;
    [SerializeField] float drag = 25f;
    [Header("Grabbing")]
    [SerializeField] string grabTag = "GrabPoint";
    [SerializeField] XRInputValueReader<float> grabInput;
    bool isGrabbing = false;

    bool isColliding = false;
    float maxFrictionOfCollisions = 0f;

    
    void Start()
    {
        transform.position = controllerTransform.position;
        transform.rotation = controllerTransform.rotation;
        handRigidbody = GetComponent<Rigidbody>();
        handRigidbody.maxAngularVelocity = float.PositiveInfinity;
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PIDMovement();
        PIDRotation();


        if (isColliding)
        {
            
            ApplyPushToPlayer();
        }

    }

    void PIDMovement()
    {
        float kp = (6f * frequency) * (6f * frequency) * 0.25f;
        float kd = 4.5f * frequency * damping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Vector3 force = (controllerTransform.position - transform.position) * ksg + (playerRigidbody.linearVelocity - handRigidbody.linearVelocity) * kdg;
        handRigidbody.AddForce(force, ForceMode.Acceleration);
    }

    void PIDRotation()
    {
        float kp = (6f * rotFrequency) * (6f * rotFrequency) * 0.25f;
        float kd = 4.5f * rotFrequency * rotDamping;
        float g = 1f / (1f + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;

        Quaternion q = controllerTransform.rotation * Quaternion.Inverse(transform.rotation);
        if (q.w < 0f)
            q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
        q.ToAngleAxis(out float angle, out Vector3 axis);
        // Bail early if rotation is effectively zero
        if (angle < 0.001f || axis.sqrMagnitude < 1e-6f)
            return;
        axis.Normalize();
        angle *= Mathf.Deg2Rad;

        Vector3 torque = axis * angle * ksg - handRigidbody.angularVelocity * kdg;
        handRigidbody.AddTorque(torque, ForceMode.Acceleration);
    }

    void ApplyPushToPlayer()
    {
        Vector3 displacement = transform.position - controllerTransform.position;
        Vector3 force = displacement * springiness;
        float dragCoefficient = GetDrag();
        float frictionCoefficient = maxFrictionOfCollisions;
        Vector3 springForce = force * frictionCoefficient;
        Vector3 dragForce = dragCoefficient * -playerRigidbody.linearVelocity * drag * frictionCoefficient;

        playerRigidbody.AddForce(springForce, ForceMode.Acceleration);
        playerRigidbody.AddForce(dragForce, ForceMode.Acceleration);
    }

    Vector3 previousPosition;
    float GetDrag()
    {
        Vector3 handVelocity = (controllerTransform.localPosition - previousPosition) / Time.fixedDeltaTime;
        float drag = 1 / handVelocity.magnitude + 0.01f;
        drag = Mathf.Clamp(drag, 0.03f, 1);
        previousPosition = transform.position;
        return drag;
    }

    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;

        if (collision.collider.material.dynamicFriction > maxFrictionOfCollisions)
        {
            maxFrictionOfCollisions = collision.collider.material.dynamicFriction;
        }
    }

    private void StartGrab()
    {
        isGrabbing = true;
        handRigidbody.isKinematic = true;
    }

    private void EndGrab()
    {
        isGrabbing = false;
        handRigidbody.isKinematic = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        maxFrictionOfCollisions = 0.0f;
    }
}
