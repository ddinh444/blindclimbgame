using UnityEngine;

public class TriggerObjectHandler : MonoBehaviour
{
    [Header("Target Objects")]
    [SerializeField] 
    private GameObject[] enableOnEnter;

    [SerializeField]
    private GameObject[] disableOnExit;

    [SerializeField] 
    private string targetTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the correct tag
        if (other.CompareTag(targetTag))
        {
            foreach(GameObject obj in enableOnEnter)
            {
                obj.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            foreach(GameObject obj in disableOnExit)
            {
                obj.SetActive(false);
            }
        }
    }

}