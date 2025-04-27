using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGraphicBase : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
{
    public enum ResolutionMode
    {
        None,
        PerSegment,
        PerLine
    }

    static protected Material s_ETC1DefaultUI = null;

    [SerializeField] private Sprite m_Sprite;
    public Sprite sprite { get { return m_Sprite; } set { if (SetPropertyUtility.SetClass(ref m_Sprite, value)) GeneratedUVs(); SetAllDirty(); } }

    [NonSerialized]
    private Sprite m_OverrideSprite;
    public Sprite overrideSprite { get { return activeSprite; } set { if (SetPropertyUtility.SetClass(ref m_OverrideSprite, value)) GeneratedUVs(); SetAllDirty(); } }

    protected Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : sprite; } }

    // Not serialized until we support read-enabled sprites better.
    internal float m_EventAlphaThreshold = 1;
    public float eventAlphaThreshold { get { return m_EventAlphaThreshold; } set { m_EventAlphaThreshold = value; } }

    [SerializeField]
    private ResolutionMode m_improveResolution;
    public ResolutionMode ImproveResolution { get { return m_improveResolution; } set { m_improveResolution = value; SetAllDirty(); } }

    [SerializeField]
    protected float m_Resolution;
    public float Resoloution { get { return m_Resolution; } set { m_Resolution = value; SetAllDirty(); } }

    [SerializeField]
    private bool m_useNativeSize;
    public bool UseNativeSize { get { return m_useNativeSize; } set { m_useNativeSize = value; SetAllDirty(); } }

    protected MapGraphicBase()
    {
        useLegacyMeshGeneration = false;
    }

    /// <summary>
    /// Default material used to draw everything if no explicit material was specified.
    /// </summary>

    static public Material defaultETC1GraphicMaterial
    {
        get
        {
            if (s_ETC1DefaultUI == null)
                s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
            return s_ETC1DefaultUI;
        }
    }

    /// <summary>
    /// Image's texture comes from the UnityEngine.Image.
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            if (activeSprite == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return activeSprite.texture;
        }
    }

    /// <summary>
    /// Whether the Image has a border to work with.
    /// </summary>

    public bool hasBorder
    {
        get
        {
            if (activeSprite != null)
            {
                Vector4 v = activeSprite.border;
                return v.sqrMagnitude > 0f;
            }
            return false;
        }
    }

    public float pixelsPerUnit
    {
        get
        {
            float spritePixelsPerUnit = 100;
            if (activeSprite)
                spritePixelsPerUnit = activeSprite.pixelsPerUnit;

            float referencePixelsPerUnit = 100;
            if (canvas)
                referencePixelsPerUnit = canvas.referencePixelsPerUnit;

            return spritePixelsPerUnit / referencePixelsPerUnit;
        }
    }

    public override Material material
    {
        get
        {
            if (m_Material != null)
                return m_Material;

            if (activeSprite && activeSprite.associatedAlphaSplitTexture != null)
                return defaultETC1GraphicMaterial;

            return defaultMaterial;
        }

        set
        {
            base.material = value;
        }
    }


    protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
    {
        UIVertex[] vbo = new UIVertex[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            vbo[i] = vert;
        }
        return vbo;
    }

    protected Vector2[] IncreaseResolution(Vector2[] input)
    {
        var outputList = new List<Vector2>();

        switch (ImproveResolution)
        {
            case ResolutionMode.PerLine:
                float totalDistance = 0, increments = 0;
                for (int i = 0; i < input.Length - 1; i++)
                {
                    totalDistance += Vector2.Distance(input[i], input[i + 1]);
                }
                ResolutionToNativeSize(totalDistance);
                increments = totalDistance / m_Resolution;
                var incrementCount = 0;
                for (int i = 0; i < input.Length - 1; i++)
                {
                    var p1 = input[i];
                    outputList.Add(p1);
                    var p2 = input[i + 1];
                    var segmentDistance = Vector2.Distance(p1, p2) / increments;
                    var incrementTime = 1f / segmentDistance;
                    for (int j = 0; j < segmentDistance; j++)
                    {
                        outputList.Add(Vector2.Lerp(p1, (Vector2)p2, j * incrementTime));
                        incrementCount++;
                    }
                    outputList.Add(p2);
                }
                break;
            case ResolutionMode.PerSegment:
                for (int i = 0; i < input.Length - 1; i++)
                {
                    var p1 = input[i];
                    outputList.Add(p1);
                    var p2 = input[i + 1];
                    ResolutionToNativeSize(Vector2.Distance(p1, p2));
                    increments = 1f / m_Resolution;
                    for (Single j = 1; j < m_Resolution; j++)
                    {
                        outputList.Add(Vector2.Lerp(p1, (Vector2)p2, increments * j));
                    }
                    outputList.Add(p2);
                }
                break;
        }
        return outputList.ToArray();
    }

    protected virtual void GeneratedUVs() { }

    protected virtual void ResolutionToNativeSize(float distance) { }


    #region ILayoutElement Interface

    public virtual void CalculateLayoutInputHorizontal() { }
    public virtual void CalculateLayoutInputVertical() { }

    public virtual float minWidth { get { return 0; } }

    public virtual float preferredWidth
    {
        get
        {
            if (overrideSprite == null)
                return 0;
            return overrideSprite.rect.size.x / pixelsPerUnit;
        }
    }

    public virtual float flexibleWidth { get { return -1; } }

    public virtual float minHeight { get { return 0; } }

    public virtual float preferredHeight
    {
        get
        {
            if (overrideSprite == null)
                return 0;
            return overrideSprite.rect.size.y / pixelsPerUnit;
        }
    }

    public virtual float flexibleHeight { get { return -1; } }

    public virtual int layoutPriority { get { return 0; } }

    #endregion

    #region ICanvasRaycastFilter Interface
    public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // add test for line check
        if (m_EventAlphaThreshold >= 1)
            return true;

        Sprite sprite = overrideSprite;
        if (sprite == null)
            return true;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);

        Rect rect = GetPixelAdjustedRect();

        // Convert to have lower left corner as reference point.
        local.x += rectTransform.pivot.x * rect.width;
        local.y += rectTransform.pivot.y * rect.height;

        local = MapCoordinate(local, rect);

        //test local coord with Mesh

        // Normalize local coordinates.
        Rect spriteRect = sprite.textureRect;
        Vector2 normalized = new Vector2(local.x / spriteRect.width, local.y / spriteRect.height);

        // Convert to texture space.
        float x = Mathf.Lerp(spriteRect.x, spriteRect.xMax, normalized.x) / sprite.texture.width;
        float y = Mathf.Lerp(spriteRect.y, spriteRect.yMax, normalized.y) / sprite.texture.height;

        try
        {
            return sprite.texture.GetPixelBilinear(x, y).a >= m_EventAlphaThreshold;
        }
        catch (UnityException e)
        {
            Debug.LogError("Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite.", this);
            return true;
        }
    }

    /// <summary>
    /// Return image adjusted position
    /// **Copied from Unity's Image component for now and simplified for UI Extensions primatives
    /// </summary>
    /// <param name="local"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    private Vector2 MapCoordinate(Vector2 local, Rect rect)
    {
        Rect spriteRect = sprite.rect;
        //if (type == Type.Simple || type == Type.Filled)
        return new Vector2(local.x * rect.width, local.y * rect.height);

        //Vector4 border = sprite.border;
        //Vector4 adjustedBorder = GetAdjustedBorders(border / pixelsPerUnit, rect);

        //for (int i = 0; i < 2; i++)
        //{
        //    if (local[i] <= adjustedBorder[i])
        //        continue;

        //    if (rect.size[i] - local[i] <= adjustedBorder[i + 2])
        //    {
        //        local[i] -= (rect.size[i] - spriteRect.size[i]);
        //        continue;
        //    }

        //    if (type == Type.Sliced)
        //    {
        //        float lerp = Mathf.InverseLerp(adjustedBorder[i], rect.size[i] - adjustedBorder[i + 2], local[i]);
        //        local[i] = Mathf.Lerp(border[i], spriteRect.size[i] - border[i + 2], lerp);
        //        continue;
        //    }
        //    else
        //    {
        //        local[i] -= adjustedBorder[i];
        //        local[i] = Mathf.Repeat(local[i], spriteRect.size[i] - border[i] - border[i + 2]);
        //        local[i] += border[i];
        //        continue;
        //    }
        //}

        //return local;
    }

    Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
    {
        for (int axis = 0; axis <= 1; axis++)
        {
            // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
            // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
            float combinedBorders = border[axis] + border[axis + 2];
            if (rect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                float borderScaleRatio = rect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }
        return border;
    }

    #endregion

    #region onEnable
    protected override void OnEnable()
    {
        base.OnEnable();
        SetAllDirty();
    }
    #endregion
}

