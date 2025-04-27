using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MWM.DynamicList
{
	public enum OrderMethod { _Ascending, _Descending }

	public interface IDynamicList
	{
		void Create(string source);

		void Delete(bool clear = true);

		void Sort(string data);
	}

	public interface IDynamicRecord
	{
		string ID { get; }

		LRecord Record { get; }

		void Apply(string data = "All");
	}

	public interface IDynamicSorter
	{
		string Sort(LSort info);
	}

	public interface IDynamicPageControl
	{
		void Previous();
		
		void Next();
	}

	[System.Serializable]
	public class ListAttributes
	{
		[Header("On Create")]
		public float interval = 0.0f;

		[Header("Pages")]
		public bool usePages = true;
		public int recordsPerPage = 25;
		public Button previousButton;
		public Button nextButton;
	}

	[System.Serializable]
	public class LRecordGroup
	{
		public string id;
        public string prefab;
		public List<LRecord> records;
	}

	[System.Serializable]
	public class LRecord
	{
		public string id;
		public List<LRecordElement> elements;

		public string GetElement(string key)
		{
			return elements.FirstOrDefault(e => e.id.Equals(key)).data;
		}
	}

	[System.Serializable]
	public class LRecordElement
	{
		public string id;
		public string data;
	}

	[System.Serializable]
	public class LSort
	{
		public string data;
		public string method;
		public OrderMethod order;

		public LSort(string d, string m, OrderMethod o)
		{
			data = d;
			method = m;
			order = o;
		}
	}
}
