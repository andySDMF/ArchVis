using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.Camera;

public class CameraTarget : MonoBehaviour, ICameraTarget
{
    [SerializeField]
    private bool setAngle = false;

    [SerializeField]
    private Vector2 angle = new Vector2(45, 45);

    [SerializeField]
    private bool setFocus = true;

    [SerializeField]
    private float distance = 10.0f;

    [Header("Camera Control")]
    [SerializeField]
    private CameraController controller;

    public void OnClick()
    {
        if (controller == null) return;

        if(setAngle)
        {
            if(setFocus)
            {
                controller.MoveCameraTo(angle.x, angle.y, distance);
            }
            else
            {
                controller.MoveCameraTo(angle.x, angle.y, -1);
            }
        }
        else
        {
            if(setFocus)
            {
                controller.SetFocus(distance);
            }
        }

        controller.MoveOrbitCenterTo(this.transform);
    }
}
