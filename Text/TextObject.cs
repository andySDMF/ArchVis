using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.Text;

public class TextObject : MonoBehaviour, IText
{
    [SerializeField]
    private string source;

    [SerializeField]
    private string tagID = "p1";

    [SerializeField]
    private Text textScript;

    [SerializeField]
    private TextHandler handler;

    [Header("Enable/Disable Events")]
    [SerializeField]
    private bool eventsOn = true;

    public string SourceRef { get { return source; } }
    public string TagRef {  get { return tagID; } }

    private void OnEnable()
    {
        if(eventsOn)
        {
            if(handler != null)
            {
                Append(handler.Get(source, tagID));
            }
        }
    }

    private void OnDisable()
    {
        if(eventsOn)
        {
            if (textScript != null) textScript.text = null;
        }
    }

    public void Append(string rawData)
    {
        if (string.IsNullOrEmpty(rawData)) return;

        if(textScript != null)
        {
            textScript.text = rawData;
        }
    }
}
