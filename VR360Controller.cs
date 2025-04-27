using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Grain.Camera;

public class VR360Controller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private bool isEnabled = true;

    [SerializeField]
    private Transform defaultOrbitPoint;

    [SerializeField]
    private Camera cameraToControl;

    [SerializeField]
    private string clickFunctionName = "OnClick";

    [SerializeField]
    private Vector2 startCameraAngle = Vector2.zero;

    [SerializeField]
    private bool checkTouches = false;

    [Header("Rotation Settings")]
    public bool allowRotation = true;

    [SerializeField]
    private CameraControlFingers fingersForRotation = CameraControlFingers.One;

    public float rotationXsensitivity = 1f;
    public float rotationYsensitivity = 1f;
    public float rotationInertia = 2f;

    [Header("Rotation X-Axis Limits")]
    public bool limitXaxis = false;

    public float xMinimum = 140f;
    public float xMaximum = 190f;

    [Header("Rotation Y-Axis Limits")]
    public bool limitYaxis = false;

    public float yMinimum = 20f;
    public float yMaximum = 40f;

    private float xAngle = 0f;
    private float yAngle = 0f;
    private float distance = 50f;

    private float currentXangle = 0f;
    private float currentYangle = 0f;
    private float currentDistance = 0f;
    private Quaternion currentRotation = Quaternion.identity;

    private RaycastHit hitData;
    private Transform cameraTransform;

    private int numberOfPointers = 0;
    private int firstPointerId = -1;
    private int secondPointerId = -1;
    private float lastPointerDistance = 0f;
    private float currentPointerDistance = 0f;
    private float gestureStartingDistance = 10f;
    private Vector2 firstPointerDelta = Vector2.zero;
    private Vector3 firstPointerPosition = Vector3.zero;
    private Vector3 lastFirstPointerPosition = Vector3.zero;
    private Vector3 lastSecondPointerPosition = Vector3.zero;
    private Vector3 secondPointerPosition = Vector3.zero;

    private Vector3 initialCameraView;

    private bool isMovingToNewPosition = false;
    private Coroutine movingCoroutine = null;

    private void Awake()
    {
        xAngle = startCameraAngle.x;
        yAngle = startCameraAngle.y;
        distance = 0.0f;

        initialCameraView = new Vector3(xAngle, yAngle, distance);
    }

    private void Start()
    {
#if UNITY_IOS
        checkTouches = true;
#endif
        if (cameraTransform == null) cameraTransform = cameraToControl.transform;

        currentXangle = xAngle;
        currentYangle = yAngle;
        currentDistance = distance;

        RecalculateCurrentParameters();

        currentRotation = Quaternion.Euler(currentYangle, currentXangle, 0f);
        cameraTransform.rotation = currentRotation;
    }

    private void LateUpdate()
    {
        if (isEnabled)
        {
            RecalculateCurrentParameters();

            currentRotation = Quaternion.Euler(currentYangle, currentXangle, 0f);
            cameraTransform.rotation = currentRotation;
        }
    }

    private void Update()
    {
        if (isEnabled)
        {
#if !UNITY_EDITOR
        if(checkTouches)
        {
		    if(Input.touchCount == 0)
		    {
			    numberOfPointers = 0;
			    firstPointerId = -1;
			    secondPointerId = -1;
		    }
        }
#endif

            ApplyLimitsToParameters();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isEnabled) return;

        numberOfPointers++;

        if (firstPointerId == -1)
        {
            firstPointerId = eventData.pointerId;

            if (distance > 5f)
            {
                gestureStartingDistance = distance;
            }
            else
            {
                gestureStartingDistance = 5f;
            }

            firstPointerPosition = eventData.position;
            firstPointerPosition.z = gestureStartingDistance;
            firstPointerPosition = cameraToControl.ScreenToWorldPoint(firstPointerPosition);
            firstPointerPosition = cameraTransform.InverseTransformPoint(firstPointerPosition);

            lastFirstPointerPosition = firstPointerPosition;
            return;
        }

        if (secondPointerId == -1)
        {
            secondPointerId = eventData.pointerId;

            secondPointerPosition = eventData.position;
            secondPointerPosition.z = gestureStartingDistance;
            secondPointerPosition = cameraToControl.ScreenToWorldPoint(secondPointerPosition);
            secondPointerPosition = cameraTransform.InverseTransformPoint(secondPointerPosition);

            lastPointerDistance = Vector2.Distance(firstPointerPosition, secondPointerPosition);
            lastSecondPointerPosition = secondPointerPosition;
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isEnabled) return;

        if ((numberOfPointers == (int)fingersForRotation))
        {
            if (eventData.pointerId == firstPointerId)
            {
                firstPointerPosition = eventData.position;
                firstPointerPosition.z = gestureStartingDistance;
                firstPointerPosition = cameraToControl.ScreenToWorldPoint(firstPointerPosition);
                firstPointerPosition = cameraTransform.InverseTransformPoint(firstPointerPosition);

                Rotate(firstPointerPosition - lastFirstPointerPosition);

                lastFirstPointerPosition = firstPointerPosition;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isEnabled) return;

        if ((numberOfPointers == 1) && (Vector2.Distance(eventData.pressPosition, eventData.position) < EventSystem.current.pixelDragThreshold))
        {
            Vector3 screenPoint = eventData.position;

            if (Physics.Raycast(cameraToControl.ScreenPointToRay(screenPoint), out hitData))
            {
                hitData.transform.SendMessage(clickFunctionName, SendMessageOptions.DontRequireReceiver);
            }
        }

        numberOfPointers--;

        if (firstPointerId == eventData.pointerId)
            firstPointerId = -1;

        if (secondPointerId == eventData.pointerId)
            secondPointerId = -1;
    }

    public void EnableControl(bool state)
    {
        isEnabled = state;
    }

    private void Rotate(Vector2 delta)
    {
        if (!allowRotation) return;

        if (isMovingToNewPosition) return;

        xAngle += delta.x * rotationXsensitivity;
        yAngle -= delta.y * rotationYsensitivity;

        ApplyLimitsToParameters();
    }

    private void Zoom(float zoomAmount)
    {
        if (isMovingToNewPosition) return;

        ApplyLimitsToParameters();
    }

    public void ForceReset()
    {
        xAngle = startCameraAngle.x;
        yAngle = startCameraAngle.y;
        distance = 0.0f;

        currentXangle = xAngle;
        currentYangle = yAngle;
        currentDistance = distance;

        RecalculateCurrentParameters();

        currentRotation = Quaternion.Euler(currentYangle, currentXangle, 0f);

        if (cameraTransform == null) cameraTransform = cameraToControl.transform;

        cameraTransform.rotation = currentRotation;
    }

    public void Set(Vector2 vec)
    {
        xAngle = vec.x;
        yAngle = vec.y;

        currentXangle = xAngle;
        currentYangle = yAngle;

        RecalculateCurrentParameters();

        currentRotation = Quaternion.Euler(currentYangle, currentXangle, 0f);

        if (cameraTransform == null) cameraTransform = cameraToControl.transform;

        cameraTransform.rotation = currentRotation;
    }

    private void RecalculateCurrentParameters()
    {
        currentXangle = Mathf.Lerp(currentXangle, xAngle, Time.deltaTime * rotationInertia);
        currentYangle = Mathf.Lerp(currentYangle, yAngle, Time.deltaTime * rotationInertia);
    }

    private void ApplyLimitsToParameters()
    {
        if (limitXaxis)
            xAngle = Mathf.Clamp(xAngle, xMinimum, xMaximum);

        if (limitYaxis)
            yAngle = Mathf.Clamp(yAngle, yMinimum, yMaximum);

        if (distance < 0)
            distance = 0;

        while (currentXangle >= 360f)
        {
            xAngle -= 360f;
            currentXangle -= 360f;
        }

        while (currentXangle < 0f)
        {
            xAngle += 360f;
            currentXangle += 360f;
        }

        while (yAngle >= 360f)
        {
            yAngle -= 360f;
            currentYangle -= 360f;
        }

        while (yAngle < 0f)
        {
            yAngle += 360f;
            currentYangle += 360f;
        }
    }

    private IEnumerator DoMoveCameraTo(float newXangle, float newYangle, float newDistance)
    {
        isMovingToNewPosition = true;

        xAngle = newXangle;
        yAngle = newYangle;
        distance = newDistance;

        yield return new WaitForEndOfFrame();

        movingCoroutine = null;
        isMovingToNewPosition = false;
        yield break;
    }
}