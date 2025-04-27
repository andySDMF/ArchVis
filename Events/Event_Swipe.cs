using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Event_Swipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private RectTransform objectToDrag;

    [SerializeField]
    private DragDirection dragDirection = DragDirection._Vertical;

    [SerializeField]
    private Vector2 from;

    [SerializeField]
    private Vector2 to;

    [SerializeField]
    private UnityEvent onVisible = new UnityEvent();

    [SerializeField]
    private UnityEvent onInVisible = new UnityEvent();

    private Vector2 current;
    private Vector2 target;

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    private bool hasEntered = false;
    private bool state = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();

        hasEntered = true;
        currentX = objectToDrag.anchoredPosition.x;
        currentY = objectToDrag.anchoredPosition.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (hasEntered)
        {
            if(dragDirection.Equals(DragDirection._Horizontal))
            {
                if (objectToDrag.anchoredPosition.x > to.x || objectToDrag.anchoredPosition.x < from.x)
                {
                    return;
                }
            }
            else
            {
                if (objectToDrag.anchoredPosition.y > to.y || objectToDrag.anchoredPosition.y < from.y)
                {
                    return;
                }
            }

            currentX += eventData.delta.x;
            currentY += eventData.delta.y;

            if (dragDirection.Equals(DragDirection._Horizontal))
            {
                objectToDrag.anchoredPosition = new Vector2(currentX, objectToDrag.anchoredPosition.y);
            }
            else if (dragDirection.Equals(DragDirection._Vertical))
            {
                objectToDrag.anchoredPosition = new Vector2(objectToDrag.anchoredPosition.x, currentY);
            }
            else objectToDrag.anchoredPosition = new Vector2(currentX, currentY);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        hasEntered = false;

        current = objectToDrag.anchoredPosition;

        float halfway = 0.0f;

        if (dragDirection.Equals(DragDirection._Horizontal))
        {
            if (from.x < to.x)
            {
                halfway = (Mathf.Abs(to.x) - Mathf.Abs(from.x)) / 2;
            }
            else
            {
                halfway = (Mathf.Abs(from.x) - Mathf.Abs(to.x)) / 2;
            }

            if (current.x > halfway)
            {
                target = to;
                state = true;
            }
            else
            {
                target = from;
                state = false;
            }
        }
        else
        {
            if(from.y < to.y)
            {
                halfway = (Mathf.Abs(to.y) - Mathf.Abs(from.y)) / 2;
            }
            else
            {
                halfway = (Mathf.Abs(from.y) - Mathf.Abs(to.y)) / 2;
            }

            if (current.y > halfway) 
            {
                target = to;
                state = true;
            }
            else
            {
                target = from;
                state = false;
            }
        }

        StartCoroutine(OnRelease());
    }

    public void ResetThis()
    {
        objectToDrag.anchoredPosition = from;
        state = false;
    }

    public void QuickLerp(bool targetState)
    {
        if(state != targetState)
        {
            if (!targetState)
            {
                target = from;
                current = to;
            }
            else
            {
                target = to;
                current = from;
            }

            state = targetState;
            StartCoroutine(OnRelease());
        }
    }

    private IEnumerator OnRelease()
    {
        float runningTime = 0.0f;
        float percentage = 0.0f;

        while (percentage < 1.0f)
        {
            runningTime += Time.deltaTime;
            percentage = runningTime / 0.2f;

            objectToDrag.anchoredPosition = new Vector2(EaseOutQuad(current.x, target.x, percentage), EaseOutQuad(current.y, target.y, percentage));

            yield return new WaitForEndOfFrame();
        }

        objectToDrag.anchoredPosition = target;

        if(state)
        {
            onVisible.Invoke();
        }
        else
        {
            onInVisible.Invoke();
        }
    }

    protected float EaseOutQuad(float start, float end, float val)
    {
        end -= start;
        return -end * val * (val - 2) + start;
    }

    [System.Serializable]
    private enum DragDirection { _Vertical, _Horizontal }
}
