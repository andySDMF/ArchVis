using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapScriptableCache : ScriptableObject
{
    [SerializeField]
    public string mapName = "Default";

    [SerializeField]
    public List<Journey> journeys = new List<Journey>();

    public string GetJourney(string name, string travelMode)
    {
        string str = "";

        Journey journey = journeys.FirstOrDefault(j => j.destination.Equals(name + "." + travelMode));

        if (journey != null)
        {
            str = journey.cache;
        }

        return str;
    }

    public void AddJourney(string name, string travelMode, string cache)
    {
        Journey journey = journeys.FirstOrDefault(j => j.destination.Equals(name + "." + travelMode));

        if(journey != null)
        {
            journey.cache = cache;
        }
        else
        {
            journeys.Add(new Journey(name, travelMode, cache));
        }
    }

    public float GetJourneyZoomLevel(string name, string travelMode)
    {
        float z = -1.0f;

        Journey journey = journeys.FirstOrDefault(j => j.destination.Equals(name + "." + travelMode));

        if (journey != null)
        {
            z = journey.zoomLevel;
        }

        return z;
    }

    public Vector2 GetJourneyExtent(string name, string travelMode)
    {
        Vector2 extents = Vector2.zero;

        Journey journey = journeys.FirstOrDefault(j => j.destination.Equals(name + "." + travelMode));

        if (journey != null)
        {
            extents = new Vector2(journey.extents.x, journey.extents.y);
        }

        return extents;
    }

    public void AddJourneyExtent(string name, string travelMode, Vector2 extents, float zoomLevel)
    {
        Journey journey = journeys.FirstOrDefault(j => j.destination.Equals(name + "." + travelMode));

        if (journey != null)
        {
            journey.AddExtents(extents, zoomLevel);
        }
    }

    [System.Serializable]
    public class Journey
    {
        public string destination;
        public string travelMode;
        public string cache;

        [Header("Extents")]
        public Vector2 extents;
        public int zoomLevel = 0;

        public Journey(string d, string t, string c)
        {
            destination = d + "." + t;
            travelMode = t;
            cache = c;
        }

        public void AddExtents(Vector2 v, float z)
        {
            extents = v;
            zoomLevel = (int)z;
        }
    }
}
