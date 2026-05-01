using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class CollisionHaptics : MonoBehaviour
{
    public float duration, amplitude;
    void OnTriggerEnter(Collider c)
    {
        HapticImpulsePlayer player;
        if(c.TryGetComponent<HapticImpulsePlayer>(out player))
        {
            player.SendHapticImpulse(amplitude, duration);
        }
    }
}
