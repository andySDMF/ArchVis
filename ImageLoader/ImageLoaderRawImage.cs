using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.ImageLoader;

[RequireComponent(typeof(RawImage))]
public class ImageLoaderRawImage : MonoBehaviour, IImageLoader
{
    [SerializeField]
    private string data = "";

    [SerializeField]
    private bool fromMemory = false;

    [SerializeField]
    private RequestType requestType = RequestType._WebRequest;

    [SerializeField]
    private bool enableCulling = true;

    [Header("Tools")]
    [SerializeField]
    private Vector2 deltaSize = new Vector2(2048, 2048);
    [SerializeField]
    private bool setSizeNow = false;
    [SerializeField]
    private bool preview = false;

    [SerializeField]
    private bool cancelTools = false;

    private RawImage imageScript;
    private bool isShowing = false;
    private RectTransform rectT;

    private bool isPreviewed = false;

    public bool FromMemory { get { return fromMemory; } }
    public string Data { get { return data; } }

    private void Awake()
    {
        if (imageScript == null) imageScript = GetComponent<RawImage>();

        rectT = GetComponent<RectTransform>();

        imageScript.CrossFadeAlpha(0.0f, 0.0f, true);
    }

    private void OnEnable()
    {
        imageScript.CrossFadeAlpha(0.0f, 0.0f, true);

        if (!enableCulling) Load();
    }

    private void OnDisable()
    {
        Unload();
    }

    private void Update()
    {
        if(imageScript != null)
        {
            if(enableCulling)
            {
                if (ImageLoaderUtils.IsCulled(rectT))
                {
                    Load();
                }
                else
                {
                    Unload();
                }
            }
        }
    }

    public void Append(string data)
    {
        Awake();

        if (string.IsNullOrEmpty(data)) return;

        this.data = data;
    }

    public void Load()
    {
        if (!enableCulling)
        {
            Unload();
        }
        else
        {
            if (isShowing) return;

            Unload();
        }

        imageScript.CrossFadeAlpha(0.0f, 0.0f, true);

        if (imageScript != null)
        {
            if (!fromMemory)
            {
                imageScript.texture = Resources.Load(data) as Texture2D;

                imageScript.CrossFadeAlpha(1.0f, 0.5f, true);
            }
            else
            {
                if (requestType == RequestType._WebRequest)
                {
                    StartCoroutine(ImageLoaderUtils.WebRequest(data, imageScript));
                }
                else if (requestType == RequestType._WWW)
                {
                    StartCoroutine(ImageLoaderUtils.WWWRequest(data, imageScript));
                }
                else
                {
                    imageScript.texture = ImageLoaderUtils.LoadFile(data);
                    imageScript.CrossFadeAlpha(1.0f, 0.5f, true);
                }
            }
        }

        isShowing = true;
    }

    public void Unload()
    {
        Awake();

        if(imageScript != null)
        {
            imageScript.CrossFadeAlpha(0.0f, 0.5f, true);

            if (fromMemory)
                Destroy(imageScript.texture);
            else
                imageScript.texture = null;

            Resources.UnloadUnusedAssets();
        }

        isShowing = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        if (cancelTools) return;

        if (setSizeNow)
        {
            GetComponent<RawImage>().rectTransform.sizeDelta = deltaSize;

            setSizeNow = false;
        }

        if(preview)
        {
            if (isPreviewed) return;

            isPreviewed = true;

            Texture2D tex;

            if (fromMemory) tex = ImageLoaderUtils.LoadFile(data);
            else tex = Resources.Load(data) as Texture2D;

            if (tex != null)
            {
                GetComponent<RawImage>().texture = tex;
            }
        }
        else
        {
            if(GetComponent<RawImage>().texture != null)
            {
                if (fromMemory) DestroyImmediate(GetComponent<RawImage>().texture);
       
                GetComponent<RawImage>().texture = null;
            }

            isPreviewed = false;
        }
    }
#endif
}
