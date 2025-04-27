using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class ApplicationCanvasScaler : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR
        Resolution screenResolution = Screen.currentResolution;

        GetComponent<CanvasScaler>().referenceResolution = new Vector2(screenResolution.width, screenResolution.height);
#endif
    }
}