internal static class SetPropertyUtility
{
    public static bool SetColor(ref Color currentValue, Color newValue)
    {
        if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
    {
        if (currentValue.Equals(newValue))
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return false;

        currentValue = newValue;
        return true;
    }
}

public class BezierPath
{
    public int SegmentsPerCurve = 10;
    public float MINIMUM_SQR_DISTANCE = 0.01f;

    // This corresponds to about 172 degrees, 8 degrees from a straight line
    public float DIVISION_THRESHOLD = -0.99f;

    private List<Vector2> controlPoints;

    private int curveCount; //how many bezier curves in this path?

    /**
        Constructs a new empty Bezier curve. Use one of these methods
        to add points: SetControlPoints, Interpolate, SamplePoints.
    */
    public BezierPath()
    {
        controlPoints = new List<Vector2>();
    }

    /**
        Sets the control points of this Bezier path.
        Points 0-3 forms the first Bezier curve, points 
        3-6 forms the second curve, etc.
    */
    public void SetControlPoints(List<Vector2> newControlPoints)
    {
        controlPoints.Clear();
        controlPoints.AddRange(newControlPoints);
        curveCount = (controlPoints.Count - 1) / 3;
    }

    public void SetControlPoints(Vector2[] newControlPoints)
    {
        controlPoints.Clear();
        controlPoints.AddRange(newControlPoints);
        curveCount = (controlPoints.Count - 1) / 3;
    }

