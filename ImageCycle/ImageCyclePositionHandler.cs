using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageCyclePositionHandler : MonoBehaviour
{
    [Header("Display")]
    [SerializeField]
    private float duration = 5.0f;

    [SerializeField]
    List<ImageCycleItem> cycleItems;

    private bool isTiming = false;
    private float currentTime = 0.0f;

    private int currentObject = 0;

    private void Start()
    {
        Begin();
    }

    private void Update()
    {
        if (isTiming)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= duration)
            {
                isTiming = false;
                Perform();
            }
        }
    }

    public void End()
    {
        currentObject = 0;

        cycleItems.ForEach(s => s.End());
    }

    public void Begin()
    {
        if (isTiming) return;

        isTiming = true;
        currentTime = 0.0f;
    }

    private void Perform()
    {
        if (currentObject > cycleItems.Count - 1)
        {
            currentObject = 0;
        }

        if (currentObject + 1 > cycleItems.Count - 1)
        {
            cycleItems[0].Set();
        }
        else
        {
            cycleItems[currentObject + 1].Set();
        }

        cycleItems[currentObject].Begin();

        currentObject++;
    }
}
