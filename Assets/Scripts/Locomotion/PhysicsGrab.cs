using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsGrab : MonoBehaviour
{
    public InputActionProperty grabInputSource;
    [SerializeField] InputActionProperty pickupInputSource;
    [SerializeField] InputActionProperty selectUpInput;
    [SerializeField] InputActionProperty selectDownInput;
    public float radius = 0.1f;
    public LayerMask grabLayer;

    [SerializeField] private PhysicsContinousMovement movementScript;
    [SerializeField] private Animator anim;

    public bool isLeft = false;

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;
    private GameObject heldObject;

    private float timer = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;
        bool canGrab = movementScript != null ? movementScript.hasStamina : true;

        anim.SetFloat("Grip", grabInputSource.action.ReadValue<float>());

        if (isGrabButtonPressed && !isGrabbing && canGrab)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius, grabLayer, QueryTriggerInteraction.Ignore);

            if(nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidbody = nearbyColliders[0].attachedRigidbody;

                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = false;

                heldObject = nearbyColliders[0].gameObject;

                if (nearbyRigidbody)
                {
                    fixedJoint.connectedBody = nearbyRigidbody;
                    fixedJoint.connectedAnchor = nearbyRigidbody.transform.InverseTransformPoint(transform.position);
                }
                else
                {
                    fixedJoint.connectedAnchor = transform.position;
                }

                if(HapticSingleton.Instance != null)
                {
                    if(isLeft) HapticSingleton.Instance.SendLeftImpulse(0.65f, 0.1f);
                    else HapticSingleton.Instance.SendRightImpulse(0.65f, 0.1f);
                }

                isGrabbing = true;
            }
        }
        else if((!isGrabButtonPressed || !canGrab) && isGrabbing)
        {
            LetGo();
        }

        Pickup p;
        if(isGrabbing && heldObject.TryGetComponent<Pickup>(out p) && pickupInputSource.action.ReadValue<float>() > 0.5 && timer < 0)
        {
            Inventory.Instance.AddItem(p);
            if(HapticSingleton.Instance != null)
            {
                if(isLeft) HapticSingleton.Instance.SendLeftImpulse(0.5f, 0.15f);
                else HapticSingleton.Instance.SendRightImpulse(1f, 0.15f);
            }
            LetGo();
            timer = 0.5f;
            return;
        }

        if(isGrabButtonPressed && !isGrabbing && pickupInputSource.action.ReadValue<float>() > 0.5 && timer < 0)
        {
            timer = 0.5f;
            Inventory.Instance.GetSelectedItem(transform.position);

            if(HapticSingleton.Instance != null)
            {
                if(isLeft) HapticSingleton.Instance.SendLeftImpulse(1f, 0.15f);
                else HapticSingleton.Instance.SendRightImpulse(1f, 0.15f);
            }
        }

        //updating selection
        if(selectUpInput.action.ReadValue<float>() > 0.5)
        {
            Inventory.Instance.UpdateSelectedItem(-1);
            return;
        }

        if(selectDownInput.action.ReadValue<float>() > 0.5)
        {
            Inventory.Instance.UpdateSelectedItem(1);
        }
    }

    void LetGo()
    {
        isGrabbing = false;
        if(fixedJoint) Destroy(fixedJoint);
    }
}