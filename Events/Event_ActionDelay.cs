using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_ActionDelay : MonoBehaviour
{
    [SerializeField]
    private float delay = 0.0f;

    [SerializeField]
    private UnityEvent action = new UnityEvent();

    private bool cancel = false;

    public void Cancel()
    {
        cancel = true;
        StopCoroutine("Process");
    }

    public void Begin()
    {
        if (!gameObject.activeInHierarchy) return;

        cancel = false;
        StopCoroutine("Process");
        StartCoroutine(Process());
    }

    private IEnumerator Process()
    {
        yield return new WaitForSeconds(delay);

        if(!cancel) action.Invoke();
    }

    public void Jump()
    {
        action.Invoke();
    }
}
