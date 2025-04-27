using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Grain.Gallery;

public class GalleryIndicatorControl : MonoBehaviour, IGallery
{
	[SerializeField]
	private GameObject indicatorPrefab;

	public string ID { get { return gameObject.name; } }

    public System.Action<int> OnPublishDelegate;

    private List<GalleryIndicator> indicators = new List<GalleryIndicator> ();

	public void Append(string data)
	{
		if (string.IsNullOrEmpty(data))
			return;

		if (indicatorPrefab == null || indicatorPrefab.GetComponent<GalleryIndicator> () == null)
			return;

        int n;

		if (!int.TryParse(data, out n))
			return;

		if (n > 1) 
		{
			for (int i = 0; i < n; i++) 
			{
				GameObject go = Instantiate (indicatorPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
				go.transform.localScale = Vector3.one;
				go.transform.localPosition = Vector3.zero;
				go.SetActive (true);

				indicators.Add (go.GetComponent<GalleryIndicator> ());
			}
		}
	}

	public void Publish(int control = -1)
	{
		if (indicators.Count < 1)
        {
            if(OnPublishDelegate != null) OnPublishDelegate.Invoke(control);
            return;
        }

		for (int i = 0; i < indicators.Count; i++) 
		{
			if (i.Equals (control))
				indicators [i].Set (true);
			else
				indicators [i].Set (false);
		}

        if (OnPublishDelegate != null) OnPublishDelegate.Invoke(control);
    }

	public void Clear()
	{
		indicators.ForEach (i => Destroy (i.gameObject));

		indicators.Clear ();
	}

    public void Jump(int val)
    {

    }
}
