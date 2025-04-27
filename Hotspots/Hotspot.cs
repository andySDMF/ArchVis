using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotspot : MonoBehaviour
{
    [SerializeField]
    private string id = "";

    private HotspotHandler handler;

    public void OnClick()
    {
        if (handler == null) handler = GetComponentInParent<HotspotHandler>();

        if (handler != null)
        {
            handler.ChangeTo(id);
        }
    }
}