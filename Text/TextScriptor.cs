using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextScriptor : MonoBehaviour
{
    [Header("Text Scripter")]
    [SerializeField]
    private string phrase = "";

    [SerializeField]
    private MaskableGraphic textScript;

    [SerializeField]
    private float interval = 0.1f;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float delay = 0.0f;

    private float delayTime = 0.0f;
    private float timeBetweenText = 0.0f;

    private int charCount = 0;
    private bool isTweening = false;
    private bool state = false;

    public string Phrase { get { return phrase; } set { phrase = value; } }

    private void Update()
    {
        if (isTweening)
        {
            if (delayTime < delay)
            {
                delayTime += Time.deltaTime;
            }
            else
            {
                if (charCount < phrase.Length)
                {
                    if (timeBetweenText < interval)
                    {
                        timeBetweenText += Time.deltaTime;
                    }
                    else
                    {
                        if (textScript is TextMeshProUGUI)
                        {
                            ((TextMeshProUGUI)textScript).text += phrase[charCount].ToString();
                        }
                        else
                        {
                            ((Text)textScript).text += phrase[charCount].ToString();
                        } 

                        charCount++;
                        timeBetweenText = 0.0f;
                    }
                }
                else
                {
                    isTweening = false;
                }
            }
        }
    }

    public void Begin()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if(canvasGroup != null) canvasGroup.alpha = 0.0f;

        state = false;

        delayTime = 0.0f;
        timeBetweenText = 0.0f;
        charCount = 0;

        if(textScript is TextMeshProUGUI)
        {
            ((TextMeshProUGUI)textScript).text = "";
        }
        else
        {
            ((Text)textScript).text = "";
        }

        if (canvasGroup != null) canvasGroup.alpha = 1.0f;

        isTweening = true;
    }

    public void End()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup != null) canvasGroup.alpha = 1.0f;

        isTweening = false;

        if (textScript is TextMeshProUGUI)
        {
            ((TextMeshProUGUI)textScript).text = phrase;
        }
        else
        {
            ((Text)textScript).text = phrase;
        }

        state = true;
    }

    public void ResetThis()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        isTweening = false;

        if (canvasGroup != null) canvasGroup.alpha = 0.0f;
    }

    public void Amend(string str)
    {
        ResetThis();

        phrase = str;

        Begin();
    }
}
