using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Event_ScrollViewZoom : ScrollRect, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private float minZoom = 1.0f;

    [SerializeField]
    private float maxZoom = 10;

    [SerializeField]
    private float zoomSpeed = 10f;

    [SerializeField]
    private bool disableInputs = false;

    private float currentZoom = 1;
    private bool isPinching = false;
    private float startPinchDist;
    private float startPinchZoom;
    private Vector2 startPinchCenterPosition;
    private Vector2 startPinchScreenPosition;
    private float mouseWheelSensitivity = 1;

    private bool blockPan = false;
    private bool isReleased = false;

    public float Max { get { return maxZoom; } }
    public float Min { get { return minZoom; } }
    public float CurrentZoom { get { return currentZoom; } }

    private bool readInputs = false;

    protected override void Awake()
    {
        Input.multiTouchEnabled = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Initialise();
    }

    private void Update()
    {
        if (!readInputs) return;

        if (disableInputs) return;

        if (Input.touchCount == 2)
        {
            if (!isPinching)
            {
                isReleased = false;
                isPinching = true;

                OnPinchStart();
            }

            OnPinch();
        }
        else
        {
            if (isPinching)
            {

            }

            isPinching = false;

            if (Input.touchCount == 0)
            {
                blockPan = false;
            }
        }

        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollWheelInput) > float.Epsilon)
        {
            currentZoom *= 1 + scrollWheelInput * mouseWheelSensitivity;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            startPinchScreenPosition = (Vector2)Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, startPinchScreenPosition, null, out startPinchCenterPosition);
            Vector2 pivotPosition = new Vector3(content.pivot.x * content.rect.size.x, content.pivot.y * content.rect.size.y);
            Vector2 posFromBottomLeft = pivotPosition + startPinchCenterPosition;
            SetPivot(content, new Vector2(posFromBottomLeft.x / content.rect.width, posFromBottomLeft.y / content.rect.height));
        }

        if (content.localScale.x < minZoom)
        {
            content.localScale = Vector3.Lerp(content.localScale, new Vector3(minZoom, minZoom, minZoom), zoomSpeed * Time.deltaTime);
        }
        else
        {
            if (Mathf.Abs(content.localScale.x - currentZoom) > 0.001f)
                content.localScale = Vector3.Lerp(content.localScale, Vector3.one * currentZoom, zoomSpeed * Time.deltaTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        readInputs = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        readInputs = false;
    }

    public void Initialise()
    {
        currentZoom = 1.0f;

        content.localScale = new Vector3(minZoom, minZoom, minZoom);
        content.localPosition = Vector3.zero;
        content.pivot = new Vector2(0.5f, 0.5f);
    }

    public void DisableThis(bool state)
    {
        disableInputs = state;
    }

    public void Force(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        content.localScale = new Vector3(currentZoom, currentZoom, currentZoom);
        content.localPosition = Vector3.zero;
        content.pivot = new Vector2(0.5f, 0.5f);
    }

    

    public void Force(float zoom, Vector2 pivot)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        content.localScale = new Vector3(currentZoom, currentZoom, currentZoom);
       // content.localPosition = Vector3.zero;
        content.pivot = pivot;
    }

    protected override void SetContentAnchoredPosition(Vector2 position)
    {
        if (isPinching || blockPan) return;

        base.SetContentAnchoredPosition(position);
    }

    private void OnPinchStart()
    {
        Vector2 pos1 = Input.touches[0].position;
        Vector2 pos2 = Input.touches[1].position;

        startPinchDist = Distance(pos1, pos2) * content.localScale.x;
        startPinchZoom = currentZoom;
        startPinchScreenPosition = (pos1 + pos2) / 2;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, startPinchScreenPosition, null, out startPinchCenterPosition);

        Vector2 pivotPosition = new Vector3(content.pivot.x * content.rect.size.x, content.pivot.y * content.rect.size.y);
        Vector2 posFromBottomLeft = pivotPosition + startPinchCenterPosition;

        SetPivot(content, new Vector2(posFromBottomLeft.x / content.rect.width, posFromBottomLeft.y / content.rect.height));
        blockPan = true;
    }

    private void OnPinch()
    {
        float currentPinchDist = Distance(Input.touches[0].position, Input.touches[1].position) * content.localScale.x;
        currentZoom = (currentPinchDist / startPinchDist) * startPinchZoom;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

    }

    private float Distance(Vector2 pos1, Vector2 pos2)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pos1, null, out pos1);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pos2, null, out pos2);
        return Vector2.Distance(pos1, pos2);
    }

    private void SetPivot(RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform == null) return;

        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y) * rectTransform.localScale.x;
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= deltaPosition;
    }
}
