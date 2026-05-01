using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class ContinuousHaptics : MonoBehaviour
{
    private IEnumerator c;
    void OnTriggerEnter(Collider c)
    {
        HapticImpulsePlayer player;
        if(c.TryGetComponent<HapticImpulsePlayer>(out player))
        {
            StartCoroutine(Cuh(player));
        }
    }

    void OnTriggerExit(Collider co)
    {
        StopCoroutine(c);
    }

    IEnumerator Cuh(HapticImpulsePlayer player)
    {
        while (true)
        {
            player.SendHapticImpulse(0.5f, 0.25f);
            yield return new WaitForSeconds(0.25f);
        }
    }
}
