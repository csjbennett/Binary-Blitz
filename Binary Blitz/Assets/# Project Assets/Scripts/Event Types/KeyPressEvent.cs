using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Event called by the press of a key
public class KeyPressEvent : MonoBehaviour
{
    public KeyCode[] eventKeys;
    public bool triggerOnce = true;
    public UnityEvent onKeyPressed;

    // Update is called once per frame
    void Update()
    {
        foreach (KeyCode eventKey in eventKeys)
            if (Input.GetKeyDown(eventKey))
            {
                onKeyPressed.Invoke();

                if (triggerOnce)
                    this.enabled = false;
            }
    }
}
