using UnityEngine;

public class PhysicsRig : MonoBehaviour
{
    public Transform playerHead;
    public Transform leftController;
    public Transform rightController;

    public Rigidbody playerRb;
    public Rigidbody leftRb;
    public Rigidbody rightRb;


    public ConfigurableJoint headJoint;
    public ConfigurableJoint leftHandJoint;
    public ConfigurableJoint rightHandJoint;


    public CapsuleCollider bodyCollider;
    public SphereCollider headCollider;
    [SerializeField]
    private LayerMask groundLayer;

    public float bodyHeightMin = 0.5f;
    public float bodyHeightMax = 2;

    // Update is called once per frame
    void FixedUpdate()
    {
        //bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        //bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height/2, playerHead.localPosition.z);
        float height =  Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        RaycastHit hit;
        if(!Physics.Raycast(headCollider.transform.position, Vector3.down,out hit, height + 0.05f,groundLayer, QueryTriggerInteraction.Ignore))
        {
            bodyCollider.height = 0;
            bodyCollider.transform.position = playerHead.transform.position;
        }
        else{
            bodyCollider.height = height;
            Vector3 temp = playerHead.position;
            temp.y = hit.point.y;
            bodyCollider.transform.position = temp;
            bodyCollider.center = new Vector3(0, bodyCollider.height/2, 0);
        }

        leftHandJoint.targetPosition = leftController.localPosition;
        leftHandJoint.targetRotation = leftController.localRotation;

        rightHandJoint.targetPosition = rightController.localPosition;
        rightHandJoint.targetRotation = rightController.localRotation;

        headJoint.targetPosition = playerHead.localPosition;

        EcholocationSingleton.Instance.SetLeftHandPosition(leftHandJoint.transform.position);
        EcholocationSingleton.Instance.SetRightHandPosition(rightHandJoint.transform.position);
        EcholocationSingleton.Instance.SetHeadPosition(headJoint.transform.position);
    }

    public void ApplyPlatformDelta(Vector3 delta)
    {
        playerRb.MovePosition(playerRb.position + delta);
        leftRb.MovePosition(leftRb.position + delta);
        rightRb.MovePosition(rightRb.position + delta);
    }
}
