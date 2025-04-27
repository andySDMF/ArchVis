using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Event_ScrollViewAutoScroll : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollView;

    [SerializeField]
    private Vector2 from;

    [SerializeField]
    private Vector2 to;

    [SerializeField]
    private float duration = 1.0f;

    private Vector2 current;
    private Vector2 target;

    public void Begin(bool state)
    {
        StopAllCoroutines();

        current = new Vector2(scrollView.content.localPosition.x, scrollView.content.localPosition.y);

        if(state)
        {
            target = to;
        }
        else
        {
            target = from;
        }

        StartCoroutine(Process());
    }

    private IEnumerator Process()
    {
        float runningTime = 0.0f;
        float percentage = 0.0f;

        while(percentage < 1.0f)
        {
            runningTime += Time.deltaTime;
            percentage = runningTime / duration;

            scrollView.content.localPosition = new Vector3(EaseOutQuad(current.x, target.x, percentage), EaseOutQuad(current.y, target.y, percentage), 0.0f);

            yield return null;
        }

        scrollView.content.localPosition = new Vector3(target.x, target.y, 0.0f);
    }

    protected float EaseOutQuad(float start, float end, float val)
    {
        end -= start;
        return -end * val * (val - 2) + start;
    }
}
