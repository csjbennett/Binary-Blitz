using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Event that is meant to be called by other scripts, animations, or events
public class GenericEvent : MonoBehaviour
{
    public string eventDescription;
    public UnityEvent genericEvent;

    public void TriggerEvent()
    {
        genericEvent.Invoke();
    }
}
