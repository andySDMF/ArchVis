using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MWM.Procedure;

public class Procedure : MonoBehaviour, IProcedure
{
	[SerializeField]
	private UnityEvent onBegin = new UnityEvent();

	[Header("Debug Mode")]
	[SerializeField]
	private bool debugOn = false;

	public bool HasStarted { get; private set; }

	public bool Done { get; private set; }

	public void Begin()
	{
		HasStarted = true;

		if (debugOn) Debug.Log("Procedure [" + gameObject.name + "] has been started");

		onBegin.Invoke();
	}

	public void Handle(string procedure)
	{
		Done = true;

		if (debugOn) Debug.Log("Procedure [" + gameObject.name + "] has been handled");
	}

	public void Reset()
	{
		HasStarted = false;
		Done = false;

		if (debugOn) Debug.Log("Procedure [" + gameObject.name + "] has been reset");
	}
}
