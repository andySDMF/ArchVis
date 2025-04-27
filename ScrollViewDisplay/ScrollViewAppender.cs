using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ScrollViewAppender : MonoBehaviour
{
    [SerializeField]
    private ScrollViewDisplay scrollDisplay;

    [SerializeField]
    private UnityEvent onAppend = new UnityEvent();

    private List<string> categoryList = new List<string>();

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Set(ScrollViewDisplay control)
    {
        if (control == null) return;

        scrollDisplay = control;
    }

    public void Append(string catageryID)
    {
        if (scrollDisplay == null) return;

        categoryList.Clear();

        if(catageryID.Equals("All"))
        {
            List<Transform> transferItems = new List<Transform>();

            for (int i = 0; i < transform.childCount; i++)
            {
                transferItems.Add(transform.GetChild(i));
            }

            transferItems.ForEach(x => x.SetParent(scrollDisplay.transform));

            scrollDisplay.Append();

            onAppend.Invoke();
        }
        else
        {
            List<Transform> removeItems = new List<Transform>();
            List<Transform> transferItems = new List<Transform>();

            for (int i = 0; i < scrollDisplay.transform.childCount; i++)
            {
                ScrollViewItem svItem = scrollDisplay.transform.GetChild(i).GetComponent<ScrollViewItem>();

                if(svItem != null)
                {
                    if(!svItem.Categories.Contains<string>(catageryID))
                    {
                        removeItems.Add(scrollDisplay.transform.GetChild(i));
                    }
                }
            }

            for(int i = 0; i < transform.childCount; i++)
            {
                ScrollViewItem svItem = transform.GetChild(i).GetComponentsInChildren<ScrollViewItem>(true)[0];

                if (svItem != null)
                {
                    if (svItem.Categories.Contains<string>(catageryID))
                    {
                        transferItems.Add(transform.GetChild(i));
                    }
                }
            }

            removeItems.ForEach(x => x.SetParent(transform));
            transferItems.ForEach(x => x.SetParent(scrollDisplay.transform));

            scrollDisplay.Append();

            onAppend.Invoke();
        }
    }

    public void ResetThis()
    {
        Append("All");
    }

    public void AddToAppendList(string categoryID)
    {
        if(!categoryList.Contains(categoryID))
        {
            categoryList.Add(categoryID);
        }

        AppendList();
    }

    public void AddToAppendListNoAction(string categoryID)
    {
        if (!categoryList.Contains(categoryID))
        {
            categoryList.Add(categoryID);
        }
    }

    public void RemoveFromAppendList(string categoryID)
    {
        if (categoryList.Contains(categoryID))
        {
            categoryList.Remove(categoryID);
        }

        AppendList();
    }

    public void RemoveFromAppendListNoAction(string categoryID)
    {
        if(categoryList.Contains(categoryID))
        {
            categoryList.Remove(categoryID);
        }
    }

    private void AppendList()
    {
        List<Transform> removeItems = new List<Transform>();
        List<Transform> transferItems = new List<Transform>();

        for (int i = 0; i < scrollDisplay.transform.childCount; i++)
        {
            ScrollViewItem svItem = scrollDisplay.transform.GetChild(i).GetComponent<ScrollViewItem>();

            if (svItem != null)
            {
                int count = 0;

                for (int j = 0; j < categoryList.Count; j++)
                {
                    if (svItem.Categories.Contains<string>(categoryList[j]))
                    {
                        count++;
                    }
                }

                if(count <= 0)
                {
                    removeItems.Add(scrollDisplay.transform.GetChild(i));
                }
            }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            ScrollViewItem svItem = transform.GetChild(i).GetComponentsInChildren<ScrollViewItem>(true)[0];

            if (svItem != null)
            {
                int count = 0;

                for (int j = 0; j < categoryList.Count; j++)
                {
                    if (svItem.Categories.Contains<string>(categoryList[j]))
                    {
                        count++;
                    }
                }

                if (count > 0)
                {
                    transferItems.Add(transform.GetChild(i));
                }
            }
        }

        removeItems.ForEach(x => x.SetParent(transform));
        transferItems.ForEach(x => x.SetParent(scrollDisplay.transform));

        scrollDisplay.Append();

        onAppend.Invoke();
    }
}
