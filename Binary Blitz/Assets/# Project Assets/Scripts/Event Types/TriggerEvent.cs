using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Event called by physics trigger
public class TriggerEvent : MonoBehaviour
{
    public bool triggerOnce = true;
    public UnityEvent onTriggerEnter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onTriggerEnter.Invoke();

        if (triggerOnce)
            this.enabled = false;
    }
}
