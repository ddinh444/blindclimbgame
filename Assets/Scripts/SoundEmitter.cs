using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private AudioSource sound;
    [SerializeField] private float soundCooldown;
    [SerializeField] private float waveLifetime;
    
    private float timer = 0;
    void Start()
    {
        float offset = Random.Range(0, soundCooldown);
        timer = offset;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if(timer > soundCooldown)
        {
            sound.Play();
            EcholocationSingleton.Instance.AddSound(transform.position, waveLifetime, sound.maxDistance);
            timer = 0;
        }
    }
}