    /**
        Returns the control points for this Bezier curve.
    */
    public List<Vector2> GetControlPoints()
    {
        return controlPoints;
    }


    /**
        Calculates a Bezier interpolated path for the given points.
    */
    public void Interpolate(List<Vector2> segmentPoints, float scale)
    {
        controlPoints.Clear();

        if (segmentPoints.Count < 2)
        {
            return;
        }

        for (int i = 0; i < segmentPoints.Count; i++)
        {
            if (i == 0) // is first
            {
                Vector2 p1 = segmentPoints[i];
                Vector2 p2 = segmentPoints[i + 1];

                Vector2 tangent = (p2 - p1);
                Vector2 q1 = p1 + scale * tangent;

                controlPoints.Add(p1);
                controlPoints.Add(q1);
            }
            else if (i == segmentPoints.Count - 1) //last
            {
                Vector2 p0 = segmentPoints[i - 1];
                Vector2 p1 = segmentPoints[i];
                Vector2 tangent = (p1 - p0);
                Vector2 q0 = p1 - scale * tangent;

                controlPoints.Add(q0);
                controlPoints.Add(p1);
            }
            else
            {
                Vector2 p0 = segmentPoints[i - 1];
                Vector2 p1 = segmentPoints[i];
                Vector2 p2 = segmentPoints[i + 1];
                Vector2 tangent = (p2 - p0).normalized;
                Vector2 q0 = p1 - scale * tangent * (p1 - p0).magnitude;
                Vector2 q1 = p1 + scale * tangent * (p2 - p1).magnitude;

                controlPoints.Add(q0);
                controlPoints.Add(p1);
                controlPoints.Add(q1);
            }
        }

        curveCount = (controlPoints.Count - 1) / 3;
    }

