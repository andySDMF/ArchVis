using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageCycleFadeHandler : MonoBehaviour
{
    [Header("Display")]
    [SerializeField]
    private float displayDuration = 5.0f;

    [SerializeField]
    private List<string> images;

    [Header("Fade")]
    [SerializeField]
    private float fadeDuration = 5.0f;

    [SerializeField]
    private RawImage backImage;

    [SerializeField]
    private CanvasGroup backCanvas;

    [SerializeField]
    private RawImage frontImage;

    [SerializeField]
    private CanvasGroup frontCanvas;

    [Header("Indication")]
    [SerializeField]
    private ImageCycleIndicatorControl indicationController;

    private int currentImage = 1;

    private bool isDuration = false;
    private bool isFading = false;

    private float time = 0.0f;

    protected float runningTime = 0.0f;
    protected float percentage = 0.0f;

    private CanvasGroup current;
    private bool indicatorChanged = false;

    private void Awake()
    {
       // LoadImage(frontImage, 0);
       // LoadImage(backImage, 1);

        if (indicationController != null)
        {
            indicationController.Append(images.Count.ToString());
        }
    }

    private void OnEnable()
    {
        Begin();
    }

    private void OnDisable()
    {
        End();

        Resources.UnloadUnusedAssets();
    }

    private void Update()
    {
        if (isDuration)
        {
            time += Time.deltaTime;

            if (time > displayDuration)
            {
                isDuration = false;
                isFading = true;
            }
        }

        if (isFading)
        {
            runningTime += Time.deltaTime;
            percentage = runningTime / fadeDuration;

            if (current.Equals(frontCanvas))
            {
                frontCanvas.alpha = Mathf.Lerp(1.0f, 0.0f, percentage);
                backCanvas.alpha = Mathf.Lerp(0.0f, 1.0f, percentage);
            }
            else
            {
                backCanvas.alpha = Mathf.Lerp(1.0f, 0.0f, percentage);
                frontCanvas.alpha = Mathf.Lerp(0.0f, 1.0f, percentage);
            }

            if(percentage > 0.5f && !indicatorChanged)
            {
                indicatorChanged = true;

                if (indicationController != null)
                {
                    indicationController.Publish(currentImage);
                }
            }

            if (percentage >= 1.0f)
            {
                currentImage++;

                if (currentImage > images.Count - 1)
                {
                    currentImage = 0;
                }

                if (current.Equals(frontCanvas))
                {
                    current = backCanvas;

                    LoadImage(frontImage, currentImage);
                }
                else
                {
                    current = frontCanvas;

                    LoadImage(backImage, currentImage);
                }

                runningTime = 0.0f;
                percentage = 0.0f;

                indicatorChanged = false;

                time = 0.0f;
                isFading = false;
                isDuration = true;
            }
        }
    }

    public void Begin()
    {
        current = frontCanvas;

        LoadImage(frontImage, 0);
        LoadImage(backImage, 1);

        time = 0.0f;
        percentage = 0.0f;
        runningTime = 0.0f;
        currentImage = 1;

        if(indicationController != null)
        {
            indicationController.Publish(currentImage - 1);
        }

        isDuration = true;
    }

    public void End()
    {
        currentImage = 0;

        frontImage.texture = null;
        backImage.texture = null;

        frontCanvas.alpha = 1.0f;
        backCanvas.alpha = 0.0f;
        current = frontCanvas;

        isFading = false;
        isDuration = false;
    }

    private void LoadImage(RawImage img, int index)
    {
        img.texture = Resources.Load(images[index]) as Texture;
    }

    public void Jump(int index)
    {
        if (isFading) return;

        currentImage = index;

        if (current.Equals(frontCanvas))
        {
            LoadImage(backImage, currentImage);
        }
        else
        {
            LoadImage(frontImage, currentImage);
        }

        time = 0.0f;
        percentage = 0.0f;
        runningTime = 0.0f;

        isDuration = false;
        isFading = true;
    }
}
