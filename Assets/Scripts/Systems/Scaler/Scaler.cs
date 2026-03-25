using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scaler : MonoBehaviour
{
    public InputActionProperty increaseScaleInput;
    public InputActionProperty decreaseScaleInput;

    public float changeCooldown = 0.25f;
    private float timer;

    List<Pickup> objectsInTrigger;

    void Start()
    {
        objectsInTrigger = new List<Pickup>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if(timer > 0)
        {
            return;
        }

        if(increaseScaleInput.action.ReadValue<float>() > 0.5f)
        {
            foreach(Pickup p in objectsInTrigger)
            {
                p.UpdateScale(0.5f);
            }
            timer = changeCooldown;
            return;
        }

        if(decreaseScaleInput.action.ReadValue<float>() > 0.5f)
        {
                        foreach(Pickup p in objectsInTrigger)
            {
                p.UpdateScale(-0.5f);
            }
            timer = changeCooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Pickup p;
        if (other.TryGetComponent<Pickup>(out p))
        {
            if (!objectsInTrigger.Contains(p))
            {
                objectsInTrigger.Add(p);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Pickup p;
        if (other.TryGetComponent<Pickup>(out p))
        {
            objectsInTrigger.Remove(p);
        }
    }
}
