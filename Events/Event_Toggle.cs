using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_Toggle : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onTrue = new UnityEvent();

    [SerializeField]
    private UnityEvent onFalse = new UnityEvent();

    public void Toggle(bool state)
    {
        if (state) onTrue.Invoke();
        else onFalse.Invoke();
    }
}
