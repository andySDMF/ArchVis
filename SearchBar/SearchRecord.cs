using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Grain.SearchBar;
using UnityEngine.Events;
using TMPro;

public class SearchRecord : MonoBehaviour, ISearch
{
    [SerializeField]
    private RecordData data;

    [SerializeField]
    private RecordData dataFiltered;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onShow = new UnityEvent();

    [SerializeField]
    private UnityEvent onHide = new UnityEvent();

    [SerializeField]
    private UnityEvent onPublish = new UnityEvent();

    [Header("UI Elements")]
    [SerializeField]
    private List<RecordUIElements> displayElements = new List<RecordUIElements>();

    public RecordData Data { get { return data; } }
    public string ID { get { return gameObject.name; } }

    public MonoBehaviour Component
    {
        get
        {
            return this;
        }
    }

    private SearchBar searchBar;

    private void Start()
    {
        if(searchBar == null) searchBar = GetComponentInParent<SearchBar>();
    }

    public void Publish(string rawData)
    {
        if(string.IsNullOrEmpty(rawData)) return;

        searchBar = GetComponentInParent<SearchBar>();

        data = JsonUtility.FromJson<RecordData>(rawData);

        //only need to search against these two vals
        dataFiltered.values.Add(data.values[0]);
        dataFiltered.values.Add(data.values[1]);


        foreach (RecordSingleData sData in data.values)
        {
            RecordUIElements ui = displayElements.FirstOrDefault(e => e.key.Equals(sData.key));

            if(ui != null && ui.asset != null)
            {
                if(ui.asset is TextMeshProUGUI)
                {
                    ((TextMeshProUGUI)ui.asset).text = sData.value;
                }
                else if(ui.asset is Text)
                {
                    ((Text)ui.asset).text = sData.value;
                }
                else if(ui.asset is Image)
                {
                    ((Image)ui.asset).sprite = Resources.Load<Sprite>(sData.value);
                }
                else if (ui.asset is RawImage)
                {
                    ((RawImage)ui.asset).texture = Resources.Load(sData.value) as Texture;
                }
                else
                {
                    Debug.Log(sData.key + ", " + sData.value);
                }
            }
        }

        onPublish.Invoke();
    }

    public bool Display(string criteria)
    {
        if (dataFiltered == null || dataFiltered.values == null) return false;

        foreach(RecordSingleData d in dataFiltered.values)
        {
            //can comment out for now no need to do check as looping through a filtered list

            //if (searchBar.IgnoreValues.Contains(d.key))
            //{
            //    continue;
            //}

            if (d.value.ToLower().StartsWith(criteria.ToLower())) return true;
        }

        return false;
    }

    public void UpdateDataValue(string k, string v)
    {
        RecordSingleData rData = data.values.FirstOrDefault(d => d.key.Equals(k));

        if (rData != null) rData.value = v;
    }

    public void Show()
    {
        onShow.Invoke();
    }

    public void Hide()
    {
        onHide.Invoke();
    }
}
