using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Line;

public class LineController : MonoBehaviour, ILine
{
	[SerializeField]
	private string id = "";

    [SerializeField]
	private List<Line> lines = new List<Line>();

	[Header("Append")]
	[SerializeField]
	private bool createOnAppend = false;

	[SerializeField]
	private bool drawOnAppend = false;

	[Header("Distribution")]
	[SerializeField]
	private LineDistribution distribution;

	[SerializeField]
	private bool distributionOverride = false;

    [Header("Display")]
    [SerializeField]
    private LineColor color;

    [SerializeField]
    private bool colorOverride = false;

    [SerializeField]
    private string materialName = "";

    [SerializeField]
    private bool materialOverride = false;

    [Header("Create")]
    [SerializeField]
	private Transform container;

	[SerializeField]
	private GameObject prefab;

	public string ID { get { return id; } }

	private string cacheData = "";

    public void Append(string data)
    {
		if (string.IsNullOrEmpty(data))
			return;

		Clear();

		LineGroup lGroup = JsonUtility.FromJson<LineGroup> (data);

		if (lGroup == null)
			return;

		lGroup.lines.ForEach (l => lines.Add(l));

		if (createOnAppend)
			Create (drawOnAppend);
    }

	public void Create (bool draw = false)
	{
		if (prefab == null)
			return;

        lines.ForEach(l => l.Clear());

        foreach (Line line in lines) 
		{
			GameObject go = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
			go.name = "Line_" + line.id;

			go.transform.SetParent ((container != null) ? container : transform);
			go.transform.position = new Vector3(line.settings.points[0].Get().x, 0, line.settings.points[0].Get().z);
			go.transform.localScale = Vector3.one;
			go.transform.localEulerAngles = Vector3.zero;

            ILine lInterface = (ILine)go.GetComponent(typeof(ILine));

            if (distributionOverride) line.settings.distribution = distribution;
            if (materialOverride) line.material = materialName;
            if (colorOverride) line.color = color;

            line.Obj = go;

			if (lInterface != null) 
			{
				lInterface.Append (JsonUtility.ToJson (line));
				lInterface.Create (false);
			}
		}

		if (draw) Draw ();
	}

    public void Draw()
    {
		if (lines == null || lines.Count <= 0)
			return;

		foreach (Line line in lines) 
		{
			if (line.Obj == null)
				continue;

			ILine lInterface = (ILine)line.Obj.GetComponent(typeof(ILine));

			if (lInterface != null) 
			{
				lInterface.Draw();
			}
		}
    }

    public void Hide()
    {
        if (lines == null || lines.Count <= 0)
            return;

        foreach (Line line in lines)
        {
            if (line.Obj == null)
                continue;

            ILine lInterface = (ILine)line.Obj.GetComponent(typeof(ILine));

            if (lInterface != null)
            {
                lInterface.Hide();
            }
        }
    }

    public void Clear()
    {
		lines.ForEach (l => l.Clear());

		lines.Clear();
    }

    public void Animate(float delay)
    {
        if (lines == null || lines.Count <= 0)
            return;

        foreach (Line line in lines)
        {
            if (line.Obj == null)
                continue;

            ILine lInterface = (ILine)line.Obj.GetComponent(typeof(ILine));

            if (lInterface != null)
            {
                lInterface.Animate(delay);
            }
        }
    }
}
