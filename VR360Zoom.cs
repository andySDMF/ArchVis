using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VR360Zoom : MonoBehaviour
{
    [SerializeField]
    private float minZoom = 0.0f;

    [SerializeField]
    private float maxZoom = 1.0f;

    [SerializeField]
    private float zoomSpeed = 1.0f;

    [SerializeField]
    private Camera control;

    private void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagDiff = prevTouchDeltaMag - touchDeltaMag;

            if (control.enabled)
            {
                control.fieldOfView += deltaMagDiff * zoomSpeed;
                control.fieldOfView = Mathf.Clamp(control.fieldOfView, minZoom, maxZoom);
            }
        }
        else
        {
            control.fieldOfView += Input.GetAxis("Mouse ScrollWheel") * (zoomSpeed * 10);
            control.fieldOfView = Mathf.Clamp(control.fieldOfView, minZoom, maxZoom);
        }
    }
}
