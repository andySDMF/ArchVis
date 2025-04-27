using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoJsonConverter : MonoBehaviour 
{
	[SerializeField]
	private VideoObject videoObject;

	[Header("Push")]
	[SerializeField]
	private GameObject target;

	public void ToJson(string reference)
	{
        if (string.IsNullOrEmpty(reference)) return;

        CGroup cGroup = JsonUtility.FromJson<CGroup>(reference);

        if(cGroup != null)
        {
            reference = cGroup.entries[0].elements[0].data;
        }

		if (target != null)
			target.SendMessage ("Load", reference);
	}

	public void FromJson(string reference)
	{
		if (videoObject == null)
			return;

		videoObject.Override(reference);
	}

    [System.Serializable]
    private class CGroup
    {
        public string id;
        public List<CEntry> entries;
    }

    [System.Serializable]
    private class CEntry
    {
        public string id;
        public List<CEntryElement> elements;
    }

    [System.Serializable]
    private class CEntryElement
    {
        public string id;
        public string data;
    }
}
