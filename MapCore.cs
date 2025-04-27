using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using Grain.Map;
using Grain.Map.GoogleAPI;

namespace Grain.Map
{
    public enum MapType { roadmap, satellite, hybrid, terrain }

    public enum MapZoomIntervals { _1 }

    public enum MapSize { _512, _1024, _2048, _4096 }

    public interface IMap
    {
        string ID { get; }

        string Url { get; }

        RawImage Display { get; }
    }

    public interface IMapRegenerate
    {
        void Clear();

        void Regenerate(float val = 0.0f);
    }

    public interface IMapTooltip
    {
        string TravelMode { get; set; }

        void Show();

        void Hide();

        void Publish(MapMarker marker);
    }

    [System.Serializable]
    public class MapSettings
    {
        public float latitude;
        public float longitude;

        public int zoomMin;
        public int zoomMax;
        public int baseZoom = 14;
        public MapZoomIntervals zoomIntervals = MapZoomIntervals._1;

        public MapType type = MapType.terrain;
        public int scale;

        public string key;
        public MapSize size = MapSize._4096;
        [Header("Style must include &style=")]
        public string style = "";

        public float Width { get { return MapUtils.GetSize(size); } }
        public float Height { get { return MapUtils.GetSize(size); } }
    }

    [System.Serializable]
    public class MapPNG
    {
        public string name;
        public Vector4 matrix;

        public List<MapZoomLevel> zoomLevels = new List<MapZoomLevel>();

        public MapPNG(string n, Vector4 m)
        {
            name = n;
            matrix = m;
        }

        public void Add(MapZoomLevel zLevel)
        {
            MapZoomLevel mZoom = zoomLevels.FirstOrDefault(z => z.Equals(zLevel.zoom));

            if(mZoom != null)
            {
                mZoom.internalURL = zLevel.internalURL;
            }
            else
            {
                zoomLevels.Add(zLevel);
            }
        }
    }

    [System.Serializable]
    public class MapZoomLevel
    {
        public float zoom;
        public string internalURL;

        public MapZoomLevel(float z, string s)
        {
            zoom = z;
            internalURL = s;
        }
    }

    [System.Serializable]
    public class MapMarkerItem
    {
        public string name = "";
        public Transform prefab;
    }

    public class MapDrawing
    {
        public enum DrawType { _Instant, _Gradual }

        public bool Cancel { get; set; }

        public MonoBehaviour mono;
        public DrawType type = DrawType._Instant;

        public string Reference = "";
        public System.Action<string> OnComplete;

        public IEnumerator Draw3D(LineRenderer renderer, Color col, List<Vector3> vecs, float drawSpeed)
        {
            if (renderer == null) yield break;

            Cancel = false;

            renderer.positionCount = 0;
            renderer.positionCount = 2;
            renderer.startWidth = 2;
            renderer.endWidth = 2;
            renderer.startColor = col;
            renderer.endColor = col;

            renderer.SetPosition(0, vecs[0]);

            int count = 0;

            while (count < vecs.Count - 1)
            {
                if (Cancel) break;

                yield return mono.StartCoroutine(Lerp3D(renderer, count, vecs[count + 1], drawSpeed / 10));

                if (Cancel) break;

                count++;

                if (renderer.positionCount < vecs.Count) renderer.positionCount++;
            }

            if (Cancel) yield break;

            if (OnComplete != null)
            {
                OnComplete.Invoke(Reference);
            }

            yield return 0;
        }

        public IEnumerator Draw2D(MapGraphicLine renderer, Color col, List<Vector2> vecs, float drawSpeed)
        {
            if (renderer == null) yield break;

            Cancel = false;

            renderer.color = col;
            renderer.RelativeSize = false;
            renderer.drivenExternally = true;

            int count = 0;

            List<Vector2> applied = new List<Vector2>();
            applied.Add(vecs[0]);
            //applied.Add(vecs[0]);

            renderer.Points = applied.ToArray();

            bool callComplete = true;

            while (count < vecs.Count - 1)
            {
                if (Cancel) break;

                yield return mono.StartCoroutine(Lerp2D(renderer, count, vecs[count + 1], drawSpeed / 10));

                if (Cancel) break;

                applied.Add(vecs[count + 1]);

                count++;

                if (renderer == null)
                {
                    callComplete = false;
                    break;
                }

                if (renderer.Points.Length - 1 < vecs.Count)
                {
                    renderer.Points = applied.ToArray();
                }

                if (Cancel) break;
            }

            if (Cancel) yield break;

            if (callComplete)
            {
                if (OnComplete != null)
                {
                    OnComplete.Invoke(Reference);
                }
            }

            yield return 0;
        }

