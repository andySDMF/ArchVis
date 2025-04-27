using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grain.ImageLoader;

[RequireComponent(typeof(Image))]
public class ImageLoaderSprite : MonoBehaviour, IImageLoader
{
    [SerializeField]
    private string data = "";

    [SerializeField]
    private bool withinMultipleSprite = false;

    [SerializeField]
    private int multipleSpriteIndex = 0;

    [SerializeField]
    private bool enableCulling = true;

    [Header("Tools")]
    [SerializeField]
    private bool setNativeSizeNow = false;
    [SerializeField]
    private bool preview = false;

    private Image imageScript;
    private bool isShowing = false;
    private RectTransform rectT;

    private bool isPreviewed = false;

    private void Awake()
    {
        if (imageScript == null)
        {
            imageScript = GetComponent<Image>();
            imageScript.CrossFadeAlpha(0.0f, 0.0f, true);
        }

        rectT = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (!enableCulling) Load();
    }

    private void OnDisable()
    {
        Unload();
    }

    private void Update()
    {
        if (imageScript != null)
        {
            if (enableCulling)
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
        if (isShowing) return;

        Awake();

        imageScript.CrossFadeAlpha(0.0f, 0.0f, true);

        if (imageScript != null)
        {
            Sprite sprite = null;

            if (withinMultipleSprite)
            {
                Object[] assets = Resources.LoadAll<Sprite>(data);

                if(multipleSpriteIndex < assets.Length) sprite = (Sprite)assets[multipleSpriteIndex];
            }
            else sprite = Resources.Load<Sprite>(data);

            if (sprite != null)
            {
                imageScript.enabled = false;
                imageScript.sprite = sprite;
                imageScript.enabled = true;
            }
        }

        isShowing = true;

        imageScript.CrossFadeAlpha(1.0f, 0.0f, true);
    }

    public void Unload()
    {
        Awake();

        if (imageScript != null)
        {
            imageScript.CrossFadeAlpha(0.0f, 0.5f, true);
            imageScript.sprite = null;
        }

        isShowing = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        if (setNativeSizeNow)
        {
            Sprite sprite = Resources.Load<Sprite>(data);

            if (sprite != null)
            {
                if (withinMultipleSprite)
                {
                    Object[] assets = Resources.LoadAll<Sprite>(data);

                    if (multipleSpriteIndex < assets.Length) GetComponent<Image>().sprite = (Sprite)assets[multipleSpriteIndex];
                }
                else GetComponent<Image>().sprite = sprite;
                GetComponent<Image>().SetNativeSize();

                GetComponent<Image>().sprite = null;

                setNativeSizeNow = false;
            }
        }

        if (preview)
        {
            if (isPreviewed) return;

            isPreviewed = true;
            Sprite sprite = null;

            if (withinMultipleSprite)
            {
                Object[] assets = Resources.LoadAll<Sprite>(data);

                if (multipleSpriteIndex < assets.Length) sprite = (Sprite)assets[multipleSpriteIndex];
            }
            else sprite = Resources.Load<Sprite>(data);

            if (sprite != null)
            {
                GetComponent<Image>().sprite = sprite;
                GetComponent<Image>().SetNativeSize();
            }
        }
        else
        {
            if (GetComponent<Image>().sprite != null)
            {
                GetComponent<Image>().sprite = null;
            }

            isPreviewed = false;
        }
    }
#endif
}
