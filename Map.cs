using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Grain.Map;

public class Map : MonoBehaviour, IMap, IMapRegenerate
{
    [SerializeField]
    private string id = "Default";

    [SerializeField]
    private RawImage baseImage;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Transform markerContainer;

    [SerializeField]
    private MapMarkerVisualiser markerVisualiser;

    [SerializeField]
    private MapSettings settings;

    [SerializeField]
    private string directory = "";

    [SerializeField]
    private string resourcePath = "";

    [SerializeField]
    private List<MapPNG> rawData = new List<MapPNG>();

    [SerializeField]
    private List<MapTile> tiles = new List<MapTile>();

    [SerializeField]
    private bool debugOn = false;

    [SerializeField]
    private bool processOnStart = true;

    [SerializeField]
    private MapScriptableCache scriptableCache;

    public string ID { get { return id; } }
    public string Url { get; private set; }
    public RawImage Display { get { return baseImage; } }

    public string Directory { get { return directory; } set { directory = value; } }
    public Vector2 Size { get { return new Vector2(settings.Width, settings.Height); } }
    public Camera Cam { get { return cam; } }
    public string ResourcePath { get { return resourcePath; } }
    public MapType Type { get { return settings.type; } }
    public float ZoomLevel { get; set; }
    public string Key { get { return settings.key; } }
    public string Origin { get { return settings.latitude.ToString() + "," + settings.longitude.ToString(); } }
    public List<MapPNG> RawData
    {
        get { return rawData; }
        set
        {
#if UNITY_EDITOR
            rawData = value;
#endif
        }
    }

    public MapScriptableCache MapCache
    {
        get { return scriptableCache; }
        set
        {
#if UNITY_EDITOR
            scriptableCache = value;
#endif
        }
    }

    public float ZoomScale { get { return cacheMakerScale; } }

    public float ConvertZoomInterval { get { return (!settings.zoomIntervals.Equals(MapZoomIntervals._1) ? 1.0f : 1.0f); } }
    public float ConvertZoomDistance { get { return settings.zoomMax - settings.zoomMin; } }
    public float MaxZoom { get { return settings.zoomMax; } }
    public float MinZoom { get { return settings.zoomMin; } }
    public List<MapMarker> FindAllMarkers { get { return (markerContainer != null) ? markerContainer.GetComponentsInChildren<MapMarker>(true).ToList() : null; } }

    public bool CanControl { get; private set; }
    public MapMarkerVisualiser Visualiser { get; set; }
    public float ZoomStep { get { return zoomSteps; } }
    public int ZoomDifference { get; private set; }
    public bool Downloading { get; private set; }

    private float zoomSteps;
    private float cacheStep;
    private float cacheMakerScale = 1.0f;
    private bool hasProcessed = false;

    private void Awake()
    {
        MapUtils.Maps.Add(id, this);

        MapUtils.GPSEncoder.SetLocalOrigin(new Vector2(settings.latitude, settings.longitude));

        ZoomLevel = (settings.baseZoom < settings.zoomMin || settings.baseZoom > settings.zoomMax) ? settings.zoomMin : settings.baseZoom;
        zoomSteps = ZoomLevel;
        cacheStep = zoomSteps;

        CanControl = true;
    }

    private void Start()
    {
        if(processOnStart)
        {
            ProcessMap();
        }
    }

    public void DownloadBaseTexture()
    {
        Downloading = true;

        StartCoroutine(AssignBaseTexture(ZoomLevel, settings.type));
    }

    public void ProcessMap()
    {
        if (hasProcessed) return;

        hasProcessed = true;

        Regenerate();

        StartCoroutine(Process());
    }

    private IEnumerator AssignBaseTexture(float zoom, MapType type)
    {
        Url = "https://maps.googleapis.com/maps/api/staticmap?center=" + settings.latitude + "," + settings.longitude +
            "&zoom=" + zoom + "&size=" + settings.Width + "x" + settings.Height + "&scale=" + settings.scale
                + "&format=png" + "&maptype=" + type + settings.style + "&key=" + settings.key;

        WWW www = new WWW(Url);
        yield return www;

        baseImage.texture = www.texture;
        baseImage.rectTransform.sizeDelta = new Vector2(settings.Width, settings.Height);

        Downloading = false;
    }

