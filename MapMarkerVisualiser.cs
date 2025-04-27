using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Grain.Map;
using Grain.Map.GoogleAPI;

public class MapMarkerVisualiser : MonoBehaviour, IMapRegenerate
{
    [SerializeField]
    private Transform markerContainer;

    [SerializeField]
    private Vector2 alignment;

    [SerializeField]
    private UnityEvent onMarkerSelect = new UnityEvent();

    [SerializeField]
    private List<MapMarkerItem> markerUILookup;

    [SerializeField]
    private Transform journeyContainer;

    [SerializeField]
    private string originReference = "";

    [SerializeField]
    private Color journeyColor = Color.red;

    [SerializeField]
    private float journeySpeed = 1.0f;

    [SerializeField]
    private bool setExtents = true;

    [SerializeField]
    private GameObject tooltip;

    [SerializeField]
    private bool debugOn = false;

    private List<MapMarker> markers = new List<MapMarker>();
    private Map map;

    public MapMarker Current { get; private set; }
    public bool MapAssigned {  get { return (map == null) ? false : true; } }
    public string MapName { get { return (MapAssigned) ? map.ID : ""; } }
    public string TravelMode { get { return travelMode.ToString(); } }

    private GoogleTravelMode travelMode = GoogleTravelMode.driving;
    private float cacheZoomLevel = 0.0f;
    private float journeyZoomScale = 1.0f;

    private MapDrawing mDraw = new MapDrawing();
    private MapGraphicConnect mGC;
    private IMapTooltip iTooltip;

    private List<MapInnerLevel> mapLevels = new List<MapInnerLevel>();
    private bool mapLevelsUsed = false;
    private Vector2 cacheSize;
    private RectTransform rectT;

    public void Process(Map map)
    {
        mDraw.mono = this;

        rectT = GetComponent<RectTransform>();
        cacheSize = new Vector2(rectT.sizeDelta.x, rectT.sizeDelta.y);

        mapLevels = GetComponentsInChildren<MapInnerLevel>().ToList();
        mapLevelsUsed = mapLevels.Count > 0;

        if(journeyContainer != null)
        {
            mGC = journeyContainer.GetComponent<MapGraphicConnect>();

            if (mGC != null) mGC.OnComplete = ShowTooltip;
        }

        if(tooltip != null)
        {
            iTooltip = (IMapTooltip)tooltip.GetComponent(typeof(IMapTooltip));
        }

        if (map != null)
        {
            this.map = map;
            map.Visualiser = this;

            foreach(MapMarker mMarker in map.FindAllMarkers)
            {
                mMarker.Set();

                Transform tMarker = markerUILookup.FirstOrDefault(m => m.name.Equals(mMarker.Visualise)).prefab;

                if(tMarker != null)
                {
                    Vector3 vec = map.Cam.WorldToScreenPoint(mMarker.transform.position);

                    GameObject go = Instantiate(tMarker.gameObject, Vector3.zero, Quaternion.identity, markerContainer) as GameObject;
                    go.transform.localScale = Vector3.one;
                    go.transform.position = new Vector3(vec.x + alignment.x, vec.y + alignment.y, 0.0f);
                    go.SetActive(true);

                    go.name = "MapMarkerUI_" + mMarker.ID;

                    mMarker.UIMarker = go.GetComponentInChildren<MapMarkerUI>();
                    mMarker.UIMarker.OnClick += OnMarkerClick;
                    
                    markers.Add(mMarker);

                    mMarker.UIMarker.MapName = map.ID;
                    mMarker.UIMarker.Process(map.ZoomLevel);
                }
            }
        }
        else
        {
            if (debugOn)
            {
                Debug.Log("Could not process visualiser. Map is Null.");
            }
        }

        if (mapLevelsUsed)
        {
            mapLevels.ForEach(l => l.Regenerate(map.ZoomLevel));

            int count = 0;

            for (int i = 0; i < mapLevels.Count; i++)
            {
                if (mapLevels[i].gameObject.activeSelf) count++;
            }

            if (rectT)
            {
                rectT.sizeDelta = new Vector2(cacheSize.x * count, cacheSize.y * count);
            }

        }
    }