    /**
        Sample the given points as a Bezier path.
    */
    public void SamplePoints(List<Vector2> sourcePoints, float minSqrDistance, float maxSqrDistance, float scale)
    {
        if (sourcePoints.Count < 2)
        {
            return;
        }

        Stack<Vector2> samplePoints = new Stack<Vector2>();

        samplePoints.Push(sourcePoints[0]);

        Vector2 potentialSamplePoint = sourcePoints[1];

        int i = 2;

        for (i = 2; i < sourcePoints.Count; i++)
        {
            if (
                ((potentialSamplePoint - sourcePoints[i]).sqrMagnitude > minSqrDistance) &&
                ((samplePoints.Peek() - sourcePoints[i]).sqrMagnitude > maxSqrDistance))
            {
                samplePoints.Push(potentialSamplePoint);
            }

            potentialSamplePoint = sourcePoints[i];
        }

        //now handle last bit of curve
        Vector2 p1 = samplePoints.Pop(); //last sample point
        Vector2 p0 = samplePoints.Peek(); //second last sample point
        Vector2 tangent = (p0 - potentialSamplePoint).normalized;
        float d2 = (potentialSamplePoint - p1).magnitude;
        float d1 = (p1 - p0).magnitude;
        p1 = p1 + tangent * ((d1 - d2) / 2);

        samplePoints.Push(p1);
        samplePoints.Push(potentialSamplePoint);


        Interpolate(new List<Vector2>(samplePoints), scale);
    }

    /**
        Caluclates a point on the path.
        
        @param curveIndex The index of the curve that the point is on. For example, 
        the second curve (index 1) is the curve with controlpoints 3, 4, 5, and 6.
        
        @param t The paramater indicating where on the curve the point is. 0 corresponds 
        to the "left" point, 1 corresponds to the "right" end point.
    */
    public Vector2 CalculateBezierPoint(int curveIndex, float t)
    {
        int nodeIndex = curveIndex * 3;

        Vector2 p0 = controlPoints[nodeIndex];
        Vector2 p1 = controlPoints[nodeIndex + 1];
        Vector2 p2 = controlPoints[nodeIndex + 2];
        Vector2 p3 = controlPoints[nodeIndex + 3];

        return CalculateBezierPoint(t, p0, p1, p2, p3);
    }

    /**
        Gets the drawing points. This implementation simply calculates a certain number
        of points per curve.
    */
    public List<Vector2> GetDrawingPoints0()
    {
        List<Vector2> drawingPoints = new List<Vector2>();

        for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
        {
            if (curveIndex == 0) //Only do this for the first end point. 
                                 //When i != 0, this coincides with the 
                                 //end point of the previous segment,
            {
                drawingPoints.Add(CalculateBezierPoint(curveIndex, 0));
            }

            for (int j = 1; j <= SegmentsPerCurve; j++)
            {
                float t = j / (float)SegmentsPerCurve;
                drawingPoints.Add(CalculateBezierPoint(curveIndex, t));
            }
        }

        return drawingPoints;
    }

    /**
        Gets the drawing points. This implementation simply calculates a certain number
        of points per curve.

        This is a lsightly different inplementation from the one above.
    */
    public List<Vector2> GetDrawingPoints1()
    {
        List<Vector2> drawingPoints = new List<Vector2>();

        for (int i = 0; i < controlPoints.Count - 3; i += 3)
        {
            Vector2 p0 = controlPoints[i];
            Vector2 p1 = controlPoints[i + 1];
            Vector2 p2 = controlPoints[i + 2];
            Vector2 p3 = controlPoints[i + 3];

            if (i == 0) //only do this for the first end point. When i != 0, this coincides with the end point of the previous segment,
            {
                drawingPoints.Add(CalculateBezierPoint(0, p0, p1, p2, p3));
            }

            for (int j = 1; j <= SegmentsPerCurve; j++)
            {
                float t = j / (float)SegmentsPerCurve;
                drawingPoints.Add(CalculateBezierPoint(t, p0, p1, p2, p3));
            }
        }

        return drawingPoints;
    }

