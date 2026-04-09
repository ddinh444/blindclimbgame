using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticSingleton : MonoBehaviour
{
    public static HapticSingleton Instance {get; private set;}
    [SerializeField] private Transform player;
    [SerializeField] private HapticImpulsePlayer leftHaptics;
    [SerializeField] private HapticImpulsePlayer rightHaptics;
    [SerializeField] private float attenuation;

    private int currPriority = -1;
    private float timer = 0;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        currPriority = -100;
    }
    public void SendLeftImpulse(float amplitude, float duration, int priority = -1)
    {
        if(priority < currPriority)
        {
            return;
        }
        leftHaptics.SendHapticImpulse(amplitude, duration);
        currPriority = priority;
        timer = duration;
    }

    public void SendRightImpulse(float amplitude, float duration, int priority = -1)
    {
        if(priority < currPriority)
        {
            return;
        }
        rightHaptics.SendHapticImpulse(amplitude, duration);
        currPriority = priority;
        timer = duration;
    }

    public void SendBothImpulses(float amplitude, float duration, int priority = -1)
    {
        if(priority < currPriority)
        {
            return;
        }
        SendLeftImpulse(amplitude, duration, priority);
        SendRightImpulse(amplitude,duration, priority);

        currPriority = priority;
        timer = duration;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            currPriority = -100;
        }
    }

    public void SendDirectionalImpulse(float amplitude, float duration, Vector3 position, int priority = -1)
    {
        if (priority < currPriority)
            return;

        Vector3 playerPos = player.position;

        Vector3 toSource = position - playerPos;
        float distance = toSource.magnitude;

        toSource.y = 0;
        toSource.Normalize();

        Vector3 rightAxis = player.right;
        rightAxis.y = 0;
        rightAxis.Normalize();

        float pan = Vector3.Dot(toSource, rightAxis);

        float rightWeight = Mathf.Sqrt((pan + 1f) * 0.5f);
        float leftWeight  = Mathf.Sqrt((1f - pan) * 0.5f);

        float total = rightWeight + leftWeight;
        rightWeight /= total;
        leftWeight  /= total;

        float distanceFactor = 1f / (1f + attenuation * distance * distance);

        float finalAmplitude = amplitude * distanceFactor;

        rightHaptics.SendHapticImpulse(finalAmplitude * rightWeight, duration);
        leftHaptics.SendHapticImpulse(finalAmplitude * leftWeight, duration);

        currPriority = priority;
        timer = duration;
    }


    public void SendContinousImpulse(float amplitude, float duration, Vector3 position, float maxDistance, int priority = -1)
    {
        float distance = Vector3.Distance(position, player.transform.position);
        float factor = 1 - Mathf.InverseLerp(0, maxDistance, distance);
        SendDirectionalImpulse(amplitude * factor, duration, position, priority);
    }
}
