using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Event called with a delay
public class DelayedEvent : MonoBehaviour
{
    public string eventDescription;
    public bool triggerOnce = true;
    public UnityEvent onDelayComplete;

    public void StartEvent(float time)
    {
        StartCoroutine(DelayEvent(time));
    }

    IEnumerator DelayEvent(float time)
    {
        yield return new WaitForSeconds(time);

        onDelayComplete.Invoke();

        if (triggerOnce)
            this.enabled = false;
    }
}
