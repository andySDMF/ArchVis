using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.Gallery;

[RequireComponent(typeof(Image))]
public class GalleryEntrySpriteElement : GalleryBaseEntryElement, IGalleryElement
{
	[Header("Sprite")]
	[SerializeField]
	private int index = 0;

	public string Reference { get { return reference; } }

	private Image imageScript;

	private void OnDisable()
	{
		if (imageScript != null)
			imageScript.sprite = null;
	}

	public void Recieve (string data)
	{
		if(string.IsNullOrEmpty(data))
			return;

		if (imageScript == null)
			imageScript = GetComponent<Image> ();

		if (imageScript != null) 
		{
			Sprite[] sprites = Resources.LoadAll<Sprite>(data);

			if (sprites.Length > 0 && (index >= 0 && index < sprites.Length - 1)) 
			{
				imageScript.sprite = sprites[index];
			}
		}
	}

	public void Clear()
	{
		if (imageScript != null)
			imageScript.sprite = null;
	}
}