    /**
        This gets the drawing points of a bezier curve, using recursive division,
        which results in less points for the same accuracy as the above implementation.
    */
    public List<Vector2> GetDrawingPoints2()
    {
        List<Vector2> drawingPoints = new List<Vector2>();

        for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
        {
            List<Vector2> bezierCurveDrawingPoints = FindDrawingPoints(curveIndex);

            if (curveIndex != 0)
            {
                //remove the fist point, as it coincides with the last point of the previous Bezier curve.
                bezierCurveDrawingPoints.RemoveAt(0);
            }

            drawingPoints.AddRange(bezierCurveDrawingPoints);
        }

        return drawingPoints;
    }

    List<Vector2> FindDrawingPoints(int curveIndex)
    {
        List<Vector2> pointList = new List<Vector2>();

        Vector2 left = CalculateBezierPoint(curveIndex, 0);
        Vector2 right = CalculateBezierPoint(curveIndex, 1);

        pointList.Add(left);
        pointList.Add(right);

        FindDrawingPoints(curveIndex, 0, 1, pointList, 1);

        return pointList;
    }


    /**
        @returns the number of points added.
    */
    int FindDrawingPoints(int curveIndex, float t0, float t1,
        List<Vector2> pointList, int insertionIndex)
    {
        Vector2 left = CalculateBezierPoint(curveIndex, t0);
        Vector2 right = CalculateBezierPoint(curveIndex, t1);

        if ((left - right).sqrMagnitude < MINIMUM_SQR_DISTANCE)
        {
            return 0;
        }

        float tMid = (t0 + t1) / 2;
        Vector2 mid = CalculateBezierPoint(curveIndex, tMid);

        Vector2 leftDirection = (left - mid).normalized;
        Vector2 rightDirection = (right - mid).normalized;

        if (Vector2.Dot(leftDirection, rightDirection) > DIVISION_THRESHOLD || Mathf.Abs(tMid - 0.5f) < 0.0001f)
        {
            int pointsAddedCount = 0;

            pointsAddedCount += FindDrawingPoints(curveIndex, t0, tMid, pointList, insertionIndex);
            pointList.Insert(insertionIndex + pointsAddedCount, mid);
            pointsAddedCount++;
            pointsAddedCount += FindDrawingPoints(curveIndex, tMid, t1, pointList, insertionIndex + pointsAddedCount);

            return pointsAddedCount;
        }

        return 0;
    }



    /**
        Caluclates a point on the Bezier curve represented with the four controlpoints given.
    */
    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * p0; //first term

        p += 3 * uu * t * p1; //second term
        p += 3 * u * tt * p2; //third term
        p += ttt * p3; //fourth term

        return p;

    }
}

[System.Serializable]
public class CableCurve
{
    [SerializeField]
    Vector2 m_start;
    [SerializeField]
    Vector2 m_end;
    [SerializeField]
    float m_slack;
    [SerializeField]
    int m_steps;
    [SerializeField]
    bool m_regen;

