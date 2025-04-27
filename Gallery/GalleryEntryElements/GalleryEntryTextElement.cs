using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.Gallery;

[RequireComponent(typeof(Text))]
public class GalleryEntryTextElement : GalleryBaseEntryElement, IGalleryElement 
{
	public string Reference { get { return reference; } }

	private Text textScript;

	private void OnDisable()
	{
		if (textScript != null)
			textScript.text = "";
	}

	public void Recieve (string data)
	{
		if(string.IsNullOrEmpty(data))
			return;

		if (textScript == null)
			textScript = GetComponent<Text> ();

		if (textScript != null) 
		{
			textScript.text = data;
		}
	}

	public void Clear()
	{
		if (textScript != null)
			textScript.text = "";
	}
}
