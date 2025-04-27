using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class ApplicationCanvasStats : MonoBehaviour
{
    private static ApplicationCanvasStats instance;

    public static ApplicationCanvasStats Instance
    {
        get
        {
            if(instance == null)
            {
                ApplicationCanvasStats[] allInstances = Resources.FindObjectsOfTypeAll<ApplicationCanvasStats>();

                if(allInstances.Length.Equals(1))
                {
                    instance = allInstances[0];

                    return instance;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("No instances exists!");
#endif
                }

                return instance;
            }
            else return instance;
        }
    }

    public float CanvasTrueScale
    {
        get
        {
            if (Scaler != null)
            {
                float referenceWidth = Scaler.referenceResolution.x;
                float referenceHeight = Scaler.referenceResolution.y;
                float match = Scaler.matchWidthOrHeight;

                return (Screen.width / referenceWidth) * (1 - match) + (Screen.height / referenceHeight) * match;
            }
            else return 1.0f;
        }
    }

    public Canvas ThisCanvas
    {
        get { return GetComponent<Canvas>(); }
    }

    public GraphicRaycaster Raycaster
    {
        get { return GetComponent<GraphicRaycaster>(); }
    }

    public CanvasScaler Scaler
    {
        get { return GetComponent<CanvasScaler>(); }
    }
}
