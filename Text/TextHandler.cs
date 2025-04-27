using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Grain.Text;
using System.IO;
using System.Linq;

public class TextHandler : MonoBehaviour, IText
{
    [SerializeField]
    private string id = "";

    [Header("Load Handler")]
    [SerializeField]
    private string source;

    [SerializeField]
    private bool fromMemory = false;

    [SerializeField]
    private bool loadOnAwake = false;

    [Header("Sources")]
    [SerializeField]
    private List<TextSource> dictionary = new List<TextSource>();

    private void Awake()
    {
        if (loadOnAwake) Load();
    }

    public void Append(string rawData)
    {
        if (string.IsNullOrEmpty(rawData)) return;

        SGroup sGroup = JsonUtility.FromJson<SGroup>(rawData);

        if (sGroup != null)
        {
            if(id.Equals(sGroup.id))
            {
                string str = "";

                foreach (SEntry entry in sGroup.entries)
                {
                    foreach (SEntryElement element in entry.elements)
                    {
                        if (element.id.Equals("url"))
                        {
                            str += entry.reference + "^" + FileLoad(element.data) + "¬";
                        }
                    }
                }

                Create(str, true);

                TextObject[] tObjects = GetComponentsInChildren<TextObject>(true);

                for(int i = 0; i < tObjects.Length; i++)
                {
                    tObjects[i].Append(Get(tObjects[i].SourceRef, tObjects[i].TagRef));
                }
            }
        }
    }

    public void Load()
    {
        if(fromMemory)
        {
            Load(source);
        }
        else
        {
            TextAsset asset = Resources.Load(source) as TextAsset;
            Load(asset);
        }
    }

    public void Load(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        Create(FileLoad(source));
    }

    public void Load(TextAsset asset)
    {
        if (asset == null) return;

        Create(asset.text);
    }

    public string Get(string source, string tagID)
    {
        TextSource tSource = dictionary.FirstOrDefault(s => s.id.Equals(source));

        if(tSource != null)
        {
            return tSource.Get(tagID);
        }

        return "";
    }

    private string FileLoad(string data)
    {
        string str = "";

        if (data.Contains("file://"))
        {
            data = data.Replace("file://", "");
        }

        if (File.Exists(data))
        {
            str = File.ReadAllText(data);
        }

        return str;
    }

    private void Create(string data, bool multiple = false)
    {
        if (string.IsNullOrEmpty(data)) return;

        dictionary.Clear();

        if(multiple)
        {
            string[] splitSources = data.Split('¬');

            for(int i = 0; i < splitSources.Length; i++)
            {
                if (string.IsNullOrEmpty(splitSources[i])) continue;

                string[] source = splitSources[i].Split('^');

                TextSource tSource = new TextSource(source[0], source[1]);
                dictionary.Add(tSource);
            }
        }
        else
        {
            TextSource tSource = new TextSource(id, data);
            dictionary.Add(tSource);
        }
    }

    [System.Serializable]
    private class SGroup
    {
        public string id;
        public List<SEntry> entries;
    }

    [System.Serializable]
    private class SEntry
    {
        public string id;
        public string reference;
        public List<SEntryElement> elements;
    }

    [System.Serializable]
    private class SEntryElement
    {
        public string id;
        public string data;
    }
}
