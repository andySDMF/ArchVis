using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MenuSystem : MonoBehaviour
{
    [SerializeField]
    private List<MenuSection> sections = new List<MenuSection>();

    [SerializeField]
    private UnityEvent onLoadComplete = new UnityEvent();

    [SerializeField]
    private GameObject baseOverlay;

    private bool hasInit = false;
    private string cached = "";

    public string Current
    {
        get
        {
            MenuSection mSection = sections.FirstOrDefault(s => s.IsLoaded);

            return (mSection != null) ? mSection.name : "";
        }
    }

    public void Init()
    {
        if (!hasInit)
        {
            hasInit = true;

            sections.ForEach(s => s.Init());

            ResetThis();

            onLoadComplete.Invoke();
        }
    }

    public void Show(string name)
    {
        if (!string.IsNullOrEmpty(Current))
        {
            if (Current.Equals(name)) return;
            else
            {
                Hide(Current);
            }
        }

        MenuSection mSection = sections.FirstOrDefault(s => s.name.Equals(name));

        if (mSection != null)
        {
            if (!mSection.IsLoaded)
            {
                mSection.onShow.Invoke();
                mSection.IsLoaded = true;

                cached = name;
            }

            if (baseOverlay != null) baseOverlay.SetActive(false);
        }
        else
        {
            if (baseOverlay != null) baseOverlay.SetActive(true);
        }
    }

    public void ShowPrevious()
    {
        Show(cached);
    }

    public void Hide(string name)
    {
        MenuSection mSection = sections.FirstOrDefault(s => s.name.Equals(name));

        if (mSection != null)
        {
            if (mSection.IsLoaded)
            {
                mSection.onHide.Invoke();
                mSection.IsLoaded = false;
            }
        }
    }

    public void SetNow(string name)
    {
        MenuSection mSection = sections.FirstOrDefault(s => s.name.Equals(name));

        if (mSection != null)
        {
            cached = name;
        }
    }

    public void ResetThis()
    {
        if (!string.IsNullOrEmpty(Current))
        {
            cached = Current;
        }

        foreach (MenuSection mSection in sections)
        {
            mSection.onReset.Invoke();
            mSection.IsLoaded = false;
        }
    }

    [System.Serializable]
    private class MenuSection
    {
        public string name = "";
        public UnityEvent onShow = new UnityEvent();
        public UnityEvent onHide = new UnityEvent();
        public UnityEvent onReset = new UnityEvent();

        public bool IsLoaded { get; set; }

        public void Init()
        {
            IsLoaded = false;
        }
    }
}
