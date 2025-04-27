using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_WorldUIToggle : MonoBehaviour
{
    public bool interactable = true;
    public bool isOn = false;

    [SerializeField]
    private UnityEvent onTrue = new UnityEvent();

    [SerializeField]
    private UnityEvent onFalse = new UnityEvent();

    [SerializeField]
    private UnityEvent onSetWithNoAction = new UnityEvent();

    public void OnClick()
    {
        Toggle(!isOn);
    }

    public void Toggle(bool state)
    {
        if (!interactable) return;

        isOn = state;

        if (isOn) onTrue.Invoke();
        else onFalse.Invoke();
    }

    public void SetToggleWithNoAction(bool state)
    {
        isOn = state;

        onSetWithNoAction.Invoke();
    }
}
