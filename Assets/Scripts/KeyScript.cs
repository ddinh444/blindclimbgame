using UnityEngine;

public class KeyScript : MonoBehaviour
{
    [SerializeField] private AudioSource pickupNoise;
    [SerializeField] private MeshRenderer renderer;
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player")
        {
            return;
        }
        renderer.enabled = false;
        pickupNoise.Play();
        EcholocationSingleton.Instance.AddSound(transform.position, 2, 10);
        GameEvents.hasKey = true;
    }
}
