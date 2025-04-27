using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Grain.Camera;

public class CameraController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Tooltip("The default point to orbit.")]
    [SerializeField]
    private Transform defaultOrbitPoint;

    [Tooltip("The camera this UI element moves.")]
    [SerializeField]
    private Transform cameraToControl;

    [Tooltip("A function with this name is called on any 3D object that is tapped.")]
    [SerializeField]
    private string clickFunctionName = "OnClick";

    [Header("Rotation Settings")]
    public bool allowRotation = true;

    [SerializeField]
    private CameraControlFingers fingersForRotation = CameraControlFingers.One;

    public float rotationXsensitivity = 1f;
    public float rotationYsensitivity = 1f;

    [Tooltip("A higher value results in a faster slow-down.")]
    public float rotationInertia = 2f;

    [Header("Rotation X-Axis Limits")]
    public bool limitXaxis = false;

    public float xMinimum = 140f;
    public float xMaximum = 190f;

    [Header("Rotation Y-Axis Limits")]
    public bool limitYaxis = false;

    public float yMinimum = 20f;
    public float yMaximum = 40f;

    [Header("Pan Settings")]
    public bool allowPanning = false;

    [SerializeField]
    private CameraControlFingers fingersForPan = CameraControlFingers.Three;

    public float panXsensitivity = 1f;
    public float panYsensitivity = 1f;

    [Tooltip("A higher value results in a faster slow-down.")]
    public float panInertia = 5f;

    [Header("Pan Limits")]
    public bool limitPanning = false;

    public Vector3 panLimits = new Vector3(10f, 0.1f, 10f);

    [SerializeField]
    private Color editorPanLimitsColour = Color.green;

    [Header("Zoom Settings")]
    public bool allowZooming = false;

    [SerializeField]
    private CameraControlFingers fingersForZoom = CameraControlFingers.Two;

    [Tooltip("Zoom sensitivity when using a touch screen.")]
    public float touchZoomSensitivity = 7f;

    [Tooltip("Zoom sensitivity when using a mouse scroll wheel.")]
    public float scrollWheelZoomSensitivity = 6f;

    [Tooltip("A higher value results in a faster slow-down.")]
    public float zoomInertia = 4f;

    [Header("Zoom Limits")]
    public bool limitZoom = false;

    public float zoomDistanceMinimum = 45f;
    public float zoomDistanceMaximum = 64f;

    [Header("Misc Settings")]
    [Tooltip("Stops pan if zooming and stops zooming if panning")]
    public bool exclusivePanAndZoom = true;

    [SerializeField]
    private bool checkTouches = false;

    [Header("Compass Settings")]
    [SerializeField]
    private Transform compass;

    [SerializeField]
    private float compassAngleOffset;

    [Header("-- Editor Tools --")]
    [Tooltip("Moves the editor camera to assist with setting the initial position the camera should start in.")]
    [SerializeField]
    private bool editorSimulation = false;

    [SerializeField]
    private float xAngle = 0f;

    [SerializeField]
    private float yAngle = 0f;

    [SerializeField]
    private float distance = 50f;

    [SerializeField]
    private Vector3 panOffset = Vector3.zero;

    [SerializeField]
    private float shadowDistanceBias;

    private float currentXangle = 0f;
    private float currentYangle = 0f;
    private float currentDistance = 0f;
    private Vector3 currentPanOffset = Vector3.zero;

    private Vector3 currentPosition = Vector3.zero;
    private Quaternion currentRotation = Quaternion.identity;

    private RaycastHit hitData;
    private Transform cameraTransform;

    private bool isPanning = false;
    private bool isZooming = false;
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

    public float CurrentXangle { get { return currentXangle; } }
    public float CurrentYangle { get { return currentYangle; } }

    private void Awake()
    {
        initialCameraView = new Vector3(xAngle, yAngle, distance);
    }

    private void Start()
    {
#if UNITY_EDITOR
        editorSimulation = false;
#endif

#if UNITY_IOS
        checkTouches = true;
#endif
        if (cameraToControl == null)
        {
            Debug.LogError("No camera to control, self destructing.");
            Destroy(gameObject);
            return;
        }
        else
        {
            cameraTransform = cameraToControl.transform;
        }

        currentXangle = xAngle;
        currentYangle = yAngle;
        currentDistance = distance;
        currentPanOffset = panOffset;
    }

    private void LateUpdate()
    {
        RecalculateCurrentParameters();

        currentRotation = Quaternion.Euler(currentYangle, currentXangle, 0f);
        currentPosition = defaultOrbitPoint.position + currentPanOffset + (currentRotation * new Vector3(0f, 0f, -currentDistance));

        cameraTransform.position = currentPosition;
        cameraTransform.rotation = currentRotation;
    }

    private void Update()
    {
        if (!isMovingToNewPosition && allowZooming) distance -= Input.GetAxis("Mouse ScrollWheel") * scrollWheelZoomSensitivity;

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

        if (compass) compass.eulerAngles = new Vector3(compass.eulerAngles.x, compass.eulerAngles.y, (cameraToControl.transform.eulerAngles.y + compassAngleOffset));
    }

    public void SetDefaultView(float xAngle, float yAngle, float distance)
    {
        initialCameraView = new Vector3(xAngle, yAngle, distance);
    }

    public void SetOrbitPoint(Transform newTransform)
    {
        defaultOrbitPoint = newTransform;
        panOffset = Vector3.zero;
        currentPanOffset = Vector3.zero;
    }

    public void RefreshCameraToControl()
    {
        cameraTransform = cameraToControl.transform;
    }

    public void MoveOrbitCenterTo(Transform targetTransform)
    {
        panOffset = targetTransform.position - defaultOrbitPoint.position;
    }

    public void MoveOrbitCenterTo(Vector3 target)
    {
        panOffset = target - defaultOrbitPoint.position;
    }

    public void ResetPanOffset()
    {
        panOffset = Vector3.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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
            firstPointerPosition = Camera.main.ScreenToWorldPoint(firstPointerPosition);
            firstPointerPosition = cameraTransform.InverseTransformPoint(firstPointerPosition);

            lastFirstPointerPosition = firstPointerPosition;
            return;
        }

        if (secondPointerId == -1)
        {
            secondPointerId = eventData.pointerId;

            secondPointerPosition = eventData.position;
            secondPointerPosition.z = gestureStartingDistance;
            secondPointerPosition = Camera.main.ScreenToWorldPoint(secondPointerPosition);
            secondPointerPosition = cameraTransform.InverseTransformPoint(secondPointerPosition);

            lastPointerDistance = Vector2.Distance(firstPointerPosition, secondPointerPosition);
            lastSecondPointerPosition = secondPointerPosition;
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Rotate
        if (!isPanning && !isZooming && (numberOfPointers == (int)fingersForRotation))
        {
            if (eventData.pointerId == firstPointerId)
            {
                firstPointerPosition = eventData.position;
                firstPointerPosition.z = gestureStartingDistance;
                firstPointerPosition = Camera.main.ScreenToWorldPoint(firstPointerPosition);
                firstPointerPosition = cameraTransform.InverseTransformPoint(firstPointerPosition);

                Rotate(firstPointerPosition - lastFirstPointerPosition);

                lastFirstPointerPosition = firstPointerPosition;
            }
        }

        //Pan
        if (!isZooming && (numberOfPointers == (int)fingersForPan))
        {
            if ((int)fingersForPan > 1)
            {
                if (eventData.pointerId == firstPointerId)
                {
                    firstPointerDelta = eventData.delta;

                    firstPointerPosition = eventData.position;
                    firstPointerPosition.z = gestureStartingDistance;
                    firstPointerPosition = Camera.main.ScreenToWorldPoint(firstPointerPosition);
                    firstPointerPosition = cameraTransform.InverseTransformPoint(firstPointerPosition);
                }
                else if (eventData.pointerId == secondPointerId)
                {
                    if ((eventData.delta != Vector2.zero) && (firstPointerDelta != Vector2.zero) && (Vector2.Angle(eventData.delta, firstPointerDelta) < 30f))
                    {
                        if (exclusivePanAndZoom)
                            isPanning = true;

                        secondPointerPosition = eventData.position;
                        secondPointerPosition.z = gestureStartingDistance;
                        secondPointerPosition = Camera.main.ScreenToWorldPoint(secondPointerPosition);
                        secondPointerPosition = cameraTransform.InverseTransformPoint(secondPointerPosition);

                        Pan(secondPointerPosition - lastSecondPointerPosition);

                        lastSecondPointerPosition = secondPointerPosition;
                    }
                }
            }
            else
            {
                if (eventData.pointerId == firstPointerId)
                {
                    if (exclusivePanAndZoom)
                        isPanning = true;

                    firstPointerPosition = eventData.position;
                    firstPointerPosition.z = gestureStartingDistance;
                    firstPointerPosition = Camera.main.ScreenToWorldPoint(firstPointerPosition);
                    firstPointerPosition = cameraTransform.InverseTransformPoint(firstPointerPosition);

                    Pan(firstPointerPosition - lastFirstPointerPosition);

                    lastFirstPointerPosition = firstPointerPosition;
                }
            }
        }

        //Zoom
        if (!isPanning && (numberOfPointers == (int)fingersForZoom))
        {
            if (eventData.pointerId == firstPointerId)
            {
                firstPointerDelta = eventData.delta;

                firstPointerPosition = eventData.position;
                firstPointerPosition.z = gestureStartingDistance;
                firstPointerPosition = Camera.main.ScreenToWorldPoint(firstPointerPosition);
                firstPointerPosition = cameraTransform.InverseTransformPoint(firstPointerPosition);
            }
            else if (eventData.pointerId == secondPointerId)
            {
                if ((eventData.delta != Vector2.zero) && (firstPointerDelta != Vector2.zero) && (Vector2.Angle(eventData.delta, firstPointerDelta) > 120f))
                {
                    if (exclusivePanAndZoom)
                        isZooming = true;

                    secondPointerPosition = eventData.position;
                    secondPointerPosition.z = gestureStartingDistance;
                    secondPointerPosition = Camera.main.ScreenToWorldPoint(secondPointerPosition);
                    secondPointerPosition = cameraTransform.InverseTransformPoint(secondPointerPosition);

                    currentPointerDistance = Vector3.Distance(firstPointerPosition, secondPointerPosition);

                    Zoom(currentPointerDistance - lastPointerDistance);

                    lastPointerDistance = currentPointerDistance;
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if ((numberOfPointers == 1) && (Vector2.Distance(eventData.pressPosition, eventData.position) < EventSystem.current.pixelDragThreshold))
        {
            Vector3 screenPoint = eventData.position;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPoint), out hitData))
            {
                hitData.transform.SendMessage(clickFunctionName, SendMessageOptions.DontRequireReceiver);
            }
        }

        numberOfPointers--;

        if (firstPointerId == eventData.pointerId)
            firstPointerId = -1;

        if (secondPointerId == eventData.pointerId)
            secondPointerId = -1;

        if (numberOfPointers == 0)
        {
            isZooming = false;
            isPanning = false;
        }
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
        if (!allowZooming) return;
        if (isMovingToNewPosition) return;

        if ((zoomAmount > 0.05f) || (zoomAmount < -0.05f))
            distance -= zoomAmount * touchZoomSensitivity;
        ApplyLimitsToParameters();
    }

    private void Pan(Vector2 delta)
    {
        if (!allowPanning) return;
        if (isMovingToNewPosition) return;

        panOffset -= cameraTransform.right * delta.x * panXsensitivity;
        panOffset -= (Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * Vector3.forward) * delta.y * panYsensitivity;
        panOffset.y = 0f;

        ApplyLimitsToParameters();
    }

    private void RecalculateCurrentParameters()
    {
        currentXangle = Mathf.Lerp(currentXangle, xAngle, Time.deltaTime * rotationInertia);
        currentYangle = Mathf.Lerp(currentYangle, yAngle, Time.deltaTime * rotationInertia);

        currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * zoomInertia);
        currentPanOffset = Vector3.Lerp(currentPanOffset, panOffset, Time.deltaTime * panInertia);
    }

    private void ApplyLimitsToParameters()
    {
        if (limitXaxis)
            xAngle = Mathf.Clamp(xAngle, xMinimum, xMaximum);

        if (limitYaxis)
            yAngle = Mathf.Clamp(yAngle, yMinimum, yMaximum);

        if (limitZoom)
            distance = Mathf.Clamp(distance, zoomDistanceMinimum, zoomDistanceMaximum);

        if (distance < 0)
            distance = 0;

        if (limitPanning)
        {
            panOffset.x = Mathf.Clamp(panOffset.x, -panLimits.x / 2f, panLimits.x / 2f);
            panOffset.z = Mathf.Clamp(panOffset.z, -panLimits.z / 2f, panLimits.z / 2f);
        }

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

    public void ResetCameraView()
    {
        xAngle = initialCameraView.x;
        yAngle = initialCameraView.y;
        distance = initialCameraView.z;

        ZoomOut(true);
    }

    public void ZoomOut(bool reCenter)
    {
        distance = initialCameraView.z;

        if (reCenter) ResetPanOffset();
    }

    public void SetFocus(float newDistance)
    {
        distance = newDistance;
    }

    public void SwitchOrbitPointTo(Transform newOrbitPoint, float newXangle, float newYangle, float newDistance)
    {
        xAngle = newXangle;
        yAngle = newYangle;
        distance = newDistance;

        panOffset = newOrbitPoint.position - defaultOrbitPoint.position;
    }

    public void MoveCameraTo(float newXangle, float newYangle, float newDistance)
    {
        if (movingCoroutine != null)
            StopCoroutine(movingCoroutine);

        if (!gameObject.activeInHierarchy) return;

        if (newDistance < 0) movingCoroutine = StartCoroutine(DoMoveCameraTo(newXangle, newYangle, distance));
        else movingCoroutine = StartCoroutine(DoMoveCameraTo(newXangle, newYangle, newDistance));
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

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (editorSimulation)
        {
            if (defaultOrbitPoint == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error!", "Can't preview camera position without a default orbit point assigned!", "Oops, my bad!");
                editorSimulation = false;
                return;
            }

            if (cameraToControl == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error!", "Can't preview camera position without a \"Camera to Control\" assigned!", "Oops, my bad!");
                editorSimulation = false;
                return;
            }
            else
            {
                cameraTransform = cameraToControl.transform;
            }

            ApplyLimitsToParameters();

            currentRotation = Quaternion.Euler(yAngle, xAngle, 0f);
            currentPosition = defaultOrbitPoint.position + panOffset + (currentRotation * new Vector3(0f, 0f, -distance));

            cameraTransform.position = currentPosition;
            cameraTransform.rotation = currentRotation;

            UnityEditor.SceneView.currentDrawingSceneView.AlignViewToObject(cameraTransform);
        }

        if (limitPanning && (defaultOrbitPoint != null))
        {
            if ((panLimits.y > 0.15f) || (panLimits.y < 0f))
            {
                panLimits.y = 0.1f;
            }

            Gizmos.color = editorPanLimitsColour;
            Gizmos.DrawWireCube(defaultOrbitPoint.position, panLimits);

            Color tempColor = Gizmos.color;
            tempColor.a = 0.3f;
            Gizmos.color = tempColor;
            Gizmos.DrawCube(defaultOrbitPoint.position, panLimits);
        }
    }

#endif
}