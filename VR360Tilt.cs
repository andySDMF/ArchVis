using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VR360Tilt : MonoBehaviour
{
    [SerializeField]
    private bool isEnabled = true;

    [SerializeField]
    private float speed = 1.0f;

    [SerializeField]
    private Transform control;

    public bool IsEnabled { get { return isEnabled; } }

    private float xValue;
    private float xValueMinMax = 1.0f;
    private float yValue;
    private float yValueMinMax = 1.0f;
    private Vector3 accelometerSmoothValue;
    private float acceleromterUpdateInterval = 1.0f / 100.0f;
    private float lowPassKernelWidthInSeconds = 0.001f;
    private Vector3 lowPassValue = Vector3.zero;

    private void Update()
    {
        if (isEnabled)
        {
            GyroModifyCamera();
        }
    }

    private void GyroModifyCamera()
    {
        control.localRotation = GyroToUnity(Input.gyro.attitude);
    }

    private Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    public void EnableControl(bool state)
    {
        isEnabled = state;
    }

    private Vector3 Lowpass()
    {
        float lowPassFilterFactor = acceleromterUpdateInterval / lowPassKernelWidthInSeconds;
        lowPassValue = Vector3.Lerp(lowPassValue, Input.acceleration, lowPassFilterFactor);

        return lowPassValue;
    }
}
