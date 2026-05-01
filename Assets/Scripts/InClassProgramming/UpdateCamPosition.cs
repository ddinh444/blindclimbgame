using UnityEngine;

public class UpdateCamPosition : MonoBehaviour
{
    private Camera cam;
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        EcholocationSingleton.Instance.SetHeadPosition(cam.transform.position);
        EcholocationSingleton.Instance.SetHeadRadius(7.5f);
    }
}