        private IEnumerator Lerp3D(LineRenderer r, int n, Vector3 pos, float speed)
        {
            float t = 0.0f;
            Vector3 v = Vector3.zero;

            while (t < 1.0f)
            {
                t += (r.positionCount * Time.deltaTime) * speed;

                v = Vector3.Lerp(r.GetPosition(n), pos, t);
                r.SetPosition(n + 1, v);

                if (Cancel) break;

                yield return null;
            }
        }

        private IEnumerator Lerp2D(MapGraphicLine r, int n, Vector2 pos, float speed)
        {
            float t = 0.0f;
            Vector2 v = Vector2.zero;

            while (t < 1.0f)
            {
                t += (r.Points.Length - 1 * Time.deltaTime) * speed;

                v = Vector2.Lerp(r.Points[n], pos, t);
                if(n+1 < r.Points.Length) r.Points[n + 1] = v;
                else
                {
                    break;
                }

                if (Cancel) break;

                if (r == null)
                {
                    break;
                }

                yield return null;
            }
        }
    }
}

namespace Grain.Map.GoogleAPI
{
    [System.Serializable]
    public enum GoogleTravelMode { driving, walking, bicycling, transit }

    [System.Serializable]
    public class GoogleMapJourney
    {
        public List<GoogleMapWaypoint> geocoded_waypoints;
        public List<GoogleMapRoute> routes;
    }

    [System.Serializable]
    public class GoogleMapWaypoint
    {
        public string geocoder_status;
        public string place_id;
    }

    [System.Serializable]
    public class GoogleMapRoute
    {
        public GoogleMapBounds bounds;
        public string copyrights;
        public List<GoogleMapRouteLeg> legs;

        public List<Vector3> UCS = new List<Vector3>();

        public void ConvertRoute()
        {
            UCS.Clear();
            UCS.Add(MapUtils.GPSEncoder.GPSToUCS(new Vector2(float.Parse(legs[0].start_location.lat), float.Parse(legs[0].start_location.lng))));

            foreach (GoogleMapRouteLeg leg in legs)
            {
                UCS.AddRange(leg.ConvertRoute());
            }
        }
    }

    [System.Serializable]
    public class GoogleMapBounds
    {
        public GoogleMapGEO northeast;
        public GoogleMapGEO southwest;
    }

    [System.Serializable]
    public class GoogleMapRouteLeg
    {
        public GoogleMapText distance;
        public GoogleMapText duration;
        public string end_address;
        public GoogleMapGEO end_location;
        public string start_address;
        public GoogleMapGEO start_location;
        public List<GoogleMapRouteStep> steps;

        public List<Vector3> ConvertRoute()
        {
            List<Vector3> vecs = new List<Vector3>();

            foreach(GoogleMapRouteStep step in steps)
            {
                vecs.Add(step.ConvertRoute());
            }

            return vecs;
        }
    }

    [System.Serializable]
    public class GoogleMapRouteStep
    {
        public GoogleMapText distance;
        public GoogleMapText duration;
        public GoogleMapGEO end_location;
        public GoogleMapGEO start_location;
        public string travel_mode;

        public Vector3 ConvertRoute()
        {
            return MapUtils.GPSEncoder.GPSToUCS(new Vector2(float.Parse(end_location.lat), float.Parse(end_location.lng)));
        }
    }

    [System.Serializable]
    public class GoogleMapText
    {
        public string text;
        public string value;
    }

    [System.Serializable]
    public class GoogleMapGEO
    {
        public string lat;
        public string lng;
    }
}

public static class MapUtils
{
    public static Dictionary<string, Map> Maps = new Dictionary<string, Map>();

    public static MAPGPSEncoder GPSEncoder = new MAPGPSEncoder();