    public void OnChangeMapType(int n)
    {
        switch(n)
        {
            case 0:
                settings.type = MapType.roadmap;
                break;
            case 1:
                settings.type = MapType.satellite;
                break;
            case 2:
                settings.type = MapType.hybrid;
                break;
            case 3:
                settings.type = MapType.terrain;
                break;
        }

        SetTiles();
    }

    public void SetTiles()
    {
        if (debugOn)
        {
            Debug.Log("Setting tiles for map [" + id + "]");
        }

        tiles.ForEach(t => t.Set());
    }

    public string GetTile(string tile)
    {
        MapPNG mPNG = rawData.FirstOrDefault(d => d.name.Equals(tile));

        if(mPNG != null)
        {
            MapZoomLevel mZoom = mPNG.zoomLevels.FirstOrDefault(z => z.zoom.Equals(zoomSteps));

            if (mZoom != null)
            {
                string folder = "";

                switch (Type)
                {
                    case MapType.roadmap:
                        folder = "roadmap";
                        break;
                    case MapType.satellite:
                        folder = "satellite";
                        break;
                    case MapType.hybrid:
                        folder = "hybrid";
                        break;
                    case MapType.terrain:
                        folder = "terrain";
                        break;
                }

                if (debugOn)
                {
                    Debug.Log("Tile [" + tile + "] has been found for map::" + id);
                }

                return folder + "/" + mZoom.internalURL;
            }
            else
            {
                if (debugOn)
                {
                    Debug.Log("Zoom Level  for Tile [" + tile + "] does not exists for map::" + id);
                }
            }
        }
        else
        {
            if(debugOn)
            {
                Debug.Log("Tile [" + tile + "] does not exists for map::" + id);
            }
        }

        return "";
    }

    public void Clear()
    {
        if (markerVisualiser != null) markerVisualiser.Clear();

        ZoomLevel = (settings.baseZoom < settings.zoomMin || settings.baseZoom > settings.zoomMax) ? settings.zoomMin : settings.baseZoom;

        Regenerate();

        if (debugOn)
        {
            Debug.Log("Clearing map [" + id + "]");
        }
    }

    public void Regenerate(float val = 0.0f)
    {
        for (int i = 0; i < rawData.Count; i++)
        {
            for (int j = 0; j < rawData[i].zoomLevels.Count; j++)
            {
                if(rawData[i].zoomLevels[j].zoom < ZoomLevel + 0.1f && rawData[i].zoomLevels[j].zoom > ZoomLevel - 0.1f)
                {
                    zoomSteps = rawData[i].zoomLevels[j].zoom;

                    if (zoomSteps.Equals(cacheStep)) break;

                    if (debugOn)
                    {
                        Debug.Log("Regenerating map [" + id + "]");
                    }

                    CanControl = false;

                    SetTiles();

                    CalculateZoomDependacy();

                    if (markerContainer != null)
                    {
                        markerContainer.localScale = new Vector3(cacheMakerScale, cacheMakerScale, 0.0f);
                    }

                    if (markerVisualiser != null) markerVisualiser.Regenerate(cacheMakerScale);

                    cacheStep = zoomSteps;

                    CanControl = true;

                    break;
                }
            }
        }
    }

    private IEnumerator Process()
    {
        if(markerVisualiser != null) markerVisualiser.Process(this);
        else
        {
            MapMarker[] markers = GetComponentsInChildren<MapMarker>(true);

            for (int i = 0; i < markers.Length; i++) markers[i].Set();
        }

        if (debugOn)
        {
            Debug.Log("Processed map [" + id + "]");
        }

        yield return null;
    }

    private void CalculateZoomDependacy()
    {
        bool divide = false;

        if (zoomSteps < cacheStep)
        {
            ZoomDifference = (int)(cacheStep - zoomSteps);
            divide = true;
        }
        else if (zoomSteps > cacheStep)
        {
            ZoomDifference = (int)(zoomSteps - cacheStep);
        }

        for(int i = 0; i < ZoomDifference; i++)
        {
           if(divide) cacheMakerScale = (cacheMakerScale / 2);
           else cacheMakerScale = (cacheMakerScale * 2);
        }
    }
}
