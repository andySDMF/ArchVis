using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Linq;
using Grain.CMS;

public class GrainCMSCategoryDownloader : MonoBehaviour
{
    public enum ApplicationPath { _PersistantDataPath, _DataPath }

    [SerializeField]
    private ApplicationPath applicationPath = ApplicationPath._DataPath;

    [SerializeField]
    private bool debug = false;

    [SerializeField]
    private string apiToken = "";

    [Header("Download Events")]
    [SerializeField]
    private UnityEvent onBegin;

    [SerializeField]
    private UnityEvent onEnd;

    public string initialPath = "";

    public bool DownloadInProgress { get; set; }
    public bool IsPaused { get; set; }

    public int TotalDownload { get; set; }

    private bool hasStarted = false;

    public System.Action<float> onDownloadProgress;
    public System.Action<string> onDownloadBegin;
    public System.Action onDownloadEnd;

    private void Awake()
    {
        initialPath = "";
        GrainCMSUtils.CategoryDownloader = this;
    }

    private void Start()
    {
        Set();
    }

    public void Begin()
    {
        if (DownloadInProgress) return;

        if (!Application.isPlaying) Set();

        DownloadInProgress = true;
        IsPaused = false;

        StartCoroutine(Perform());
    }

    public void End()
    {
        DownloadInProgress = false;
        IsPaused = false;

        onEnd.Invoke();
    }

    public void Pause(bool state)
    {
        IsPaused = state;
    }

    private void Set()
    {

#if UNITY_IOS
        applicationPath = ApplicationPath._PersistantDataPath;
#endif


        switch (applicationPath)
        {
            case ApplicationPath._PersistantDataPath:

                initialPath = Application.persistentDataPath + "/";

                break;

            default:

                string[] temp = Application.dataPath.Split('/');

                for (int i = 0; i < temp.Length - 1; i++)
                {
                    initialPath += temp[i] + "/";
                }
                break;
        }

        if (!GrainCMSUtils.FileExists(initialPath + "CMS"))
        {
            GrainCMSUtils.CreateFile(initialPath + "CMS");
        }

        initialPath += "CMS/";
    }

    private IEnumerator Perform()
    {
        List<Category> categories = GrainCMSUtils.GetCategories();

        TotalDownload = GrainCMSUtils.GetTotalCollectionCount(false);

        List<string> existingCategories= GrainCMSUtils.GetDirectorirs(initialPath);

        foreach (string file in existingCategories)
        {
            string[] splitDirectory = file.Split('/');

            Category existingCategory = GrainCMSUtils.GetCategory(splitDirectory[splitDirectory.Length - 1]);

            if (existingCategory == null)
            {
                if (debug) Debug.Log("Deleting category = " + splitDirectory[splitDirectory.Length - 1]);

                if (!hasStarted)
                {
                    hasStarted = true;
                    onBegin.Invoke();
                }

                GrainCMSUtils.DeleteFile(file);
            }
        }

        foreach (Category category in categories)
        {
            if (!GrainCMSUtils.FileExists(initialPath + category.name))
            {
                GrainCMSUtils.CreateFile(initialPath + "/" + category.name);
            }

            List<string> existingCollections = GrainCMSUtils.GetDirectorirs(initialPath + "/" + category.name);

            foreach (string file in existingCollections)
            {
                string[] splitCollection = file.Split('/');

                CategoryCollection existingCollection = GrainCMSUtils.GetCategoryCollection(category.name, splitCollection[splitCollection.Length - 1]);

                if (existingCollection == null)
                {
                    if (debug) Debug.Log("Deleting collection = " + splitCollection[splitCollection.Length - 1]);

                    if (!hasStarted)
                    {
                        hasStarted = true;
                        onBegin.Invoke();
                    }

                    GrainCMSUtils.DeleteFile(file);
                }
            }

            foreach (CategoryCollection collection in category.collections)
            {
                if (!GrainCMSUtils.FileExists(initialPath + category.name + "/" + collection.name))
                {
                    GrainCMSUtils.CreateFile(initialPath + category.name + "/" + collection.name);
                }

                List<string> existingFiles = GrainCMSUtils.GetDirectorirs(initialPath + category.name + "/" + collection.name);

                foreach(string file in existingFiles)
                {
                    string temp = file.Replace(@"\", "/"); 

                    string[] splitContent = temp.Split('/');

                    CategoryCollectionContent content = collection.contents.FirstOrDefault(c => c.identifier.Equals(splitContent[splitContent.Length - 1]));

                    if(content == null)
                    {
                        if (debug) Debug.Log("Deleting file content = " + splitContent[splitContent.Length - 1]);

                        if (!hasStarted)
                        {
                            hasStarted = true;
                            onBegin.Invoke();
                        }

                        GrainCMSUtils.DeleteFile(file);
                    }
                }

                foreach (CategoryCollectionContent content in collection.contents)
                {
                    content.internalURL = "file://" + initialPath + category.name + "/" + collection.name + "/" + content.identifier;

                    if (GrainCMSUtils.FileExists(initialPath + category.name + "/" + collection.name + "/" + content.identifier))
                    {
                        if (content.updated < 0)
                        {
                            if (onDownloadEnd != null) onDownloadEnd.Invoke();

                            continue;
                        }
                        else
                        {
                            if (!hasStarted)
                            {
                                hasStarted = true;
                                onBegin.Invoke();
                            }

                            GrainCMSUtils.DeleteFile(initialPath + category.name + "/" + collection.name + "/" + content.identifier);
                        }
                    }

                    using (UnityWebRequest request = UnityWebRequest.Get(content.url + "?token=" + apiToken))
                    {
                        if (!hasStarted)
                        {
                            hasStarted = true;
                            onBegin.Invoke();
                        }

                        request.SendWebRequest();

                        if(onDownloadBegin != null) onDownloadBegin.Invoke(content.identifier);

                        while(!request.isDone)
                        {
                            if (onDownloadProgress != null) onDownloadProgress.Invoke(request.downloadProgress);

                            yield return null;
                        }

                        if(request.isNetworkError || request.isHttpError)
                        {
                            if(debug) Debug.Log("CMS request [" + content.identifier +"], error " + request.responseCode.ToString() + "[" + request.error + "]");
                        }
                        else
                        {
                            GrainCMSUtils.SaveFile(initialPath + category.name + "/" + collection.name, initialPath + category.name + "/" + collection.name + "/" + content.identifier, request.downloadHandler.data);

                            if (onDownloadEnd != null) onDownloadEnd.Invoke();
                        }

                        request.Dispose();
                    }

                    content.updated = -1;
                }
            }
        }

        GrainCMSUtils.SaveCategories();

        End();

        yield return null;
    }
}
