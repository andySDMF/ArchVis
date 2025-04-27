using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Grain.CMS;

public class GrainCMSCategoryDownloadProgress : MonoBehaviour
{
    [SerializeField]
    private Text contentText;

    [SerializeField]
    private Slider singleProgressBar;

    [SerializeField]
    private Slider totalProgressBar;

    [SerializeField]
    private UnityEvent onComplete = new UnityEvent();

    private void Awake()
    {
        GrainCMSUtils.CategoryDownloader.onDownloadBegin += OnBeginCallback;
        GrainCMSUtils.CategoryDownloader.onDownloadProgress += OnProgressCallback;
        GrainCMSUtils.CategoryDownloader.onDownloadEnd += OnEndCallback;
    }

    private void OnEnable()
    {
        if(totalProgressBar != null)
        {
            totalProgressBar.wholeNumbers = true;
            totalProgressBar.minValue = 0;
            totalProgressBar.maxValue = GrainCMSUtils.CategoryDownloader.TotalDownload;
        }

        if (singleProgressBar != null)
        {
            singleProgressBar.wholeNumbers = false;
            singleProgressBar.minValue = 0.0f;
            singleProgressBar.maxValue = 1.0f;
        }
    }

    public void InstantiateThis()
    {
        GameObject tCanvas = GameObject.FindGameObjectWithTag("Canvas");

        if (tCanvas == null) return;

        GrainCMSCategoryDownloadProgress tProgress = tCanvas.GetComponentInChildren<GrainCMSCategoryDownloadProgress>(true);

        if (tProgress != null) tProgress.gameObject.SetActive(true);
        else
        {
            GameObject go = Instantiate(this.gameObject, Vector3.zero, Quaternion.identity, tCanvas.transform) as GameObject;

            go.transform.SetParent(tCanvas.transform);
            go.transform.localScale = Vector3.one;
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);
            go.transform.SetAsLastSibling();

            go.SetActive(true);
        }
    }

    public void OnBeginCallback(string name)
    {
        if (contentText != null) contentText.text = name;

        if (singleProgressBar != null) singleProgressBar.value = 0.0f;
    }

    public void OnProgressCallback(float val)
    {
        if(singleProgressBar != null) singleProgressBar.value = val;
    }

    public void OnEndCallback()
    {
        if (singleProgressBar != null) singleProgressBar.value = 1.0f;

        if (totalProgressBar != null)
        {
            totalProgressBar.value++;

            if(totalProgressBar.value >= totalProgressBar.maxValue)
            {
                onComplete.Invoke();
            }
        }
    }
}
