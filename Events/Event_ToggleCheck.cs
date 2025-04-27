using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class Event_ToggleCheck : MonoBehaviour
{
    private Toggle component;

    public bool CheckState()
    {
        if(component == null)
        {
            component = GetComponent<Toggle>();
        }

        return (component == null) ? false : component.isOn;
    }

    public void SetStateWithNoAction(bool state)
    {
        if (component == null)
        {
            component = GetComponent<Toggle>();
        }

        if(component != null)
        {
            for(int i = 0; i < component.onValueChanged.GetPersistentEventCount(); i++)
            {
                component.onValueChanged.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }

            component.isOn = state;

            for (int i = 0; i < component.onValueChanged.GetPersistentEventCount(); i++)
            {
                component.onValueChanged.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
            }
        }
    }
}
