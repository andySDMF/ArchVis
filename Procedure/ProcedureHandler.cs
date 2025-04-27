using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MWM.Procedure;

public class ProcedureHandler : MonoBehaviour, IProcedure
{
	[SerializeField]
	private bool autoBegin = false;

	[SerializeField]
	private List<ProcedureEvent> events = new List<ProcedureEvent>();

	[Header("")]
	[SerializeField]
	private UnityEvent onEnd = new UnityEvent();

	[Header("Debug Mode")]
	[SerializeField]
	private bool debugOn = false;

	public bool HasStarted { get; private set; }

	public bool Done { get; private set; }

	private Queue procedureStack = new Queue ();
	private ProcedureEvent current;

	private void Start()
	{
        ProcedureUtils.Add(gameObject.name, (IProcedure)this);

		if (autoBegin) Begin();
	}

	public void Begin()
	{
        if (Done) return;

        if (HasStarted) 
		{
			if (debugOn) Debug.Log("Procedures processing. Cancelled call");
			return;
		}
			
		events.ForEach(e => procedureStack.Enqueue(e));

		if (procedureStack.Count > 0) 
		{
			ProcedureEvent pEvent = (ProcedureEvent)procedureStack.Dequeue ();

			if (debugOn) Debug.Log ("Processing next procedure");

            pEvent.HandlerName = gameObject.name;
            pEvent.Begin();

			current = pEvent;
		} 
		else 
		{
            current = null;

            if (debugOn) Debug.Log("Procedures completed");

            onEnd.Invoke();
        }
	}

	public void Handle(string procedure)
	{
        if (Done) return;

		if (string.IsNullOrEmpty(procedure)) 
		{
			if (debugOn) Debug.Log("Procedure cannot be null or empty. Cancelled call");
			return;
		}

		if(current != null)
		{
			current.Handle(procedure);
		}

		if (procedureStack.Count > 0) 
		{
			current = (ProcedureEvent)procedureStack.Dequeue ();

			if (debugOn) Debug.Log ("Processing next procedure");

            current.HandlerName = gameObject.name;
            current.Begin ();
		} 
		else 
		{
			current = null;

			if (debugOn) Debug.Log("Procedures completed");

            Done = true;

			onEnd.Invoke();
		}
	}

	public void Reset()
	{
		if (debugOn) Debug.Log("Resetting all procedures");

		events.ForEach(e => e.Reset());

        Done = false;

        HasStarted = false;
    }

	[System.Serializable]
	private class ProcedureEvent : IProcedure
	{
		[SerializeField]
		private string id;

		[Header("Push")]
		[SerializeField]
		private ProcedureType type = ProcedureType._Procedure;

		[SerializeField]
		private GameObject target;

		[SerializeField]
		private string command = "";

        [SerializeField]
        private bool ignore = false;

		public bool HasStarted { get; private set; }

		public bool Done { get; private set; }

        public string HandlerName { get; set; }

		public void Begin()
		{
			HasStarted = true;

            if (target == null || ignore)
            {
                ProcedureUtils.Interfaces[HandlerName].Handle(id);
                return;
            }

			switch (type) 
			{
				case ProcedureType._Push:

				target.SendMessage (command);

				break;

			default:

				IProcedure procedure = (IProcedure)target.GetComponent (typeof(IProcedure));

				if (procedure != null) procedure.Begin();

				break;
			}
		}

		public void Handle(string procedure)
		{
			if (!id.Equals(procedure)) return;

			Done = true;

			if (type == ProcedureType._Procedure) 
			{
				IProcedure iPro = (IProcedure)target.GetComponent(typeof(IProcedure));

				if (iPro != null) iPro.Handle(procedure);
			}
		}

		public void Reset()
		{
			HasStarted = false;
			Done = false;

			if (type == ProcedureType._Procedure) 
			{
				IProcedure procedure = (IProcedure)target.GetComponent(typeof(IProcedure));

				if (procedure != null) procedure.Reset();
			}
		}

		[System.Serializable]
		private enum ProcedureType { _Procedure, _Push }
	}
}
