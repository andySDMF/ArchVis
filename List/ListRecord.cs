using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using MWM.DynamicList;

public class ListRecord : MonoBehaviour, IDynamicList, IDynamicRecord
{
    [SerializeField]
    private string prefabName = "Default";

	[SerializeField]
	private UnityEvent onCreate = new UnityEvent();

	[SerializeField]
	private UnityEvent onDelete = new UnityEvent();

    public string Reference { get { return prefabName; } }
	
	public string ID { get; private set; }

	public LRecord Record { get; private set; }

	private string rawSource = "";

	public void Create(string source)
	{
		rawSource = source;
		Record = JsonUtility.FromJson<LRecord> (source);

		ID = Record.id;

		Apply();
	
		onCreate.Invoke ();

		gameObject.SetActive(true);
	}

	public void Delete(bool clear = true)
	{
		onDelete.Invoke ();

		Destroy(gameObject);
	}

	public void Sort(string data)
	{

	}

	public void Apply(string data = "All")
	{
		if (Record == null)
			return;

		IDynamicRecord[] elements = GetElements();

		if (data.Equals ("All")) 
		{
			if (elements != null && elements.Length > 0) 
			{
				for (int i = 0; i < elements.Length; i++) 
				{
					LRecordElement rElement = Record.elements.FirstOrDefault (e => e.id.Equals (elements [i].ID));

					if (rElement != null) 
					{
						elements [i].Apply(rElement.data);
					}
				}
			}
		} 
		else 
		{
			if (elements != null && elements.Length > 0) 
			{
				for (int i = 0; i < elements.Length; i++) 
				{
					LRecordElement rElement = Record.elements.FirstOrDefault (e => e.id.Equals (elements [i].ID));

					if (rElement != null) 
					{
						elements [i].Apply(rElement.data);
						break;
					}
				}
			}
		}
	}

	private IDynamicRecord[] GetElements()
	{
		List<IDynamicRecord> tList = new List<IDynamicRecord> ();

		foreach (Transform t in gameObject.GetComponentsInChildren<Transform>(true)) 
		{
			if (t.Equals (this.transform))
				continue;

			IDynamicRecord rec = (IDynamicRecord)t.GetComponent(typeof(IDynamicRecord));

			if (rec != null) 
			{
				tList.Add (rec);
			}
		}

		return tList.ToArray ();
	}
}
