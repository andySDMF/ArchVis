using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_3DClick : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent onClick = new UnityEvent();

    public virtual void OnClick()
    {
        onClick.Invoke();
    }
}
