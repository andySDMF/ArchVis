using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Xml.Linq;

public class MapMarkerSource : MonoBehaviour
{
    [SerializeField]
    private TextAsset asset;

    [SerializeField]
    private bool autoBegin = false;

    [SerializeField]
    private UnityEvent onRecievedComplete = new UnityEvent();

    private bool invalid = false;

    private void Awake()
    {
        if (!GetComponentInParent<Map>())
        {
            invalid = true;
            this.enabled = false;
        }
    }

    private void Start()
    {
        if (invalid) return;

        if (autoBegin) Begin();
    }

    public void Begin()
    {
        if (invalid) return;

        if (asset != null)
        {
            Recieve(asset.text);
        }
    }

    public void Recieve(string rawData)
    {
        if (invalid) return;

        if (string.IsNullOrEmpty(rawData)) return;

        var doc = XDocument.Parse(rawData);

        foreach (XElement e in doc.Elements())
        {
            foreach(XElement inner in e.Nodes())
            {
                GameObject go = new GameObject();
                go.transform.SetParent(this.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;

                MapMarker m = go.AddComponent<MapMarker>();

                string name = "";
                string lat = "";
                string lon = "";
                string origin = "";
                bool useV = true;
                string v = "";
                string img = "";

                foreach(XElement node in inner.Nodes())
                {
                    if (node.Name.ToString().Equals("name"))
                    {
                        go.name = node.Value;
                        name = node.Value;
                    }
                    else if (node.Name.ToString().Equals("lat"))
                    {
                        lat = node.Value;
                    }
                    else if (node.Name.ToString().Equals("lon"))
                    {
                        lon = node.Value;
                    }
                    else if (node.Name.ToString().Equals("origin"))
                    {
                        origin = node.Value;
                    }
                    else if (node.Name.ToString().Equals("visualiserOrigin"))
                    {
                        useV = (node.Value.Equals("1")) ? true : false;
                    }
                    else if (node.Name.ToString().Equals("visualiserName"))
                    {
                        v = node.Value;
                    }
                    else if (node.Name.ToString().Equals("baseImage"))
                    {
                        img = node.Value;
                    }
                }

                m.Create(name, lat, lon, origin, v, useV, img);
            }
        }

        onRecievedComplete.Invoke();
    }
}
