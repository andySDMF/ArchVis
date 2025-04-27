using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.DynamicList;

public class ListTest : MonoBehaviour 
{
	[SerializeField]
	private GameObject listContainer;

    [SerializeField]
    private string prefabName = "Default";

	[SerializeField]
	private List<LRecord> records = new List<LRecord>();

	public void Create()
	{
		LRecordGroup lGroup = new LRecordGroup ();

		lGroup.id = "Test";
		lGroup.records = records;
        lGroup.prefab = prefabName;

		if (listContainer != null) 
		{
			IDynamicList l = (IDynamicList)listContainer.GetComponent (typeof(IDynamicList));

			if (l != null) l.Create (JsonUtility.ToJson (lGroup));
		}
	}
}
