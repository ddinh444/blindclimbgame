using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NonInventoryPickup : MonoBehaviour
{
    [SerializeField] private float weight;
    [SerializeField] private AudioSource audioSrc;
    private float noiseCooldown = 0.2f;
    public float noiseTimer;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Grabbable");
        noiseTimer = 0;
    }

    void Update()
    {
        noiseTimer -= Time.deltaTime;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if(collision.relativeVelocity.magnitude <= 5f)
        {
            return;
        }

        if(noiseTimer > 0)
        {
            return;
        }
        audioSrc.Play();
        float waveDuration = weight;
        EcholocationSingleton.Instance.AddSound(transform.position, waveDuration);
        if(HapticSingleton.Instance != null)
        {
            HapticSingleton.Instance.SendDirectionalImpulse(1, 0.3f, transform.position, 1);
        }
        EnemySoundDetectionSystem.Instance.CheckForInvestigativeSound(transform.position, 3f * weight);
        noiseTimer = noiseCooldown;
    }
}
