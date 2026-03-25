using UnityEngine;

public class OneTimeEnableTrigger : MonoBehaviour
{
    bool hasEnabled = false;
    [SerializeField] private GameObject[] objects;

    void Start()
    {
        hasEnabled = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player")
        {
            return;
        }

        if(hasEnabled == true) return;

        foreach(GameObject obj in objects)
        {
            obj.SetActive(true);
        }

        hasEnabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag != "Player") return;
        foreach(GameObject obj in objects)
        {
            obj.SetActive(false);
        }
    }
}
