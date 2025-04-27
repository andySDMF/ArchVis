using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.SearchBar;

public class SearchReader : MonoBehaviour
{
    [SerializeField]
    protected TextAsset asset;

    [SerializeField]
    protected bool excludeHeader = false;

    [SerializeField]
    private ReadFileType fileType = ReadFileType._TXT;

    [SerializeField]
    protected SearchBar associatedSearchBar;

    [SerializeField]
    protected GameObject prefab;

    [SerializeField]
    protected Transform container;

    private void Awake()
    {
        if (associatedSearchBar != null) associatedSearchBar.Reader = this;
    }

    public void Publish()
    {
        if (asset == null) return;

        Clear();

        if (fileType.Equals(ReadFileType._TXT))
        {
            PublishTXTFile();
        }
        else
        {
            PublishCSVFile();
        }
    }

    public virtual void Clear()
    {
        if (associatedSearchBar != null)
        {
            foreach (GameObject go in associatedSearchBar.RecordObjects)
            {
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }

            associatedSearchBar.RecordObjects.Clear();
        }
    }

    protected virtual void PublishTXTFile()
    {
        //this will be customised per thing......

        Debug.Log(asset.text);
    }

    protected virtual void PublishCSVFile()
    {
        char lineSeperater = '\n'; // It defines line seperate character

        string[] records = asset.text.Split(lineSeperater);

        Debug.Log(records.Length);

        foreach (string record in records)
        {
            Debug.Log(record);
        }
    }

    public virtual void AddNew(RecordData r)
    {

    }

    public virtual void RemoveExisting(GameObject go)
    {

    }
}
