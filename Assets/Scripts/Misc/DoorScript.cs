using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

public class DoorScript : MonoBehaviour
{
    const float speed = 10f;
    [SerializeField] private Vector3 closedPosition;
    [SerializeField] private Vector3 openedPosition;
    [SerializeField] private AudioSource openSound;

    private bool isOpen;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = closedPosition;
        isOpen = false;
    }

    public void Open()
    {
        if (isOpen)
        {
            return;
        }
        Debug.Log("Open sesame");
        openSound.Play();
        EcholocationSingleton.Instance.AddSound(transform.position);
        if(HapticSingleton.Instance != null)
        {
            HapticSingleton.Instance.SendBothImpulses(1, 0.25f, 10000);
        }
        if(EnemySoundDetectionSystem.Instance != null)
        {
            EnemySoundDetectionSystem.Instance.CheckForAggroingSound(transform.position, 20);
        }
        targetPosition = openedPosition;
        isOpen = true;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * speed);
    }
}
