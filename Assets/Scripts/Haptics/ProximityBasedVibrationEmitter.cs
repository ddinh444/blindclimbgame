using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class ProximityBasedVibrationEmitter : MonoBehaviour
{
    [SerializeField]
    private float distanceToStartVibrations = 5;
    [SerializeField]
    [Tooltip("Distance from the emitter such that the controller starts vibrating at the set max amplitude.")]
    private float distanceForMaxVibrations = 1.5f;
    [SerializeField]
    [Range(0,1)]
    [Tooltip("Minimum amplitude of vibrations.")]
    private float minAmplitude = 0;
    [SerializeField]
    [Range(0,1)]
    [Tooltip("Maximum amplitude of vibrations. Set to 0 to use default controller vibrations as maximum.")]
    private float maxAmplitude = 1;
    [SerializeField]
    private float minVibrationDuration = 1.25f;
    [SerializeField]
    private HapticImpulsePlayer[] hapticImpulsePlayers;

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (HapticImpulsePlayer player in hapticImpulsePlayers)
        {
            ProcessVibration(player);
        }
    }

    void ProcessVibration(HapticImpulsePlayer controller)
    {
        Vector3 controllerPos = controller.transform.position;
        float distance = Vector3.Distance(controllerPos, transform.position);

        if (distance > distanceToStartVibrations)
        {
            return;
        }
        float amplitude = Mathf.Lerp(minAmplitude, maxAmplitude, distance - distanceForMaxVibrations / distanceToStartVibrations - distanceForMaxVibrations);
        controller.SendHapticImpulse(amplitude, minVibrationDuration);
    }
}
