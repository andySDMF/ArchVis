using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Line;

public class LineTest : MonoBehaviour 
{
	[SerializeField]
	private List<Line> lines = new List<Line>();

	[SerializeField]
	private LineController controller;

	public void Post()
	{
		if (controller == null)
			return;

		LineGroup lGroup = new LineGroup ();
		lGroup.lines = lines;

		controller.Append (JsonUtility.ToJson(lGroup));
	}
}
