using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.CMS;

public class GrainCMSCategoryNotification : MonoBehaviour
{
    [SerializeField]
    private bool closeOnSelect = true;

    public virtual void UpdateCategories()
    {
        GrainCMSUtils.CategoriesCacheHandler.NotificationStatus = NotificationState._Confirmed;

        Close();
    }

    public virtual void Skip()
    {
        GrainCMSUtils.CategoriesCacheHandler.NotificationStatus = NotificationState._Skipped;

        Close();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
