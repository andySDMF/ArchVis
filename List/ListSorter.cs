using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MWM.DynamicList;

public class ListSorter : MonoBehaviour, IDynamicSorter
{
	[SerializeField]
	private string defaultMethod = "";

	public string Sort(LSort info)
	{
		if (info == null)
			return "";

		LRecordGroup recordGroup = JsonUtility.FromJson<LRecordGroup>(info.data);

		if (recordGroup != null) 
		{
			if (recordGroup.records == null) 
			{
				return info.data;
			}

			if (string.IsNullOrEmpty(info.method)) 
			{
				if(string.IsNullOrEmpty(defaultMethod)) return JsonUtility.ToJson(recordGroup);
				else info.method = defaultMethod;
			} 

			if(info.order == OrderMethod._Ascending) recordGroup.records.Sort((x, y) => x.GetElement(info.method).CompareTo(y.GetElement(info.method)));
			else recordGroup.records.Sort((x, y) => y.GetElement(info.method).CompareTo(x.GetElement(info.method)));
		}

		return JsonUtility.ToJson(recordGroup);
	}
}
