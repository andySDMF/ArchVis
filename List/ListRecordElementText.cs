using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MWM.DynamicList;

public class ListRecordElementText : MonoBehaviour, IDynamicRecord
{
	[SerializeField]
	private string id = "";

	public string ID { get { return id; } }

	public LRecord Record { get; private set; }

	private Text textScript;

	public void Apply(string data = "All")
	{
		if (textScript == null)
			textScript = GetComponent<Text> ();

		if (textScript != null) 
		{
			textScript.text = data;
		}
	}
}
