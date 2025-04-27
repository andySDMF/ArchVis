using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_Enable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onEnabled = new UnityEvent();

    private void OnEnable()
    {
        onEnabled.Invoke();
    }

    public void ExternalInvoke()
    {
        onEnabled.Invoke();
    }
}
