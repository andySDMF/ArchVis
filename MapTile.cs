using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.Map;

public class MapTile : MonoBehaviour
{
    [SerializeField]
    private string tile = "";

    [SerializeField]
    private RawImage baseImage;

    [SerializeField]
    private string mapReference = "Default";

    public string ID { get { return tile; } }
    public string Url { get; private set; }
    public RawImage Display { get { return baseImage; } }

    private void OnEnable()
    {
        Set();
    }

    private void OnDisable()
    {
        if(baseImage != null)
        {
            baseImage.texture = null;
        }
    }

    public void Set()
    {
        if (baseImage != null)
        {
            if(MapUtils.Maps.ContainsKey(mapReference))
            {
                baseImage.texture = Resources.Load(MapUtils.Maps[mapReference].GetTile(tile)) as Texture2D;
            }
        }
    }
}
