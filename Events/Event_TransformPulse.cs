using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_TransformPulse : MonoBehaviour
{
    [SerializeField]
    private Vector3 startScale = Vector3.one;

    [SerializeField]
    private Vector3 endScale = new Vector3(1.2f, 1.2f, 1.2f);

    [SerializeField]
    private float duration = 1.0f;

    private bool isPulsing = false;
    private bool state = false;

    private float runningTime = 0.0f;
    private float percentage = 0.0f;

    private float halfDuration = 0.0f;

    public Vector3 StartScale { get { return startScale; } set { startScale = value; } }
    public Vector3 EndScale { get { return endScale; } set { endScale = value; } }

    public void Update()
    {
        if (isPulsing)
        {
            runningTime += Time.deltaTime;
            percentage = runningTime / halfDuration;

            if (state)
            {
                transform.localScale = new Vector3(EaseOutQuad(startScale.x, endScale.x, percentage),
                    EaseOutQuad(startScale.y, endScale.y, percentage),
                    EaseOutQuad(startScale.z, endScale.z, percentage));
            }
            else
            {
                transform.localScale = new Vector3(EaseOutQuad(endScale.x, startScale.x, percentage),
                    EaseOutQuad(endScale.y, startScale.y, percentage),
                    EaseOutQuad(endScale.z, startScale.z, percentage));
            }

            if (percentage >= 1.0f)
            {
                runningTime = 0.0f;
                percentage = 0.0f;

                state = !state;
            }
        }
    }

    public void ResetScale()
    {
        state = false;
        transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        runningTime = 0.0f;
        percentage = 0.0f;
        halfDuration = duration / 2;

        isPulsing = true;
    }

    public void Begin()
    {
        state = false;
        transform.localScale = startScale;

        runningTime = 0.0f;
        percentage = 0.0f;
        halfDuration = duration / 2;

        isPulsing = true;
    }

    public void End()
    {
        isPulsing = false;
        state = false;
        transform.localScale = startScale;
    }

    public void Set(Vector3 x, Vector3 y)
    {
        startScale = x;
        endScale = y;
    }

    private float EaseOutQuad(float start, float end, float val)
    {
        end -= start;
        return -end * val * (val - 2) + start;
    }
}
