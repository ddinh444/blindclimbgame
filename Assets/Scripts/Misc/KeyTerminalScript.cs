using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class KeyTerminalScript : MonoBehaviour
{
    [SerializeField] private KeyItems[] keyItemsRequired;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField]private AudioClip keyMissingSound;
    [SerializeField]private AudioClip keyPresentSound;
    [SerializeField] private DoorScript door;
    [SerializeField] private float range;

    bool playedNoise = false;

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player")
        {
            Debug.Log("Not a player");
            return;
        }

        if(Vector3.Distance(door.transform.position, transform.position) > range)
        {
            Debug.Log("Not in range");
            return;
        }
        
        bool hasKey = true;
        foreach(KeyItems item in keyItemsRequired)
        {
            if (!Inventory.Instance.HasKeyItem(item))
            {
                hasKey = false;
                Debug.Log("Missing key item");
            }
        }

        if (!hasKey)
        {
            audioSrc.clip = keyMissingSound;
            audioSrc.Play();
            //EcholocationSingleton.Instance.AddSound(transform.position);
        }
        else
        {
            audioSrc.clip = keyPresentSound;
            audioSrc.Play();
            Debug.Log("Opening Door");
            //EcholocationSingleton.Instance.AddSound(transform.position);
            StartCoroutine(OpenDoor());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player") playedNoise = false;
    }

    IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(1.25f);
        door.Open();
    }
}
