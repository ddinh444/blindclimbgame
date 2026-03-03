using UnityEngine;
using System.Collections;

public class KeyTerminalScript : MonoBehaviour
{
    [SerializeField]private AudioSource keyMissingSound;
    [SerializeField]private AudioSource keyPresentSound;
    [SerializeField] private DoorScript door;
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player")
        {
            return;
        }

        if (!GameEvents.hasKey)
        {
            keyMissingSound.Play();
            EcholocationSingleton.Instance.AddSound(transform.position, 2, 20);
        }
        else
        {
            keyPresentSound.Play();
            EcholocationSingleton.Instance.AddSound(transform.position, 2, 20);
            StartCoroutine(OpenDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(1);
        door.Open();
    }
}