    public void Regenerate(float val)
    {
        if (markerContainer != null) markerContainer.localScale = new Vector3(val, val, 0.0f);

        if (mGC != null) mGC.Regenerate(val);

        markers.ForEach(m => m.Regenerate(map.ZoomLevel));

        if(mapLevelsUsed)
        {
            mapLevels.ForEach(l => l.Regenerate(map.ZoomLevel));

            if (rectT)
            {
                int count = 0;

                for (int i = 0; i < mapLevels.Count; i++)
                {
                    if (mapLevels[i].gameObject.activeSelf) count++;
                }

                if (rectT)
                {
                    rectT.sizeDelta = new Vector2(cacheSize.x * count, cacheSize.y * count);
                }
            }
        }

        cacheZoomLevel = val;
    }

    public void OnMarkerClick(MapMarkerUI marker)
    {
        if (marker == null) return;

        Current = markers.FirstOrDefault(m => m.UIMarker.Equals(marker));

        if (iTooltip != null)
        {
            iTooltip.Hide();
        }

        if(setExtents)
        {
            transform.localPosition = map.MapCache.GetJourneyExtent(Current.ID, TravelMode);
            
            float z = map.MapCache.GetJourneyZoomLevel(Current.ID, TravelMode);

            if (z > 0.0f && z > map.MinZoom && z < map.MaxZoom)
            {
                map.ZoomLevel = z;
                map.Regenerate();
            }
        }

        if (debugOn)
        {
            Debug.Log("Requesting Journey [" + Current.ID + "," + TravelMode + "]");
        }

        StartCoroutine(MapUtils.RequestJourney(Current, map.ID, travelMode, OnJourneyRequestComplete));

        onMarkerSelect.Invoke();
    }

    public void Clear()
    {
        Current = null;

        if (iTooltip != null)
        {
            iTooltip.Hide();
        }

        if (mGC != null) mGC.Clear();
    }

    public void ChangeJourneyMode(int val)
    {
        travelMode = (GoogleTravelMode)val;

        if (Current != null)
        {
            if (iTooltip != null)
            {
                iTooltip.Hide();
            }

            StartCoroutine(MapUtils.RequestJourney(Current, map.ID, travelMode, OnJourneyRequestComplete));
        }
    }

    public void OnJourneyRequestComplete(MapMarker marker)
    {
        if (marker == null) return;

        if (Current != marker) return;

        if (journeyContainer == null) return;

        if (debugOn)
        {
            Debug.Log("Journey identified. Processing display for [" + Current.ID + "," + TravelMode + "]");
        }

        if (journeyContainer.gameObject.layer.Equals(5))
        {
            MapMarker origin = null;
            Vector3 vOrigin;

            marker.UIMarker.transform.SetAsLastSibling();

            if (marker.UseVisualiserOrigin)
            {
                if(!string.IsNullOrEmpty(originReference))
                {
                    origin = markers.FirstOrDefault(m => m.ID.Equals(originReference));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(marker.Origin))
                {
                    origin = markers.FirstOrDefault(m => m.ID.Equals(marker.Origin));
                }
            }

            if (origin == null)
            {
                vOrigin = map.Cam.WorldToScreenPoint(marker.Journeys[travelMode.ToString()][0]);
                vOrigin = new Vector3(vOrigin.x + alignment.x, vOrigin.y + alignment.y, 0.0f);
            }
            else
            {
                vOrigin = origin.UIMarker.transform.localPosition;
            }

            List<Vector3> distances = new List<Vector3>();
            Vector3 v;

            int count = 0;

            foreach (Vector3 pos in marker.Journeys[travelMode.ToString()])
            {
                v = map.Cam.WorldToScreenPoint(pos);
                v = new Vector3(v.x + alignment.x, v.y + alignment.y, 0.0f);
                journeyZoomScale = cacheZoomLevel;

                distances.Add(v);

                count++;
            }

            if (mGC != null)
            {
                mGC.DrawSpeed = journeySpeed;
                mGC.StyleColor = journeyColor;
                mGC.SetNewPoints(marker.ID, distances.ToArray(), vOrigin);
            }
        }
        else
        {
            LineRenderer lRenderer = journeyContainer.GetComponent<LineRenderer>();

            if (lRenderer == null)
            {
                lRenderer = journeyContainer.gameObject.AddComponent<LineRenderer>();
            }

            StartCoroutine(mDraw.Draw3D(lRenderer, journeyColor, marker.Journeys[travelMode.ToString()], journeySpeed));
        }
    }

    public void ShowTooltip(string name)
    {
        if (Current == null) return;

        if (!Current.ID.Equals(name)) return;

        if (iTooltip != null)
        {
            iTooltip.TravelMode = travelMode.ToString();
            iTooltip.Publish(Current);
        }
    }
}
