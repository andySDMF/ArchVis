using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class CameraInitialiser : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onInitComplete = new UnityEvent();

    private bool hasInit = false;

    public Camera Cam { get; private set; }

    public void Initialise()
    {
        if (hasInit) return;

        hasInit = true;

        if (Cam == null)
        {
            Cam = GetComponent<Camera>();
        }

        Cam.enabled = false;

        onInitComplete.Invoke();
    }
}
