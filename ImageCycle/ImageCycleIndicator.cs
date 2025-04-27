using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ImageCycleIndicator : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private UnityEvent onTrue = new UnityEvent();

    [SerializeField]
    private UnityEvent onFalse = new UnityEvent();

    [SerializeField]
    private ImageCycleFadeHandler cycleHandler;

    public void Set(bool state)
    {
        if (state)
            onTrue.Invoke();
        else
            onFalse.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        cycleHandler.Jump(transform.GetSiblingIndex() - 1);
    }
}