    public static List<MapPNG> CreateCache(RectTransform elem, Camera cam, MapType type, float zoom, string directory, string resourcePath)
    {
        int divider = 1;

        int counter = (elem.sizeDelta.x >= 4096) ? 4 : 1;

        if (elem.sizeDelta.x >= 4096) divider = 2;

        int width = (int)elem.sizeDelta.x / divider;
        int height = (int)elem.sizeDelta.y / divider;

        List<MapPNG> tiles = new List<MapPNG>();

        for(int i = 0; i < counter; i++)
        {
            Vector4 m;

            if(i == 0)
            {
                m = new Vector4(0, 0, width, height);
            }
            else
            {
                m = GetMatrix(i);
            }

            MapPNG tile = new MapPNG("MapTile" + i.ToString(), m);
            tiles.Add(tile);
        }

        if(cam != null)
        {
            RenderTexture.active = cam.activeTexture;

            foreach (MapPNG tile in tiles)
            {
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(tile.matrix.x, tile.matrix.y, tile.matrix.z, tile.matrix.w), 0, 0);
                tex.Apply();

                // Encode texture into PNG
                byte[] bytes = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);

                File.WriteAllBytes(directory + "/" + type.ToString() + "/" + zoom.ToString() + "_" + tile.name + ".png", bytes);

                tile.zoomLevels.Add(new MapZoomLevel(zoom, resourcePath + zoom.ToString() + "_" + tile.name));
            }
        }

        return tiles;
    }

    public static float GetSize(MapSize size)
    {
        float n = 0.0f;

        switch(size)
        {
            case MapSize._512:
                n = 512f;
                break;
            case MapSize._1024:
                n = 1024f;
                break;
            case MapSize._2048:
                n = 2048f;
                break;
            case MapSize._4096:
                n = 4096f;
                break;
        }

        return n;
    }

    public static Vector4 GetMatrix(int n)
    {
        Vector4 matrix = new Vector4(0, 0, 0, 0);

        switch (n)
        {
            case 1:
                matrix = new Vector4(0, 2048, 4096, 2048);
                break;
            case 2:
                matrix = new Vector4(2048, 2048, 4096, 4096);
                break;
            case 3:
                matrix = new Vector4(2048, 0, 4096, 2048);
                break;
        }

        return matrix;
    }

    public static IEnumerator RequestJourney(MapMarker marker, string map, GoogleTravelMode travelMode, System.Action<MapMarker> onComplete = null)
    {
        if (marker == null) yield break;

        string origin = Maps[map].Origin;
        string destination = marker.GetData("Latitude") + "," + marker.GetData("Longitude");
        string mode = travelMode.ToString();

        string url = "https://maps.googleapis.com/maps/api/directions/json?origin=" + origin + "&destination=" + destination + "&avoid=indoor&mode=" + mode + "&key=" + Maps[map].Key;

        WWW www = new WWW(url);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            GoogleMapJourney journey = JsonUtility.FromJson<GoogleMapJourney>(www.text);
            journey.routes.ForEach(r => r.ConvertRoute());

            if(marker.Journeys.ContainsKey(travelMode.ToString()))
            {
                marker.Journeys[travelMode.ToString()] = journey.routes[0].UCS;
            }
            else
            {
                marker.Journeys.Add(travelMode.ToString(), journey.routes[0].UCS);
            }

            marker.CacheJourney(travelMode.ToString(), JsonUtility.ToJson(journey));
        }

        if (onComplete != null) onComplete.Invoke(marker);
    }

    public sealed class MAPGPSEncoder
    {
        [SerializeField]
        private Vector2 localOrigin = Vector2.zero;

        private float LatOrigin { get { return localOrigin.x; } }
        private float LonOrigin { get { return localOrigin.y; } }

        private float metersPerLat;
        private float metersPerLon;

        public Vector2 USCToGPS(Vector3 position)
        {
            return ConvertUCStoGPS(position);
        }

        public Vector3 GPSToUCS(Vector2 gps)
        {
            return ConvertGPStoUCS(gps);
        }

        public Vector3 GPSToUCS(float latitude, float longitude)
        {
            return ConvertGPStoUCS(new Vector2(latitude, longitude));
        }

        public void SetLocalOrigin(Vector2 localOrigin)
        {
            this.localOrigin = localOrigin;
        }

        private void FindMetersPerLat(float lat) // Compute lengths of degrees
        {
            // Set up "Constants"
            float m1 = 111132.92f;    // latitude calculation term 1
            float m2 = -559.82f;        // latitude calculation term 2
            float m3 = 1.175f;      // latitude calculation term 3
            float m4 = -0.0023f;        // latitude calculation term 4
            float p1 = 111412.84f;    // longitude calculation term 1
            float p2 = -93.5f;      // longitude calculation term 2
            float p3 = 0.118f;      // longitude calculation term 3

            lat = lat * Mathf.Deg2Rad;

            // Calculate the length of a degree of latitude and longitude in meters
            metersPerLat = m1 + (m2 * Mathf.Cos(2 * (float)lat)) + (m3 * Mathf.Cos(4 * (float)lat)) + (m4 * Mathf.Cos(6 * (float)lat));
            metersPerLon = (p1 * Mathf.Cos((float)lat)) + (p2 * Mathf.Cos(3 * (float)lat)) + (p3 * Mathf.Cos(5 * (float)lat));
        }

        private Vector3 ConvertGPStoUCS(Vector2 gps)
        {
            FindMetersPerLat(LatOrigin);
            float zPosition = metersPerLat * (gps.x - LatOrigin); //Calc current lat
            float xPosition = metersPerLon * (gps.y - LonOrigin); //Calc current lat
            return new Vector3((float)xPosition, 0, (float)zPosition);
        }

        private Vector2 ConvertUCStoGPS(Vector3 position)
        {
            FindMetersPerLat(LatOrigin);
            Vector2 geoLocation = new Vector2(0, 0);
            geoLocation.x = (LatOrigin + (position.z) / metersPerLat); //Calc current lat
            geoLocation.y = (LonOrigin + (position.x) / metersPerLon); //Calc current lon
            return geoLocation;
        }
    }
}

