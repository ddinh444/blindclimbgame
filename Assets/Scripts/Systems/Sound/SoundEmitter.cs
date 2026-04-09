using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    [SerializeField] private AudioClip[] sounds;
    [SerializeField] private float soundCooldown;
    [SerializeField] private float amplitude = 0;
    [SerializeField] private float duration = -1;
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
            if(sounds.Length > 0){
                int index = Random.Range(0, sounds.Length);
                source.clip = sounds[index];
            }
            source.Play();
            EcholocationSingleton.Instance.AddSound(transform.position);
            if(HapticSingleton.Instance != null)
            {
                if(duration == -1)
                {
                    duration = source.clip.length;
                }
                HapticSingleton.Instance.SendDirectionalImpulse(amplitude, duration, transform.position, 1);
            }
            timer = 0;
        }
    }
    
}
