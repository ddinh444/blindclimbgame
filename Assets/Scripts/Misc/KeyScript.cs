using System;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

public class KeyScript : MonoBehaviour
{
    [SerializeField] private AudioSource pickupNoise;
    [SerializeField] private KeyItems keyItem = KeyItems.Level1Key;

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
        if(HapticSingleton.Instance != null)
        {
            HapticSingleton.Instance.SendBothImpulses(.75f, 0.15f, 1);    
        }
        Inventory.Instance.AddKeyItem(keyItem);
        gameObject.SetActive(false);
    }
}
