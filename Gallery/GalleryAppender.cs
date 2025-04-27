using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.Gallery;

public class GalleryAppender : MonoBehaviour
{
	[SerializeField]
	private Gallery append;

    [SerializeField]
	public List<GEntry> entries = new List<GEntry>();

	public void Append()
	{
		GEntryGroup eGroup = new GEntryGroup ();
		eGroup.id = append.ID;
		eGroup.entries = new List<GEntry> ();

		entries.ForEach (e => eGroup.entries.Add(e));

		if(append != null)
		{
			append.Append(JsonUtility.ToJson(eGroup));
        }
	}
}
