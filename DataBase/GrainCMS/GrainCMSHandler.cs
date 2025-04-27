using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Grain.CMS;

[System.Serializable]
public class PreloadProgressUpdatedEvent : UnityEvent<float> { }

public class GrainCMSHandler : MonoBehaviour
{
    [Header("Start Up")]
    [SerializeField]
    private bool autoStart = true;

    [Header("Auto Update")]
    public bool timedUpdate = true;
    public float secondsBetweenUpdate = 60f;

    [Header("Events")]
    public UnityEvent OnLoadingStarted;
    public PreloadProgressUpdatedEvent OnLoadingProgressUpdated;
    public UnityEvent OnLoadingCompleted;

    private List<GrainCMSLoader> cmsLoaders = new List<GrainCMSLoader>();

    private bool hasStarted;

    private void Awake()
    {
        GrainCMSLoader[] cmsComponents = GetComponents<GrainCMSLoader>();

        cmsLoaders.Clear();

        for (int i = 0; i < cmsComponents.Length; i++) cmsLoaders.Add(cmsComponents[i]);

        cmsLoaders.Sort(((x, y) => x.loadOrder.CompareTo(y.loadOrder)));

        for (int i = 0; i < cmsLoaders.Count; i++)
        {
            if (i == 0) continue;

            if (cmsLoaders[i].loadOrder == cmsLoaders[i - 1].loadOrder) Debug.LogError("Grain CMS Loader " + cmsLoaders[i].cmsName + " has a duplicate load order!");
        }
    }

    private void Start()
    {
        if(autoStart)
        {
            Begin();
        }
    }

    public void Begin()
    {
        if (hasStarted) return;

        hasStarted = true;

        if (cmsLoaders.Count == 0)
        {
            Debug.LogError("No Grain CMS Loaders found! No data will be loaded into cache!");
        }
        else
        {
            if (timedUpdate)
            {
                StartCoroutine(DoTimedUpdate());
            }
            else
            {
                StartCoroutine(DoUpdateCmsLoaders());
            }
        }
    }

    private IEnumerator DoTimedUpdate()
    {
        while (true)
        {
            yield return StartCoroutine(DoUpdateCmsLoaders());

            yield return new WaitForSeconds(secondsBetweenUpdate);
        }
    }

    private IEnumerator DoUpdateCmsLoaders()
    {
        float lastProgressValue = 0;
        float newProgressValue = 0;

        OnLoadingStarted.Invoke();
        OnLoadingProgressUpdated.Invoke(0);

        for (int i = 0; i < cmsLoaders.Count; i++)
        {
            cmsLoaders[i].ResetLoadingProgress();
            cmsLoaders[i].PerformUpdate();

            while (!cmsLoaders[i].loadingCompleted)
            {
                newProgressValue = ((((float)i * 100f) + cmsLoaders[i].loadingProgress) / (float)cmsLoaders.Count);

                if (lastProgressValue != newProgressValue)
                {
                    if (newProgressValue > lastProgressValue)
                    {
                        OnLoadingProgressUpdated.Invoke(newProgressValue);
                        lastProgressValue = newProgressValue;
                    }
                }

                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        OnLoadingCompleted.Invoke();

        yield break;
    }
}
