using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<Vector3> OnFallDmgNoise;
    public static bool hasKey = false;

    public static void TriggerFallDmgNoise(Vector3 position)
    {
        OnFallDmgNoise?.Invoke(position);
    }
}