    static Vector2[] emptyCurve = new Vector2[] { new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f) };
    [SerializeField]
    Vector2[] points;

    public bool regenPoints
    {
        get { return m_regen; }
        set
        {
            m_regen = value;
        }
    }

    public Vector2 start
    {
        get { return m_start; }
        set
        {
            if (value != m_start)
                m_regen = true;
            m_start = value;
        }
    }

    public Vector2 end
    {
        get { return m_end; }
        set
        {
            if (value != m_end)
                m_regen = true;
            m_end = value;
        }
    }
    public float slack
    {
        get { return m_slack; }
        set
        {
            if (value != m_slack)
                m_regen = true;
            m_slack = Mathf.Max(0.0f, value);
        }
    }
    public int steps
    {
        get { return m_steps; }
        set
        {
            if (value != m_steps)
                m_regen = true;
            m_steps = Mathf.Max(2, value);
        }
    }

    public Vector2 midPoint
    {
        get
        {
            Vector2 mid = Vector2.zero;
            if (m_steps == 2)
            {
                return (points[0] + points[1]) * 0.5f;
            }
            else if (m_steps > 2)
            {
                int m = m_steps / 2;
                if ((m_steps % 2) == 0)
                {
                    mid = (points[m] + points[m + 1]) * 0.5f;
                }
                else
                {
                    mid = points[m];
                }
            }
            return mid;
        }
    }

    public CableCurve()
    {
        points = emptyCurve;
        m_start = Vector2.up;
        m_end = Vector2.up + Vector2.right;
        m_slack = 0.5f;
        m_steps = 20;
        m_regen = true;
    }

    public CableCurve(Vector2[] inputPoints)
    {
        points = inputPoints;
        m_start = inputPoints[0];
        m_end = inputPoints[1];
        m_slack = 0.5f;
        m_steps = 20;
        m_regen = true;
    }

    public CableCurve(CableCurve v)
    {
        points = v.Points();
        m_start = v.start;
        m_end = v.end;
        m_slack = v.slack;
        m_steps = v.steps;
        m_regen = v.regenPoints;
    }

    public Vector2[] Points()
    {
        if (!m_regen)
            return points;

        if (m_steps < 2)
            return emptyCurve;

        float lineDist = Vector2.Distance(m_end, m_start);
        float lineDistH = Vector2.Distance(new Vector2(m_end.x, m_start.y), m_start);
        float l = lineDist + Mathf.Max(0.0001f, m_slack);
        float r = 0.0f;
        float s = m_start.y;
        float u = lineDistH;
        float v = end.y;

        if ((u - r) == 0.0f)
            return emptyCurve;

        float ztarget = Mathf.Sqrt(Mathf.Pow(l, 2.0f) - Mathf.Pow(v - s, 2.0f)) / (u - r);

        int loops = 30;
        int iterationCount = 0;
        int maxIterations = loops * 10; // For safety.
        bool found = false;

        float z = 0.0f;
        float ztest = 0.0f;
        float zstep = 100.0f;
        float ztesttarget = 0.0f;
        for (int i = 0; i < loops; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                iterationCount++;
                ztest = z + zstep;
                ztesttarget = (float)Math.Sinh(ztest) / ztest;

                if (float.IsInfinity(ztesttarget))
                    continue;

                if (ztesttarget == ztarget)
                {
                    found = true;
                    z = ztest;
                    break;
                }
                else if (ztesttarget > ztarget)
                {
                    break;
                }
                else
                {
                    z = ztest;
                }

                if (iterationCount > maxIterations)
                {
                    found = true;
                    break;
                }
            }

            if (found)
                break;

            zstep *= 0.1f;
        }

        float a = (u - r) / 2.0f / z;
        float p = (r + u - a * Mathf.Log((l + v - s) / (l - v + s))) / 2.0f;
        float q = (v + s - l * (float)Math.Cosh(z) / (float)Math.Sinh(z)) / 2.0f;

        points = new Vector2[m_steps];
        float stepsf = m_steps - 1;
        float stepf;
        for (int i = 0; i < m_steps; i++)
        {
            stepf = i / stepsf;
            Vector2 pos = Vector2.zero;
            pos.x = Mathf.Lerp(start.x, end.x, stepf);
            pos.y = a * (float)Math.Cosh(((stepf * lineDistH) - p) / a) + q;
            points[i] = pos;
        }

        m_regen = false;
        return points;
    }
}
