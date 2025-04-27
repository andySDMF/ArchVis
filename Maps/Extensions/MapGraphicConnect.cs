using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Grain.Map;

public class MapGraphicConnect : MonoBehaviour, IMapRegenerate
{
    [SerializeField]
    private string mapName = "Default";

    [SerializeField]
    private Vector2 adjustment = Vector2.zero;

    [SerializeField]
    private MapDrawing.DrawType drawMethod = MapDrawing.DrawType._Instant;

    [SerializeField]
    private List<Transform> localTransforms = new List<Transform>();

    public float DrawSpeed { get; set; }
    public Color StyleColor { get; set; }
    public System.Action<string> OnComplete { get; set; }

    private MapGraphicLine mGL;
    private List<Transform> routes = new List<Transform>();
    private float cacheVal;
    private float cacheThickness = 0.0f;
    private Vector2[] newPoints;

    private MapDrawing mDraw = new MapDrawing();

    private void Awake()
    {
        mDraw.mono = this;

        cacheVal = MapUtils.Maps[mapName].ZoomScale;
        cacheThickness = cacheVal * 4;

        mDraw.OnComplete = MapUtils.Maps[mapName].Visualiser.ShowTooltip;
    }

    public void Regenerate(float val = 0.0f)
    {
        CalculateZoomDependacy(val);

        if (mGL != null)
        {
            mGL.transform.localScale = new Vector3(val, val, 0.0f);
            mGL.lineThickness = cacheThickness;
            mGL.Points = newPoints;
            mGL.RelativeSize = false;
            mGL.drivenExternally = true;
        }

        cacheVal = val;
    }

    public void SetNewPoints(string name, Vector3[] objs, Vector3 origin)
    {
        CreateTempTransforms(name, objs.ToList(), origin);

        newPoints = new Vector2[localTransforms.Count];

        for (int i = 0; i < localTransforms.Count; i++)
        {
            newPoints[i] = new Vector2(localTransforms[i].localPosition.x, localTransforms[i].localPosition.y);
        }

        if(drawMethod == MapDrawing.DrawType._Instant)
        {
            mGL.color = StyleColor;
            mGL.Points = newPoints;
            mGL.RelativeSize = false;
            mGL.drivenExternally = true;
        }
        else
        {
            mDraw.Reference = name;
            StartCoroutine(mDraw.Draw2D(mGL, StyleColor, newPoints.ToList(), DrawSpeed));
        }
    }

    public void Clear()
    {
        foreach (Transform go in routes)
        {
            DestroyObject(go.gameObject);
        }

        routes.Clear();
        localTransforms.Clear();
        newPoints = new Vector2[0];

        mGL = null;
    }

    private void CreateTempTransforms(string name, List<Vector3> vecs, Vector3 origin)
    {
        mGL = null;

        foreach (Transform go in routes)
        {
            DestroyObject(go.gameObject);
        }

        routes.Clear();

        GameObject journey = new GameObject();
        journey.transform.SetParent(this.transform);
        journey.transform.localScale = Vector3.one;
        journey.transform.position = vecs[0];
        journey.name = "Route_" + name;

        journey.AddComponent<MapGraphicLine>();
        mGL = journey.GetComponent<MapGraphicLine>();
        mGL.lineThickness = cacheThickness;

        routes.Add(journey.transform);
        localTransforms.Clear();

        for (int i = 0; i < vecs.Count; i++)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(journey.transform);
            go.transform.localScale = Vector3.one;
            go.transform.position = vecs[i];

            localTransforms.Add(go.transform);
        }

        journey.transform.localPosition = origin;
        journey.transform.localScale = new Vector3(cacheVal, cacheVal, 0.0f);
    }

    private void CalculateZoomDependacy(float val)
    {
        bool divide = false;

        if (cacheVal < val)
        {
            if (cacheThickness > 0.0f)
            {
                divide = true;
            }
        }
        else if (cacheVal > val)
        {
            if (cacheThickness > 0.0f)
            {

            }
        }

        for (int i = 0; i < MapUtils.Maps[mapName].ZoomDifference; i++)
        {
            if (divide) cacheThickness = (cacheThickness / 2);
            else cacheThickness = (cacheThickness * 2);
        }
    }
}
