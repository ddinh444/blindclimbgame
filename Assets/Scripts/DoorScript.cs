using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private Collider collider;
    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private AudioSource openSound;

    public void Open()
    {
        openSound.Play();
        collider.enabled = false;
        renderer.enabled = false;
        EcholocationSingleton.Instance.AddSound(transform.position, 2, 20);
    }
}
