using System;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    [SerializeField] private AudioSource pickupNoise;

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
        pickupNoise.Play();
        EcholocationSingleton.Instance.AddSound(transform.position);
        Inventory.Instance.AddKeyItem(KeyItems.Level1Key);
        gameObject.SetActive(false);
    }
}
