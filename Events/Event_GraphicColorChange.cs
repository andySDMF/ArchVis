using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Event_GraphicColorChange : MonoBehaviour
{
    [SerializeField]
    private MaskableGraphic asset;

    [SerializeField]
    private Color activeColor = Color.red;

    [SerializeField]
    private Color inactiveColor = Color.white;

    private bool cacheState = false;

    public void Toggle(bool state)
    {
        asset.color = (state) ? activeColor : inactiveColor;

        cacheState = state;
    }

    public void AssignColor(Color active, Color inactive)
    {
        activeColor = active;
        inactiveColor = inactive;

        Toggle(cacheState);
    }
}
