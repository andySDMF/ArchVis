using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScrollViewCategorySelector : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private string categoryID = "";

    [SerializeField]
    private ScrollViewDisplay scrollDisplay;

    [Header("OnPointerClick Events")]
    [SerializeField]
    private UnityEvent onPointerClick = new UnityEvent();

    private ScrollViewAppender appender;

    public void OnPointerClick(PointerEventData eventData)
    {
        ExternalInvoke();
    }

    public void ExternalInvoke()
    {
        if (scrollDisplay == null) return;

        onPointerClick.Invoke();

        if (appender == null)
        {
            appender = scrollDisplay.Appender.GetComponentsInChildren<ScrollViewAppender>()[0];
        }

        if (appender == null) return;

        appender.Append(categoryID);
    }
}
