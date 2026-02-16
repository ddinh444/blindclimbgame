using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class SnapTurn : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Reads input data from the right hand controller. Input Action must be a Value action type (Vector 2).")]
    XRInputValueReader<Vector2> RightHandTurnInput = new XRInputValueReader<Vector2>("Right Hand Snap Turn");
    [SerializeField]
    [Tooltip("Rotated around the pivot transform.")]
    Transform xrOrigin;
    [SerializeField]
    [Tooltip("The transform whose position will be pivoted around, usually the camera. Needed because the xr origin does not follow the position of the HMD, so rotating the xr origin will make the HMD/controllers pivot around it instead rotating locally.")]
    Transform pivotTransform;
    [SerializeField]
    [Tooltip("How much the player is turned per snap turn.")]
    private float turnAngle = 45;
    [SerializeField]
    [Tooltip("Wait time between snap turns if the player is holding the joystick down.")]
    private float timeBetweenTurns = 0.5f;
    private float timer = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        float turnCoefficient = GetTurnCoefficient(RightHandTurnInput.ReadValue().normalized);
        
        //if player is not holding down the joystick then reset internal turn timer
        if(turnCoefficient == 0)
        {
            timer = 0;
        }
    }

    void FixedUpdate()
    {
        float turnCoefficient = GetTurnCoefficient(RightHandTurnInput.ReadValue().normalized);
        if (timer < 0)
        {
            float turnAmount = turnAngle * turnCoefficient;
            Vector3 pivot = pivotTransform.position;
            pivot.y = xrOrigin.position.y;
            xrOrigin.RotateAround(pivot, Vector3.up, turnAmount);
            timer = timeBetweenTurns;
        }
    }

    private float GetTurnCoefficient(Vector2 input)
    {
        float dot = Vector2.Dot(input, Vector2.right);
        if(dot > 0.65f)
        {
            return 1;
        }
        if(dot < -0.65f)
        {
            return -1;
        }
        return 0;
    }
}
