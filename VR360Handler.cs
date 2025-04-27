using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class VR360Handler : MonoBehaviour
{
    [SerializeField]
    private List<DisplayView> views = new List<DisplayView>();

    [SerializeField]
    private Renderer leftRenderer;

    [SerializeField]
    private Renderer rightRenderer;

    [SerializeField]
    private VR360Controller controller;

    [SerializeField]
    private Transform rendererObject;

    [SerializeField]
    private List<DisplayRenderer> rendererViews = new List<DisplayRenderer>();

    [Header("Events")]
    [SerializeField]
    private UnityEvent onLoad = new UnityEvent();

    [SerializeField]
    private UnityEvent onUnload = new UnityEvent();

    public void Load(int index)
    {
        DisplayView dview = views[index];

        if(dview != null)
        {
            leftRenderer.material.mainTexture = Resources.Load(dview.leftImage) as Texture2D;
            rightRenderer.material.mainTexture = Resources.Load(dview.rightImage) as Texture2D;

            if(controller != null)
            {
                controller.Set(dview.rotation);
            }
        }

        onLoad.Invoke();
    }

    public void Unload()
    {
        leftRenderer.material.mainTexture = null;
        rightRenderer.material.mainTexture = null;

        onUnload.Invoke();
    }

    public void Change(string to)
    {
        DisplayRenderer dRenderer = rendererViews.FirstOrDefault(r => r.name.Equals(to));

        if(dRenderer != null && rendererObject != null)
        {
            rendererObject.localEulerAngles = dRenderer.rotation;
        }
    }

    [System.Serializable]
    private class DisplayView
    {
        public string leftImage;
        public string rightImage;
        public Vector2 rotation;
    }

    [System.Serializable]
    private class DisplayRenderer
    {
        public string name = "";
        public Vector3 rotation;
    }
}
