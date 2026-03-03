using UnityEngine;

public class TriggerObjectHandler : MonoBehaviour
{
    [Header("Target Objects")]
    [SerializeField] 
    [Tooltip("Objects to enable when entering the trigger and disable when leaving.")]
    private GameObject[] targets;

    [Header("Settings")]
    [SerializeField] 
    private bool disableOnExit = true;
    [SerializeField] 
    private string targetTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the correct tag
        if (other.CompareTag(targetTag))
        {
            SetObjectsState(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (disableOnExit && other.CompareTag(targetTag))
        {
            SetObjectsState(false);
        }
    }

    private void SetObjectsState(bool state)
    {
        foreach (GameObject obj in targets)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
        }
    }
}