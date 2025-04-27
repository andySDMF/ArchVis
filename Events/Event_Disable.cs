using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_Disable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onDisabled = new UnityEvent();

    private void OnDisable()
    {
        onDisabled.Invoke();
    }

    public void ExternalInvoke()
    {
        onDisabled.Invoke();
    }
}
