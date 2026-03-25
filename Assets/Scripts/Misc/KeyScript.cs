using System;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    [SerializeField] private AudioSource pickupNoise;
    [SerializeField] private MeshRenderer renderer;

    float timer = 0;

    void Update()
    {
        //timer += Time.deltaTime;
        //transform.position += Vector3.up * Mathf.Sin(timer) * .05f;
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player")
        {
            return;
        }
        renderer.enabled = false;
        pickupNoise.Play();
        EcholocationSingleton.Instance.AddSound(transform.position);
        Inventory.Instance.AddKeyItem(KeyItems.Level1Key);
    }
}
