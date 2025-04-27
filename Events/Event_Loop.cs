using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_Loop : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onBegin = new UnityEvent();

    [SerializeField]
    private UnityEvent onEnd = new UnityEvent();

    [SerializeField]
    private float loopAfter = 1.0f;

    private bool process = false;
    private float timer = 0.0f;

    private void Update()
    {
        if(process)
        {
            timer += Time.deltaTime;

            if(timer >= loopAfter)
            {
                Begin();
            }
        }
    }

    public void Begin()
    {
        timer = 0.0f;
        onBegin.Invoke();

        process = true;
    }

    public void End()
    {
        process = false;
        onEnd.Invoke();
    }
}
