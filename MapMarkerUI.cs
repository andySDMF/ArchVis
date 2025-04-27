using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Grain.Map;

public class MapMarkerUI : MonoBehaviour, IMapRegenerate, IPointerClickHandler
{
    protected float cacheZoom;
    protected float cacheSize = 1.0f;

    public System.Action<MapMarkerUI> OnClick { get; set; }
    public string MapName { get; set; }

    public virtual void Process(float val = 0.0f)
    {
        cacheZoom = val;
    }

    public virtual void Clear()
    {

    }

    public virtual void Regenerate(float val = 0.0f)
    {
        CalculateZoomDependacy(val);

        cacheZoom = val;

        transform.localScale = new Vector3(cacheSize, cacheSize, 0.0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick.Invoke(this);
    }

    private void CalculateZoomDependacy(float val)
    {
        bool divide = false;

        if (val < cacheZoom)
        {

        }
        else if (val > cacheZoom)
        {
            divide = true;
        }

        for (int i = 0; i < MapUtils.Maps[MapName].ZoomDifference; i++)
        {
            if (divide) cacheSize = (cacheSize / 2);
            else cacheSize = (cacheSize * 2);
        }
    }
}
