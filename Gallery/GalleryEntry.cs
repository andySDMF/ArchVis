using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Grain.Gallery;

public class GalleryEntry : MonoBehaviour, IGallery
{
	[SerializeField]
	private GEntry entry = new GEntry();

	public string ID { get { return entry.id; } }

	private bool publish = false;
	private string cacheID = "";

	public void Append(string data)
	{
		if (string.IsNullOrEmpty(data))
			return;

		entry = JsonUtility.FromJson<GEntry>(data);

		publish = (cacheID.Equals (ID)) ? false : true;
			
		cacheID = ID;
	}

	public void Publish(int control = -1)
	{
		if (!publish)
			return;

		List<IGalleryElement> gElements = GalleryUtils.GetInterfaces<IGalleryElement>(gameObject);

		for (int i = 0; i < gElements.Count; i++) 
		{
			gElements [i].Recieve (entry.elements.FirstOrDefault (e => e.id.Equals (gElements [i].Reference)).data);
		}
	}

	public void Clear()
	{
		cacheID = "";
		entry.id = "";

        List<IGalleryElement> gElements = GalleryUtils.GetInterfaces<IGalleryElement>(gameObject);

        for (int i = 0; i < gElements.Count; i++)
        {
            gElements[i].Clear();
        }

        entry.elements.Clear();
		publish = false;
	}
}
