using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Grain.CMS;

public class GrainCMSCategoriesCache : MonoBehaviour, IGrainCache
{
    [SerializeField]
    private bool debug = false;

    [SerializeField]
    private UpdateType updateType = UpdateType._Always;

    [SerializeField]
    private Transform notificationPrefab;

    [SerializeField]
    private UnityEvent onComplete = new UnityEvent();

    [SerializeField]
    private List<Category> categories = new List<Category>();

    [SerializeField]
    private DatabaseCache cache;

    public List<Category> Categories { get { return categories; } }

    private string cachedRawData = "";

    public NotificationState NotificationStatus = NotificationState._Waiting;

    private void Start()
    {
        GrainCMSUtils.CategoriesCacheHandler = this;
    }

    public void RecieveCMSData(string rawData)
    {
        Categories json = JsonUtility.FromJson<Categories>(rawData);

        if (json == null) return;

        bool update = true;

        if(json.update < 0)
        {
            if (Application.isPlaying) update = false;

            cachedRawData = JsonUtility.ToJson(json);
        }
        else
        {
            if (Application.isPlaying)
            {
                if (!string.IsNullOrEmpty(rawData))
                {
                    if (PlayerPrefs.GetString("CATEGORIES").Equals(JsonUtility.ToJson(json)))
                    {
                        update = false;
                        cachedRawData = PlayerPrefs.GetString("CATEGORIES");
                    }
                    else
                    {
                        cache.rawCategoriesData = rawData;

                        if (updateType.Equals(UpdateType._Notification))
                        {
                            if(notificationPrefab != null)
                            {
                                GameObject tCanvas = GameObject.FindGameObjectWithTag("Canvas");

                                if (tCanvas == null) return;

                                GrainCMSCategoryNotification tNotifitcation = tCanvas.GetComponentInChildren<GrainCMSCategoryNotification>(true);

                                if (tNotifitcation != null) tNotifitcation.gameObject.SetActive(true);
                                else
                                {
                                    GameObject go = Instantiate(notificationPrefab.gameObject, Vector3.zero, Quaternion.identity, tCanvas.transform) as GameObject;

                                    go.transform.SetParent(tCanvas.transform);
                                    go.transform.localScale = Vector3.one;
                                    go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);
                                    go.transform.SetAsLastSibling();

                                    go.SetActive(true);
                                }
           
                                NotificationStatus = NotificationState._Waiting;
                            }

                            StartCoroutine(NotificationHandle(JsonUtility.ToJson(json)));

                            return;
                        }

                        cachedRawData = JsonUtility.ToJson(json);
                        PlayerPrefs.SetString("CATEGORIES", cachedRawData);
                    }
                }
                else
                {
                    cachedRawData = PlayerPrefs.GetString("CATEGORIES");

                    if (string.IsNullOrEmpty(cachedRawData))
                    {
                        PlayerPrefs.SetString("CATEGORIES", cache.rawCategoriesData);
                        cachedRawData = cache.rawCategoriesData;
                    }
                }
            }
            else
            {
                cachedRawData = JsonUtility.ToJson(json);
                cache.rawCategoriesData = rawData;
            }
        }

        if (updateType.Equals(UpdateType._Always) || update)
        {
            categories.Clear();
            categories.AddRange(json.categories);

            if(!string.IsNullOrEmpty(json.timestamp)) GrainCMSUtils.TimeStamp = json.timestamp;
            PlayerPrefs.SetString("TIMESTAMP", GrainCMSUtils.TimeStamp);
        }

        onComplete.Invoke();
    }

    private IEnumerator NotificationHandle(string rawData)
    {
        while(NotificationStatus.Equals(NotificationState._Waiting))
        {
            yield return null;
        }

        bool update = true;

        if(NotificationStatus.Equals(NotificationState._Confirmed))
        {
            cachedRawData = rawData;
            PlayerPrefs.SetString("CATEGORIES", cachedRawData);
        }
        else
        {
            cachedRawData = PlayerPrefs.GetString("CATEGORIES");
            update = false;
        }

        if(update)
        {
            Categories json = JsonUtility.FromJson<Categories>(cachedRawData);

            categories.Clear();
            categories.AddRange(json.categories);

            if (!string.IsNullOrEmpty(json.timestamp)) GrainCMSUtils.TimeStamp = json.timestamp;
            PlayerPrefs.SetString("TIMESTAMP", GrainCMSUtils.TimeStamp);
        }

        onComplete.Invoke();
    }

    public void Save()
    {
        Categories json = new Categories();
        json.categories = new List<Category>();
        json.categories.AddRange(categories);

        cachedRawData = JsonUtility.ToJson(json);
        PlayerPrefs.SetString("CATEGORIES", cachedRawData);
    }
}

[System.Serializable]
public class Categories
{
    public string timestamp;
    public int update;
    public List<Category> categories;
}

[System.Serializable]
public class Category
{
    public string name;
    public List<CategoryCollection> collections;
}

[System.Serializable]
public class CategoryCollection
{
    public string name;
    public string content_type;
    public List<CategoryCollectionContent> contents;
}

[System.Serializable]
public class CategoryCollectionContent
{
    public string identifier;
    public string reference;
    public string url;
    public string created;
    public int updated;
    public CategoryCollectionContentData data;

    public string internalURL;
}

[System.Serializable]
public class CategoryCollectionContentData
{
    public List<string> plots;
}


public enum UpdateType { _Always, _Notification }
public enum NotificationState { _Confirmed, _Skipped, _Waiting }
