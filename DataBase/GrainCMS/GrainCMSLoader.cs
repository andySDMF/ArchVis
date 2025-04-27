using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.CMS;

public abstract class GrainCMSLoader : MonoBehaviour
{
    [Header("Base Options")]
    public int loadOrder = 0;               
    public string cmsName = "Default CMS";

    [Header("Base Runtime Variables")]
    public bool loadingCompleted = false;
    public float loadingProgress = 0;  

    public abstract void PerformUpdate();

#if UNITY_EDITOR
    private void Reset()
    {
        GrainCMSHandler cmsManager = GetComponent<GrainCMSHandler>();

        if (cmsManager == null)
        {
            UnityEditor.EditorUtility.DisplayDialog("Error!", "Grain CMS Loaders need to be placed on the same GameObject as the Handler", "Close");
        }
        else
        {
            GrainCMSLoader[] localCmsLoaders = GetComponents<GrainCMSLoader>();

            int highestLoadOrder = int.MinValue;

            for (int i = 0; i < localCmsLoaders.Length; i++)
            {
                if (localCmsLoaders[i].loadOrder > highestLoadOrder) highestLoadOrder = localCmsLoaders[i].loadOrder;
            }

            loadOrder = highestLoadOrder + 1;
        }
    }
#endif

    public void ResetLoadingProgress()
    {
        loadingCompleted = false;
        loadingProgress = 0;
    }

    public void SetLoadingProgress(int currentProgress)
    {
        loadingProgress = currentProgress;

        if (loadingProgress >= 100)
            loadingCompleted = true;
    }

    [System.Serializable]
    protected class CMSTarget
    {
        public GameObject targetObject;
        public string command = "";

        public void Send(string data)
        {
            if (targetObject == null || string.IsNullOrEmpty(command)) return;

            IGrainCache cacheScript = (IGrainCache)targetObject.GetComponent(typeof(IGrainCache));

            if(cacheScript != null)
            {
                cacheScript.RecieveCMSData(data);
            }
            else targetObject.SendMessage(command, data);
        }
    }
}


