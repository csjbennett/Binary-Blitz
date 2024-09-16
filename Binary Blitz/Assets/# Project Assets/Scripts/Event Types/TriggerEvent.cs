using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Event called by physics trigger
public class TriggerEvent : MonoBehaviour
{
    public string eventDescription;
    public bool triggerOnce = true;
    public bool useTag;
    public string requiredTag;
    public UnityEvent onTriggerEnter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((useTag && requiredTag == collision.tag) || !useTag)
            onTriggerEnter.Invoke();

        if (triggerOnce)
            this.enabled = false;
    }
}
