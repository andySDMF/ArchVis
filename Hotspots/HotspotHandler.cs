using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class HotspotHandler : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private CameraController mainControlScript;

    [SerializeField]
    private List<Hotspot> hotspots = new List<Hotspot>();

    [Header("Generic Actions")]
    [SerializeField]
    private UnityEvent onGenericOpen = new UnityEvent();

    [SerializeField]
    private UnityEvent onGenericClosed = new UnityEvent();

    [Header("On Chnage Events")]
    [SerializeField]
    private UnityEvent onBegin = new UnityEvent();

    [SerializeField]
    private UnityEvent onEnd = new UnityEvent();

    private bool mainCameraOn = true;
    private Hotspot current;

    public void ChangeTo(string id)
    {
        onBegin.Invoke();

        if (mainCameraOn)
        {
            mainCamera.enabled = false;
            mainControlScript.enabled = false;

            mainCameraOn = false;
        }

        Hotspot hSpot = hotspots.FirstOrDefault(h => h.name.Equals(id));

        if (hSpot != null)
        {
            if (current != null)
            {
                current.camera.enabled = false;
                current.controller.EnableControl(false);

                current.onClosed.Invoke();
            }
            else
            {
                onGenericOpen.Invoke();
            }

            hSpot.onOpen.Invoke();
            hSpot.camera.enabled = true;
            hSpot.controller.EnableControl(true);

            current = hSpot;
        }
        else
        {
            onGenericClosed.Invoke();
        }

        onEnd.Invoke();
    }

    public void ResetThis()
    {
        onBegin.Invoke();

        if (current != null)
        {
            current.camera.enabled = false;
            current.controller.EnableControl(false);

            current.onClosed.Invoke();
        }

        mainCamera.enabled = true;
        mainControlScript.enabled = true;
        mainCameraOn = true;

        current = null;

        onGenericClosed.Invoke();

        onEnd.Invoke();
    }

    [System.Serializable]
    private class Hotspot
    {
        public string name = "";
        public Camera camera;
        public VR360Controller controller;

        public UnityEvent onOpen = new UnityEvent();
        public UnityEvent onClosed = new UnityEvent();
    }
}