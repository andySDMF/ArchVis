using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GalleryIndicator : MonoBehaviour 
{
	[SerializeField]
	private UnityEvent onTrue = new UnityEvent ();

	[SerializeField]
	private UnityEvent onFalse = new UnityEvent ();

	public void Set(bool state)
	{
		if (state)
			onTrue.Invoke ();
		else
			onFalse.Invoke ();
	}
}
