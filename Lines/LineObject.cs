using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MWM.Line;

[RequireComponent(typeof(LineRenderer))]
public class LineObject : MonoBehaviour, ILine
{
	[SerializeField]
	private Line line;

    [SerializeField]
    private UnityEngine.Events.UnityEvent onStart = new UnityEngine.Events.UnityEvent();

    [SerializeField]
    private UnityEngine.Events.UnityEvent onStop = new UnityEngine.Events.UnityEvent();

	public string ID { get { return (line != null) ? line.id : "Null"; } }

	public bool IsDrawing { get; private set; }

	private List<GameObject> segments = new List<GameObject>();
	private LineRenderer lineRenderer;

    private System.Action<Vector3> delegates;
    private System.Action<Color> colorSetter;

    private bool cancel = false;

	public void Append(string data)
	{
		if (string.IsNullOrEmpty(data))
			return;

		Clear ();

		line = JsonUtility.FromJson<Line>(data);

		line.Obj = this.gameObject;

		if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer> ();
	}

	public void Create (bool draw = false)
	{
		if (line == null)
			return;

		Clear ();

		for(int i = 0; i < line.settings.points.Count; i++) 
		{
			var go = new GameObject ("Segment_" + i.ToString ());

			go.transform.SetParent (this.transform);
			go.transform.position = line.settings.points [i].Get ();
			go.transform.localScale = Vector3.one;
			go.transform.localEulerAngles = Vector3.zero;

            go.layer = 9;

			segments.Add(go);
		}
	}

	public void Draw()
	{
		if (!gameObject.activeSelf || line == null)
			return;

		if (IsDrawing)
			return;

		IsDrawing = true;

        cancel = false;

        gameObject.layer = LayerMask.NameToLayer("NoRender");

        lineRenderer.positionCount = 0;

        lineRenderer.startWidth = line.settings.distribution.width;
        lineRenderer.endWidth = line.settings.distribution.width;
        lineRenderer.material = Resources.Load(line.material) as Material;

        Color32 col = new Color32((byte)line.color.r, (byte)line.color.g, (byte)line.color.b, 255);

        lineRenderer.material.color = (Color)col;

        if (colorSetter != null)
        {
            colorSetter.Invoke(lineRenderer.material.color);
        }

        StartCoroutine(Process());
	}

    public void Hide()
    {
        cancel = true;

        lineRenderer.startWidth = 0.0f;
        lineRenderer.endWidth = 0.0f;

        gameObject.layer = LayerMask.NameToLayer("NoRender");

        IsDrawing = false;

        lineRenderer.positionCount = 0;
    }

    public void Clear()
	{
		if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer> ();

        lineRenderer.startWidth = 0.0f;
        lineRenderer.endWidth = 0.0f;

		IsDrawing = false;

		segments.ForEach (s => Destroy (s));

		segments.Clear ();
	}

    public void Animate(float delay)
    {
        StartCoroutine(AnimationProcess(delay));
    }

    public void AddDelegate(System.Action<Vector3> action)
    {
        delegates += action;
    }

    public void AddColorSetter(System.Action<Color> action)
    {
        colorSetter += action;
    }

    private IEnumerator AnimationProcess(float delay)
    {
        yield return new WaitForSeconds(0.2f);

        float time = 0.0f;

        cancel = false;

        while (time < delay)
        {
            if (cancel) break;

            time += Time.deltaTime;
            yield return null;

            if (cancel) break;
        }

        print("Cancel state : " + cancel);

        if (cancel)
        {
            yield break;
        }

        Draw();
    }

    private IEnumerator Process()
	{
		int count = 0;

		yield return new WaitForSeconds (0.2f);

        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, line.settings.points[0].Get());

		yield return new WaitForSeconds (line.settings.distribution.delay);

        gameObject.layer = LayerMask.NameToLayer("Lines");

        Vector3 vec = Vector3.zero;

		while (count < line.settings.points.Count - 1) 
		{
			yield return StartCoroutine (Lerp (lineRenderer, count, line.settings.points [count + 1].Get(), line.settings.distribution.speed));

			if (!IsDrawing)
				break;

			count++;

			if (lineRenderer.positionCount < line.settings.points.Count)
                lineRenderer.positionCount++;
		}

		IsDrawing = false;
	}

	private IEnumerator Lerp(LineRenderer r, int n, Vector3 pos, float speed)
	{
		float t = 0.0f;
		Vector3 v = Vector3.zero;

		while (t < 1.0f) 
		{
			t += (r.positionCount * Time.deltaTime) * speed;

			if (n > r.positionCount || !IsDrawing)
				break;

			v = Vector3.Lerp (r.GetPosition (n), pos, t);
            r.SetPosition (n + 1, new Vector3(v.x, v.y, v.z));

            if(delegates != null)
            {
                delegates.Invoke(v);
            }

			if (!IsDrawing)
				break;

			yield return null;
		}

        onStop.Invoke();
	}
}
