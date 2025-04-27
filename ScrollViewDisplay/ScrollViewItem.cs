using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollViewItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private string[] categories;

    private ScrollViewDisplay handler;

    public string[] Categories { get { return categories; } }
    public int SiblingIndex { get; private set; }

    public System.Action<PointerEventData> AdditionalListeners;

    private void Awake()
    {
        SiblingIndex = transform.GetSiblingIndex();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (handler == null) handler = GetComponentInParent<ScrollViewDisplay>();

        if (AdditionalListeners != null)
        {
            AdditionalListeners.Invoke(eventData);
        }

        if (handler != null)
        {
            handler.Select(transform);
        }
    }
}
