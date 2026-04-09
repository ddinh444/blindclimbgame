using UnityEngine;

public class ContinousHapticEmitter : MonoBehaviour
{
    [SerializeField] float pulseInterval;
    [SerializeField] float amplitude;
    [SerializeField] int priority = -1;
    [SerializeField] float range;
    float timer;

    void Start()
    {
        timer = 0;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= pulseInterval && HapticSingleton.Instance != null)
        {
            HapticSingleton.Instance.SendContinousImpulse(amplitude, pulseInterval, transform.position, range, priority);
            timer = 0;
        }
    }
}
