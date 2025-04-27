using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Event_CanvasPulse : MonoBehaviour
{
    [SerializeField]
    private float pulseDuration;

    private CanvasGroup cGroup;

    private bool process = false;
    private bool state = false;

    private float runningTime = 0.0f;
    private float percentage = 0.0f;

    private void Update()
    {
        if(process)
        {
            runningTime += Time.deltaTime;
            percentage = runningTime / pulseDuration;

            if(!state)
            {
                cGroup.alpha = Linear(0.0f, 1.0f, percentage);
            }
            else
            {
                cGroup.alpha = Linear(1.0f, 0.0f, percentage);
            }

            if(percentage >= 1.0f)
            {
                runningTime = 0.0f;
                percentage = 0.0f;
                state = !state;
            }
        }
    }

    public void Begin()
    {
        if (cGroup == null) cGroup = GetComponent<CanvasGroup>();

        cGroup.alpha = 0.0f;
        runningTime = 0.0f;
        percentage = 0.0f;

        state = false;

        process = true;
    }

    public void End()
    {
        if (cGroup == null) cGroup = GetComponent<CanvasGroup>();

        process = false;
        cGroup.alpha = 0.0f;
    }

    private float Linear(float start, float end, float val)
    {
        return Mathf.Lerp(start, end, val);
    }
}
