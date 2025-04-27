using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Grain.Map;
using Grain.Map.GoogleAPI;

public class MapMarker : MonoBehaviour, IMap, IMapRegenerate
{
    [SerializeField]
    protected string id = "Default";

    [SerializeField]
    protected string visualise = "Default";

    [SerializeField]
    protected string origin = "";

    [SerializeField]
    protected bool useVisualierOrigin = true;

    [SerializeField]
    protected RawImage baseImage;

    [Header("Fixed Coordinates")]
    [SerializeField]
    protected bool useFixedGPS = false;

    [SerializeField]
    protected float latitude;

    [SerializeField]
    protected float longitude;

    public string ID { get { return id; } }
    public string Url { get; private set; }
    public RawImage Display { get { return baseImage; } }
    public MapMarkerUI UIMarker { get; set; }
    public string Visualise { get { return visualise; } }
    public string Origin { get { return origin; } set { origin = value; } }
    
    public bool UseVisualiserOrigin { get { return useVisualierOrigin; } }
    public Dictionary<string, List<Vector3>> Journeys = new Dictionary<string, List<Vector3>>();

    protected Dictionary<string, string> data = new Dictionary<string, string>();
    protected bool hasSet = false;

    public virtual void Set()
    {
        AddData("Name", id);

        if (useFixedGPS)
        {
            AddData("Latitude", latitude.ToString());
            AddData("Longitude", longitude.ToString());

            transform.position = MapUtils.GPSEncoder.GPSToUCS(latitude, longitude);
        }
        else
        {
            transform.position = MapUtils.GPSEncoder.GPSToUCS(float.Parse(data["Latitude"]), float.Parse(data["Longitude"]));
        }

        Map map = GetComponentInParent<Map>();

        if(map != null)
        {
            if(map.MapCache != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    string travel = ((GoogleTravelMode)i).ToString();
                    string cache = map.MapCache.GetJourney(id, travel);

                    if(!string.IsNullOrEmpty(cache))
                    {
                        CacheJourney(travel, cache);

                        GoogleMapJourney journey = JsonUtility.FromJson<GoogleMapJourney>(cache);
                        journey.routes.ForEach(r => r.ConvertRoute());

                        Journeys.Add(travel, journey.routes[0].UCS);
                    }
                }
            }
        }

        hasSet = true;
    }

    public void Create(string name, string lat, string lon, string origin, string visualiser, bool useVisualiser, string img)
    {
        id = name;
        latitude = float.Parse(lat);
        longitude = float.Parse(lon);

        AddData("Latitude", latitude.ToString());
        AddData("Longitude", longitude.ToString());

        this.origin = origin;
        useVisualierOrigin = useVisualiser;
        visualise = visualiser;

        if(baseImage != null) baseImage.texture = Resources.Load(img) as Texture2D;
    }

    public virtual void AddData(string key, string val)
    {
        if (data.ContainsKey(key))
        {
            data[key] = val;
        }
        else data.Add(key, val);
    }

    public string GetData(string key)
    {
        if (!data.ContainsKey(key)) return "";

        return data[key];
    }

    public virtual void Clear()
    {
        if (UIMarker != null) UIMarker.Clear();
    }

    public virtual void Regenerate(float val = 0.0f)
    {
        if (UIMarker != null) UIMarker.Regenerate(val);
    }

    public virtual void CacheJourney(string travelMode, string rawData)
    {
        GoogleMapJourney journey = JsonUtility.FromJson<GoogleMapJourney>(rawData);

        if (journey != null)
        {
            int count = 0;

            foreach (GoogleMapRoute gRoute in journey.routes)
            {
                if (data.ContainsKey(travelMode + ",Bounds[Northeast]")) data[travelMode + ",Bounds[Northeast]"] = gRoute.bounds.northeast.lat.ToString() + "," + gRoute.bounds.northeast.lng.ToString();
                else AddData(travelMode + ",Bounds[Northeast]", gRoute.bounds.northeast.lat.ToString() + "," + gRoute.bounds.northeast.lng.ToString());

                if (data.ContainsKey(travelMode + ",Bounds[Southwest]")) data[travelMode + ",Bounds[Southwest]"] = gRoute.bounds.northeast.lat.ToString() + "," + gRoute.bounds.northeast.lng.ToString();
                else AddData(travelMode + ",Bounds[Southwest]", gRoute.bounds.southwest.lat.ToString() + "," + gRoute.bounds.southwest.lng.ToString());

                float duration = 0.0f;
                float distance = 0.0f;

                foreach (GoogleMapRouteLeg rLeg in gRoute.legs)
                {
                    distance += float.Parse(rLeg.distance.value);
                    duration += float.Parse(rLeg.duration.value);

                    if (data.ContainsKey(travelMode + ",Address")) data[travelMode + ",Address"] = rLeg.end_address;
                    else AddData(travelMode + ",Address", rLeg.end_address);
                }

                float km = (float)System.Math.Round(distance * 0.001f, 2);
                float miles = (float)System.Math.Round(km * 0.6213711922f, 2);
                int minutes = Mathf.RoundToInt(duration * 0.0166667f);

                if (data.ContainsKey(travelMode + ",Duration")) data[travelMode + ",Duration"] = minutes.ToString();
                else AddData(travelMode + ",Duration", minutes.ToString());

                if (data.ContainsKey(travelMode + ",Km")) data[travelMode + ",Km"] = km.ToString();
                else AddData(travelMode + ",Km", km.ToString());

                if (data.ContainsKey(travelMode + ",Miles")) data[travelMode + ",Miles"] = miles.ToString();
                else AddData(travelMode + ",Miles", miles.ToString());

                count++;
            }

            if (hasSet)
            {
                Map map = GetComponentInParent<Map>();

                if (map != null)
                {
                    if (map.MapCache != null)
                    {
                        map.MapCache.AddJourney(id, travelMode, rawData);
                    }
                }
            }
        }
    }

    public enum ExtentsType { _Fixed, _Bounds }
}
