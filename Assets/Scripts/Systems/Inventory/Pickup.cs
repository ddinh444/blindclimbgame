using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickup : MonoBehaviour
{
    const float minScale = 0.5f;
    const float maxScale = 3;
    public String itemName;
    [SerializeField] private float weight;
    [SerializeField] private AudioSource audioSrc;
    private float scaleMultiplier = 1;
    private Vector3 startScale;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Grabbable");
        startScale = transform.localScale;
        scaleMultiplier = 1;
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
        audioSrc.Play();
        float waveDuration = GetWeight();
        EcholocationSingleton.Instance.AddSound(transform.position, waveDuration);
        EnemySoundDetectionSystem.Instance.CheckForInvestigativeSound(transform.position, 3f * weight);
    }
}
