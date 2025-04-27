using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.Map;
using Grain.Map.Dynamic;

public class MapDynamic : MonoBehaviour, IMap
{
    [SerializeField]
    private string id = "DefaultDynamic";

    [SerializeField]
    private MapSettings settings;

    [SerializeField]
    private bool processOnStart = true;

    [SerializeField]
    private MapScriptableCache scriptableCache;

    [SerializeField]
    private float unityTileSize = 100;

    [SerializeField]
    private Transform root;

    [SerializeField]
    private Material baseMaterial;

    [SerializeField]
    private int west;
    [SerializeField]
    private int north;
    [SerializeField]
    private int east;
    [SerializeField]
    private int south;

    [SerializeField]
    private string directory = "";

    [SerializeField]
    private string resourcePath = "";

    private Vector2d mapCenter;
    private Vector2d mapCenterMercator;
    private float worldRelativeScale;
    private Dictionary<UnwrappedTileId, MapDynamicRawTile> tiles = new Dictionary<UnwrappedTileId, MapDynamicRawTile>();

    public string ID { get { return id; } }
    public string Url { get; private set; }
    public RawImage Display { get; private set; }

    public MapType Type { get { return settings.type; } }
    public string Directory {  get { return directory; } }
    public bool Downloading { get; private set; }

    public int TileCount { get { return (Tiles.Length <= 0) ? 1000 : Tiles.Length; } }
    public int Progress { get; private set; }

    public float ConvertZoomInterval { get { return (!settings.zoomIntervals.Equals(MapZoomIntervals._1) ? 1.0f : 1.0f); } }
    public float ConvertZoomDistance { get { return settings.zoomMax - settings.zoomMin; } }
    public float MaxZoom { get { return settings.zoomMax; } }
    public float MinZoom { get { return settings.zoomMin; } }
    public float ZoomLevel { get; set; }
    public string Key { get { return settings.key; } }

    public MapDynamicRawTile[] Tiles
    {
        get { return root.GetComponentsInChildren<MapDynamicRawTile>(); }
    }

    public Vector2d CenterLatitudeLongitude
    {
        get
        {
            return mapCenter;
        }
    }

    public Vector2d CenterMercator
    {
        get
        {
            return mapCenterMercator;
        }
    }

    public float WorldRelativeScale
    {
        get
        {
            return worldRelativeScale;
        }
    }

    private void Start()
    {
        tiles.Clear();

        if (processOnStart) StartCoroutine(Process());
    }

    public void DownloadBaseTexture()
    {
        Downloading = true;
        Progress = 0;

        StartCoroutine(AssignBaseTexture(ZoomLevel, settings.type));
    }

    public void CancelDownload()
    {
        StopAllCoroutines();
        Downloading = false;
    }

    private IEnumerator AssignBaseTexture(float zoom, MapType type)
    {
        while (root.childCount > 0)
        {
            GameObject.DestroyImmediate(root.GetChild(0).gameObject);
            yield return null;
        }

        tiles.Clear();

        mapCenter = new Vector2d((double)settings.latitude, (double)settings.longitude);
        var referenceTileRect = Conversions.TileBounds(TileCover.CoordinateToTileId(mapCenter, (int)zoom));
        mapCenterMercator = referenceTileRect.Center;

        worldRelativeScale = (float)(unityTileSize / referenceTileRect.Size.x);
       // root.localScale = Vector3.one * worldRelativeScale;

        AssignTiles((int)zoom);

        foreach(KeyValuePair<UnwrappedTileId, MapDynamicRawTile> tile in tiles)
        {
            Url = "https://maps.googleapis.com/maps/api/staticmap?center=" + tile.Value.Lat + "," + tile.Value.Lng +
            "&zoom=" + zoom + "&size=" + 256 + "x" + 256 + "&scale=" + settings.scale
                + "&format=png" + "&maptype=" + type + settings.style + "&key=" + settings.key;

            WWW www = new WWW(Url);
            yield return www;

            tile.Value.SetTexture(www.texture);

            Progress++;
        }

        Downloading = false;

        yield return null;
    }

    private void AssignTiles(int zoom)
    {
        var centerTile = TileCover.CoordinateToTileId(mapCenter, zoom);

        UnwrappedTileId tile;

        int id = 0;

        for (int x = (int)(centerTile.X - west); x <= (centerTile.X + east); x++)
        {
            for (int y = (int)(centerTile.Y - north); y <= (centerTile.Y + south); y++)
            {
                tile = new UnwrappedTileId(zoom, x, y);
                tiles.Add(tile, OnTileAdded(tile, id.ToString()));

                id++;
            }
        }

        Dictionary<string, List<Transform>> grid = new Dictionary<string, List<Transform>>();

        foreach(MapDynamicRawTile t in Tiles)
        {
            string[] name = t.gameObject.name.Split('/');

            if(!grid.ContainsKey(name[1]))
            {
                grid.Add(name[1], new List<Transform>());
            }
            grid[name[1]].Add(t.transform);
        }

        foreach(KeyValuePair<string, List<Transform>> nT in grid)
        {
            int count = 0;
            float increase = 0.1f;
            float zPos = Conversions.Adjustment(zoom);

            foreach(Transform t in nT.Value)
            {
                if (count.Equals(0))
                {
                    count++;
                    continue;
                }

                t.transform.localPosition = new Vector3(t.transform.localPosition.x, increase, t.transform.localPosition.z + zPos);
                increase += 0.1f;
                zPos += Conversions.Adjustment(zoom);

                count++;
            }
        }

        foreach (MapDynamicRawTile t in Tiles)
        {
           t.GetGEO(this);
        }
    }

    private MapDynamicRawTile OnTileAdded(UnwrappedTileId tileId, string id)
    {
        MapDynamicRawTile tile = null;

        tile = new GameObject().AddComponent<MapDynamicRawTile>();
        tile.transform.SetParent(root, false);

        tile.Initialize(this, tileId, baseMaterial, root.gameObject.layer, id);

        return tile;
    }

    private IEnumerator Process()
    {
        
        yield return null;
    }
}
