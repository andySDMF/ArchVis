using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOutThenIn : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration = 1.0f;

    [SerializeField]
    private float blackoutDuration = 1.0f;

    [SerializeField]
    private List<BlackOutevent> events = new List<BlackOutevent>();

    private CanvasGroup cGroup;

    public bool IsPerforming { get; private set; }

    private void OnEnable()
    {
        if (cGroup == null) cGroup = GetComponent<CanvasGroup>();
        cGroup.alpha = 0.0f;
        cGroup.blocksRaycasts = false;
    }

    private void OnDisable()
    {
        if (cGroup == null) cGroup = GetComponent<CanvasGroup>();
        cGroup.alpha = 0.0f;
        cGroup.blocksRaycasts = false;
    }

    public void BeginBlackout(string eventID)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        if (cGroup == null) cGroup = GetComponent<CanvasGroup>();
        cGroup.alpha = 0.0f;

        IsPerforming = false;

        StopAllCoroutines();
        StartCoroutine(Perform(eventID));
    }

    public void ApplyEvent(string eventID)
    {
        if (string.IsNullOrEmpty(eventID)) return;

        BlackOutevent evt = events.FirstOrDefault(x => x.id.Equals(eventID));

        if(evt != null)
        {
            evt.onApply.Invoke();
        }
    }

    private IEnumerator Perform(string eventID)
    {
        IsPerforming = true;

        float runningTime = 0.0f;
        float percentage = 0.0f;

        float current = cGroup.alpha;
        cGroup.blocksRaycasts = true;

        while (percentage < 1.0f)
        {
            runningTime += Time.deltaTime;
            percentage = runningTime / fadeDuration;

            cGroup.alpha = Mathf.Lerp(current, 1.0f, percentage);

            yield return null;
        }

        cGroup.alpha = 1.0f;
        ApplyEvent(eventID);
        runningTime = 0.0f;
        percentage = 0.0f;

        yield return new WaitForSeconds(blackoutDuration);

        while (percentage < 1.0f)
        {
            runningTime += Time.deltaTime;
            percentage = runningTime / fadeDuration;

            cGroup.alpha = Mathf.Lerp(1.0f, 0.0f, percentage);

            yield return null;
        }

        cGroup.alpha = 0.0f;
        cGroup.blocksRaycasts = false;
        IsPerforming = false;
    }

    [System.Serializable]
    private class BlackOutevent
    {
        public string id = "";
        public UnityEvent onApply = new UnityEvent();
    }
}
