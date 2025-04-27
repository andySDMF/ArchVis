using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Grain.Map.Dynamic;
using Grain.Map;

public class MapDynamicRawTile : MonoBehaviour
{
    [SerializeField]
    private Texture2D rasterData;

    [SerializeField]
    private float lat;

    [SerializeField]
    private float lng;

    [SerializeField]
    private string id;

    private float[] heightData;
    private float relativeScale;
    private Texture2D heightTexture;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private RectD rectD;
    private UnwrappedTileId unwrappedTileId;
    private CanonicalTileId canonicalTileId;

    public TilePropertyState RasterDataState;
    public TilePropertyState HeightDataState;
    public TilePropertyState VectorDataState;

    public event Action<MapDynamicRawTile> OnHeightDataChanged = delegate { };
    public event Action<MapDynamicRawTile> OnRasterDataChanged = delegate { };
    public event Action<MapDynamicRawTile> OnVectorDataChanged = delegate { };

    public float Lat { get { return lat; } }
    public float Lng { get { return lng; } }
    public string ID { get { return id; } }

    public MeshRenderer MeshRenderer
    {
        get
        {
            if (meshRenderer == null)
            {
                if (GetComponent<MeshRenderer>())
                {
                    meshRenderer = GetComponent<MeshRenderer>();
                }
            }

            return meshRenderer;
        }
    }

    public MeshFilter MeshFilter
    {
        get
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }
            return meshFilter;
        }
    }

    public RectD RectD
    {
        get
        {
            return rectD;
        }
    }

    public UnwrappedTileId UnwrappedTileId
    {
        get
        {
            return unwrappedTileId;
        }
    }

    public CanonicalTileId CanonicalTileId
    {
        get
        {
            return canonicalTileId;
        }
    }

    internal void Initialize(MapDynamic map, UnwrappedTileId tileId, Material mat, int layer, string id)
    {
        relativeScale = 1 / Mathf.Cos(Mathf.Deg2Rad * (float)map.CenterLatitudeLongitude.x);
        rectD = Conversions.TileBounds(tileId);
        unwrappedTileId = tileId;
        canonicalTileId = tileId.Canonical;
        gameObject.name = canonicalTileId.ToString();
        this.id = id;
        var position = new Vector3((float)(rectD.Center.x - map.CenterMercator.x), 0, (float)(rectD.Center.y - map.CenterMercator.y));
        transform.localPosition = position;
        gameObject.SetActive(true);

        gameObject.layer = layer;

        var renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = mat;

        gameObject.AddComponent<MeshFilter>();
        MeshFilter.sharedMesh = BuildQuad();
    }

    public void GetGEO(MapDynamic map)
    {
        Vector2d v = VectorExtensions.GetGeoPosition(transform, map.CenterMercator);
        lat = (float)v.x;
        lng = (float)v.y;
    }

    internal void Recycle()
    {
        // TODO: to hide potential visual artifacts, use placeholder mesh / texture?

        gameObject.SetActive(false);

        // Reset internal state.
        RasterDataState = TilePropertyState.None;
        HeightDataState = TilePropertyState.None;
        VectorDataState = TilePropertyState.None;

        OnHeightDataChanged = delegate { };
        OnRasterDataChanged = delegate { };
        OnVectorDataChanged = delegate { };

        // HACK: this is for vector layer features and such.
        // It's slow and wasteful, but a better solution will be difficult.
        var childCount = transform.childCount;
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    internal void SetHeightData(byte[] data, float heightMultiplier = 1f, bool useRelative = false)
    {
        // HACK: compute height values for terrain. We could probably do this without a texture2d.
        if (heightTexture == null)
        {
            heightTexture = new Texture2D(0, 0);
        }

        heightTexture.LoadImage(data);
        byte[] rgbData = heightTexture.GetRawTextureData();

        // Get rid of this temporary texture. We don't need to bloat memory.
        heightTexture.LoadImage(null);

        if (heightData == null)
        {
            heightData = new float[256 * 256];
        }

        var newRelativeScale = useRelative ? relativeScale : 1f;
        for (int xx = 0; xx < 256; ++xx)
        {
            for (int yy = 0; yy < 256; ++yy)
            {
                float r = rgbData[(xx * 256 + yy) * 4 + 1];
                float g = rgbData[(xx * 256 + yy) * 4 + 2];
                float b = rgbData[(xx * 256 + yy) * 4 + 3];
                heightData[xx * 256 + yy] = newRelativeScale * heightMultiplier * Conversions.GetAbsoluteHeightFromColor(r, g, b);
            }
        }

        HeightDataState = TilePropertyState.Loaded;
        OnHeightDataChanged(this);
    }

    public float QueryHeightData(float x, float y)
    {
        if (heightData != null)
        {
            var intX = (int)Mathf.Clamp(x * 256, 0, 255);
            var intY = (int)Mathf.Clamp(y * 256, 0, 255);
            return heightData[intY * 256 + intX];
        }

        return 0;
    }

    public void SetRasterData(byte[] data, bool useMipMap, bool useCompression)
    {
        // Don't leak the texture, just reuse it.
        if (rasterData == null)
        {
            rasterData = new Texture2D(0, 0, TextureFormat.RGB24, useMipMap);
            rasterData.wrapMode = TextureWrapMode.Clamp;
            MeshRenderer.material.mainTexture = rasterData;
        }

        rasterData.LoadImage(data);

        if (useCompression)
        {
            // High quality = true seems to decrease image quality?
            rasterData.Compress(false);
        }

        RasterDataState = TilePropertyState.Loaded;
        OnRasterDataChanged(this);
    }

    public Texture2D GetRasterData()
    {
        return rasterData;
    }

    private Mesh BuildQuad()
    {
        var unityMesh = new Mesh();
        var verts = new Vector3[4];

        verts[0] = ((RectD.Min - RectD.Center).ToVector3xz());
        verts[2] = (new Vector3((float)(RectD.Min.x - RectD.Center.x), 0, (float)(RectD.Max.y - RectD.Center.y)));
        verts[1] = (new Vector3((float)(RectD.Max.x - RectD.Center.x), 0, (float)(RectD.Min.y - RectD.Center.y)));
        verts[3] = ((RectD.Max - RectD.Center).ToVector3xz());

        unityMesh.vertices = verts;
        var trilist = new int[6] { 0, 1, 2, 1, 3, 2 };
        unityMesh.SetTriangles(trilist, 0);
        var uvlist = new Vector2[4]
        {
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(0,0),
                new Vector2(1,0)
        };

        unityMesh.uv = uvlist;
        unityMesh.RecalculateNormals();

        MeshFilter.sharedMesh = unityMesh;

        return unityMesh;
    }

    public void SetTexture(Texture tex)
    {
        var tempMaterial = new Material(MeshRenderer.sharedMaterial);
        MeshRenderer.sharedMaterial = tempMaterial;

        tex.wrapMode = TextureWrapMode.Clamp;
        MeshRenderer.sharedMaterial.mainTexture = tex;
    }
}
