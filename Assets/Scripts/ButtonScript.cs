using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public GameObject bridge;
    public GameObject theObject;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == theObject.name)
        {
            ActivateBridge();
        }
    }

    void ActivateBridge()
    {
        if (bridge != null)
        {
            bridge.SetActive(true);
        }
    }
}