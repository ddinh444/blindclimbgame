using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

[RequireComponent(typeof(Rigidbody))]
public class Pickup : MonoBehaviour
{
    const float minScale = 0.5f;
    const float maxScale = 2f;
    public String itemName;
    [SerializeField] private float weight;
    [SerializeField] private AudioSource audioSrc;
    private float scaleMultiplier = 1;
    private Vector3 startScale;

    private float noiseCooldown = 0.2f;
    public float noiseTimer;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Grabbable");
        startScale = transform.localScale;
        scaleMultiplier = 1;
        noiseTimer = noiseCooldown;
    }

    void OnEnable()
    {
        noiseTimer = noiseCooldown;
    }

    void Update()
    {
        noiseTimer -= Time.deltaTime;
    }

    public float GetWeight()
    {
        return weight * scaleMultiplier;
    }

    public void UpdateScale(float amount)
    {
        scaleMultiplier += amount;
        scaleMultiplier = Mathf.Clamp(scaleMultiplier, minScale, maxScale);

        transform.localScale = startScale * scaleMultiplier;
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
        float waveDuration = GetWeight();
        EcholocationSingleton.Instance.AddSound(transform.position, waveDuration);
        if(HapticSingleton.Instance != null)
        {
            HapticSingleton.Instance.SendDirectionalImpulse(1, 0.3f, transform.position, 1);
        }
        EnemySoundDetectionSystem.Instance.CheckForInvestigativeSound(transform.position, 10f * weight);
        noiseTimer = noiseCooldown;
    }
}
