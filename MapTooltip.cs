using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Grain.Map;

public class MapTooltip : MonoBehaviour, IMapTooltip
{
    [SerializeField]
    private Vector2 alignment = new Vector2(50 , 50);

    [SerializeField]
    private UnityEvent onShow = new UnityEvent();

    [SerializeField]
    private UnityEvent onHide = new UnityEvent();

    [SerializeField]
    private List<TooltipData> dataDisplays = new List<TooltipData>();

    public string TravelMode { get; set; }

    public void Show()
    {
        onShow.Invoke();
    }

    public void Hide()
    {
        onHide.Invoke();
    }

    public void Publish(MapMarker marker)
    {
        if (marker == null) return;

        transform.SetParent(marker.UIMarker.transform);
        transform.localPosition = new Vector3(alignment.x, alignment.y, 0.0f);

        foreach(TooltipData tData in dataDisplays)
        {
            if (tData.textObject == null) continue;

            if(tData.dataID.Equals("TravelMode"))
            {
                tData.textObject.text = ConvertFirstLetterToUpperCase(TravelMode);
            }
            else
            {
                if (tData.includeTravelMode)
                {
                    tData.textObject.text = marker.GetData(TravelMode + "," + tData.dataID);
                }
                else
                {
                    tData.textObject.text = marker.GetData(tData.dataID);
                }
            }
        }

        Show();
    }

    public string ConvertFirstLetterToUpperCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }

    [System.Serializable]
    private class TooltipData
    {
        public string dataID;
        public bool includeTravelMode = true;
        public Text textObject;
    }
}
