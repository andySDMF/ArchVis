using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.Gallery;
using TMPro;

public class GalleryIndicatorTextControl : MonoBehaviour, IGallery
{
    [SerializeField]
    private MaskableGraphic indicatorTextDisplay;

    [SerializeField]
    private int fixedCount = 0;

    public string ID { get { return gameObject.name; } }

    public System.Action<int> OnPublishDelegate;

    public void Append(string data)
    {
        if (string.IsNullOrEmpty(data))
            return;

        if (indicatorTextDisplay == null)
            return;

        int n;

        if (!int.TryParse(data, out n))
            return;

        fixedCount = n;

        if (indicatorTextDisplay is TextMeshProUGUI)
        {
            ((TextMeshProUGUI)indicatorTextDisplay).text = "1/" + fixedCount.ToString();
        }
        else
        {
            ((Text)indicatorTextDisplay).text = "1/" + fixedCount.ToString();
        }
    }

    public void Publish(int control = -1)
    {
        if (indicatorTextDisplay == null)
            return;

        if (indicatorTextDisplay is TextMeshProUGUI)
        {
            ((TextMeshProUGUI)indicatorTextDisplay).text = (control + 1).ToString() + "/" + fixedCount.ToString();
        }
        else
        {
            ((Text)indicatorTextDisplay).text = (control + 1).ToString() + "/" + fixedCount.ToString();
        }

        if (OnPublishDelegate != null) OnPublishDelegate.Invoke(control);
    }

    public void Clear()
    {
        if (indicatorTextDisplay == null)
            return;

        if (indicatorTextDisplay is TextMeshProUGUI)
        {
            ((TextMeshProUGUI)indicatorTextDisplay).text = "0/0";
        }
        else
        {
            ((Text)indicatorTextDisplay).text = "0/0";
        }
    }

    public void Jump(int val)
    {

    }
}
