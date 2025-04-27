using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SequentialListingObject : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onShow = new UnityEvent();

    [SerializeField]
    private UnityEvent onHide = new UnityEvent();

    [SerializeField]
    private UnityEvent onReset = new UnityEvent();

    public void Show()
    {
        onShow.Invoke();
    }

    public void Hide()
    {
        onHide.Invoke();
    }

    public void ResetThis()
    {
        onReset.Invoke();
    }
}
