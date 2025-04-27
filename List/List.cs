using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MWM.DynamicList;

public class List : MonoBehaviour, IDynamicList, IDynamicPageControl
{
	[SerializeField]
	private Transform listContainer;

	[SerializeField]
	private List<RecordPrefab> listObjects;

	[SerializeField]
	private GameObject listSorter;

	[SerializeField]
	private OrderMethod orderMethod = OrderMethod._Ascending;

	[SerializeField]
	private ListAttributes listAttributes = new ListAttributes();

	private List<List<LRecord>> records = new List<List<LRecord>>();
	private List<GameObject> visibleRecords = new List<GameObject>();

	private string rawSource = "";
	private int currentPage = 0;

    private bool cancel = false;
    private string cachePrefab = "";

	public void Create(string source)
	{
		if (string.IsNullOrEmpty(source)) 
		{
			return;
		}

		Delete();

		LRecordGroup recordGroup = JsonUtility.FromJson<LRecordGroup>(source);

		if (recordGroup != null) 
		{
			if (listObjects.Count <= 0 || recordGroup.records == null) 
			{
				return;
			}

			rawSource = source;
			List<LRecord> recs = new List<LRecord>();

            cachePrefab = recordGroup.prefab;

			if (listAttributes.usePages) 
			{
				int count = 0;
				currentPage = 0;

				for (int i = 0; i < recordGroup.records.Count; i++) 
				{
					if (count < listAttributes.recordsPerPage) 
					{
						recs.Add(recordGroup.records[i]);
			
						count++;

						if (i.Equals (recordGroup.records.Count - 1))
							records.Add (recs);
					} 
					else 
					{
						records.Add(recs);
						recs = new List<LRecord>();

						recs.Add(recordGroup.records[i]);

						count = 0;
					}
				}

				StartCoroutine(Process(records[currentPage]));
			} 
			else 
			{
				recordGroup.records.ForEach(r => recs.Add(r));
				records.Add(recs);

				StartCoroutine(Process(recordGroup.records));
			}
		}
	}

	public void Delete(bool clear = true)
	{
		if (records.Count < 1) 
		{
            cancel = false;
            return;
		}

        cancel = true;

        foreach (GameObject rec in visibleRecords)
		{
			IDynamicList lObject = (IDynamicList)rec.GetComponent(typeof(IDynamicList));

			if (lObject != null) lObject.Delete ();
		}

        visibleRecords.Clear();

		if(clear) records.Clear();
	}

	public void Sort(string data)
	{
		if (listSorter != null && records.Count > 0) 
		{
			IDynamicSorter iSorter = (IDynamicSorter)listSorter.GetComponent (typeof(IDynamicSorter));

			if (iSorter != null) 
			{
				string output = iSorter.Sort(new LSort(rawSource, data, orderMethod));

				Create(output);
			}
		}
	}

	public void Previous()
	{
		if (currentPage <= 0)
			return;

		Delete(false);

		StartCoroutine(Process(records[--currentPage]));
	}

	public void Next()
	{
		if (currentPage >= records.Count - 1)
			return;
		
		Delete(false);

		StartCoroutine(Process(records[++currentPage]));
	}

	public void OrderBy(int val)
	{
		orderMethod = (val < 1) ? OrderMethod._Ascending : OrderMethod._Descending;

		Sort("");
	}

	private IEnumerator Process(List<LRecord> recs)
	{
		WaitForSeconds wait = new WaitForSeconds (listAttributes.interval);

        cancel = false;

        GameObject prefab = listObjects.FirstOrDefault(o => o.name.Equals(cachePrefab)).prefab;

        if (prefab == null) yield break;

        for (int i = 0; i < recs.Count; i++) 
		{
			GameObject go = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;

			go.transform.SetParent((listContainer == null) ? this.transform : listContainer);
			go.transform.localScale = Vector3.one;
			go.name = (string.IsNullOrEmpty (recs[i].id)) ? "Record " + i.ToString () : recs[i].id;

			IDynamicList lObject = (IDynamicList)go.GetComponent (typeof(IDynamicList));

			if (lObject != null)
				lObject.Create (JsonUtility.ToJson (recs[i]));

            if (cancel)
            {
                Destroy(go);
                break;
            }

            visibleRecords.Add(go);

            if (i < recs.Count) 
			{
				yield return wait;
			}
		}

        AmendAttributes();
	}

    private void AmendAttributes()
    {
        if (records.Count <= 1)
        {
            if (listAttributes.nextButton != null)
                listAttributes.nextButton.interactable = false;

            if (listAttributes.previousButton != null)
                listAttributes.previousButton.interactable = false;

            return;
        }

        if (currentPage.Equals(0))
        {
            if (listAttributes.previousButton != null)
                listAttributes.previousButton.interactable = false;

            if (records.Count > 1)
            {
                if (listAttributes.nextButton != null)
                    listAttributes.nextButton.interactable = true;
            }
        }
        else if (currentPage >= records.Count - 1)
        {
            if (listAttributes.nextButton != null)
                listAttributes.nextButton.interactable = false;

            if (listAttributes.previousButton != null)
                listAttributes.previousButton.interactable = true;
        }
        else
        {
            if (listAttributes.nextButton != null)
                listAttributes.nextButton.interactable = true;

            if (listAttributes.previousButton != null)
                listAttributes.previousButton.interactable = true;
        }
    }

    [System.Serializable]
    private class RecordPrefab
    {
        public string name = "Default";
        public GameObject prefab;
    }
}
