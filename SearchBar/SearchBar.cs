using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Grain.SearchBar;
using UnityEngine.Events;

using System.Linq;

public class SearchBar : MonoBehaviour, ISearch
{
    [SerializeField]
    private string id = "Default";

    [SerializeField]
    private string[] ignoreValues;

    [SerializeField]
    private List<GameObject> recordObjects = new List<GameObject>();

    [SerializeField]
    private bool cacheRecords = false;

    [SerializeField]
    private bool getCacheOnStart = true;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onShow = new UnityEvent();

    [SerializeField]
    private UnityEvent onHide = new UnityEvent();

    public List<GameObject> RecordObjects { get { return recordObjects; } }
    public string ID { get { return id; } }
    public List<ISearch> AllInterfaces { get { return interfaces; } }
    public SearchReader Reader { get; set; }
    public string Criteria { get; private set; }
    public string[] IgnoreValues { get { return ignoreValues; } }

    private Coroutine CurrentSearchCoroutine;

    public int RefreshEvery = 100;

    public int ShowMaxSearchItems = 50;

    private string lastSearch = "";

    private List<ISearch> filteredItems = new List<ISearch>();
    

    public RecordData Data
    {
        get
        {
            return null;
        }
    }

    public MonoBehaviour Component
    {
        get
        {
            return this;
        }
    }

    private List<ISearch> interfaces = new List<ISearch>();

    private void Start()
    {
        Criteria = "";

        if(getCacheOnStart) GetCache();


        ResetFiltered();
    }

    private void ResetFiltered()
    {
        interfaces.ForEach(x => x.Hide());

        for (int i = 0; i < ShowMaxSearchItems; i++)
        {
            filteredItems.Add(interfaces[i]);

            filteredItems[i].Show();
        }
    }

    public void OnEdit(string str)
    {
        //Display(str);
        if (CurrentSearchCoroutine != null)
            StopCoroutine(CurrentSearchCoroutine);
        
        if(ShouldDisplay(str))
            CurrentSearchCoroutine = StartCoroutine(DisplayOverTime(str));
        else
            ResetFiltered();

    }

    public void Publish(string rawData)
    {
        if (string.IsNullOrEmpty(rawData)) return;
    }
    
    public IEnumerator DisplayOverTime(string criteria)
    {

        filteredItems.ForEach(x => x.Hide()); 

        int counter = 0;
        
        for (int i = 0; i < filteredItems.Count; i++)
        {
            ISearch searchItem = filteredItems[i];

            if (searchItem.Display(criteria))
            {
                searchItem.Show();
            }
            
            if (counter > RefreshEvery)
            {
                counter = 0;
                yield return new WaitForEndOfFrame();
            }
            
            counter++;
        }

        CurrentSearchCoroutine = null;

        yield return new WaitForEndOfFrame();

    }

    public bool ShouldDisplay(string criteria)
    {
        filteredItems.ForEach(x => x.Hide());
        filteredItems.Clear();

        for (int i = 0; i < interfaces.Count; i++)
        {
            ISearch searchItem = interfaces[i];

            if (searchItem.Display(criteria))
            {
                filteredItems.Add(searchItem);
            }
        }

        return filteredItems.Count < ShowMaxSearchItems;
   }
   

    public bool Display(string criteria)
    {
        Criteria = criteria;
        
        for (int i = 0; i < interfaces.Count; i++)
        {
            if (interfaces[i].Display(criteria))
            {
                interfaces[i].Show();
            }
            else
            {
                interfaces[i].Hide();
            }
        }

        return true;
    }

    public void Show()
    {
        onShow.Invoke();
    }

    public void Hide()
    {
        onHide.Invoke();
    }

    public void CacheRecord()
    {
        SetCache();
    }

    public void UpdateDataValue(string k, string v)
    {
        //not implemented
    }

    public bool GetCache()
    {
        if (!cacheRecords) return false;

        recordObjects.ForEach(r => interfaces.Add(r.GetComponent<ISearch>()));

        string data = PlayerPrefs.GetString(id + "_Records");

        if(string.IsNullOrEmpty(data))
        {
            SetCache();
        }
        else
        {
            AllRecords all = JsonUtility.FromJson<AllRecords>(data);

            for(int i = 0; i< all.records.Count; i++)
            {
                ISearch iSearch = interfaces.FirstOrDefault(s => s.ID.Equals(all.records[i].GetValue("ID")));

                if(iSearch != null)
                {
                    iSearch.Publish(JsonUtility.ToJson(all.records[i]));
                }
                else
                {
                    RecordData r = new RecordData();
                    r.values = all.records[i].values;

                    if(Reader != null)
                    {
                        Reader.AddNew(r);
                    }
                }
            }
        }

        return true;
    }

    private void SetCache()
    {
        if (!cacheRecords) return;

        AllRecords all = new AllRecords();
        all.records = new List<RecordData>();

        foreach(ISearch iS in interfaces)
        {
            all.records.Add(iS.Data);
        }

        PlayerPrefs.SetString(id + "_Records", JsonUtility.ToJson(all));
    }
}
