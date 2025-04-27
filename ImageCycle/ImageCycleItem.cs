using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageCycleItem : MonoBehaviour
{
    [SerializeField]
    protected ImageCyclePositionHandler handler;

    [Header("Tweening")]
    [SerializeField]
    private float duration;

    [SerializeField]
    private Vector3 start;

    [SerializeField]
    private Vector3 end;

    protected float runningTime = 0.0f;
    protected float percentage = 0.0f;

    protected bool isRunning = false;

    protected int cacheSibling = 0;

    private void Start()
    {
        cacheSibling = transform.GetSiblingIndex();
    }

    private void Update()
    {
        if (isRunning)
        {
            Running();
        }
    }

    protected virtual void Running()
    {
        runningTime += Time.deltaTime;
        percentage = runningTime / duration;

        Vector3 val = new Vector3(Ease(start.x, end.x, percentage),
            Ease(start.y, end.y, percentage),
            Ease(start.z, end.z, percentage));

        transform.localPosition = val;

        if (percentage >= 1.0f)
        {
            isRunning = false;

            if (handler != null)
            {
                transform.SetAsFirstSibling();
                transform.localPosition = start;

                handler.Begin();
            }
        }
    }

    public virtual void Begin()
    {
        runningTime = 0.0f;
        percentage = 0.0f;

        isRunning = true;
    }

    public virtual void End()
    {
        isRunning = false;
        transform.SetSiblingIndex(cacheSibling);
        transform.localPosition = start;
    }

    public virtual void Set()
    {

    }

    private float Ease(float start, float end, float val)
    {
        end -= start;
        return -end * val * (val - 2) + start;
    }
}
