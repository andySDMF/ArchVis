using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SequentialListing : MonoBehaviour
{
    [SerializeField]
    private float interval = 1.0f;

    [SerializeField]
    private List<SequentialListingObject> listingObjects = new List<SequentialListingObject>();

    [SerializeField]
    private UnityEvent onBegin = new UnityEvent();

    [SerializeField]
    private UnityEvent onEnd = new UnityEvent();

    private bool state = false;

    public void Show()
    {
        if (state) return;

        state = true;

        StopCoroutine("Process");
        StartCoroutine(Process(true));
    }

    public void Hide()
    {
        if (!state) return;

        state = false;

        StopCoroutine("Process");
        StartCoroutine(Process(false));
    }

    public void ResetThis()
    {
        state = false;
        StopCoroutine("Process");
        listingObjects.ForEach(l => l.ResetThis());
    }

    private IEnumerator Process(bool state)
    {
        int count = 0;
        WaitForSeconds wait = new WaitForSeconds(interval);

        onBegin.Invoke();

        if (state)
        {
            while(count < listingObjects.Count)
            {
                listingObjects[count].Show();

                yield return wait;

                count++;
            }
        }
        else
        {
            count = listingObjects.Count - 1;

            while (count >= 0)
            {
                listingObjects[count].Hide();

                yield return wait;

                count--;
            }
        }

        onEnd.Invoke();
    }
}
