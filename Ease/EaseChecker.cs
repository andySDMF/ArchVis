using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Ease;

public class EaseChecker : MonoBehaviour
{
    [SerializeField]
    private GameObject easeObject;

    private IEase ease;

    public bool Check(bool state)
    {
        if (ease == null) ease = easeObject.GetComponent<IEase>();

        return ease.State.Equals(state);
    }

    public void CheckThenPerform(bool state)
    {
        if (ease == null) ease = easeObject.GetComponent<IEase>();

        if(ease.State.Equals(state))
        {
            if(state)
            {
                ease.EaseOut();
            }
            else
            {
                ease.EaseIn();
            }
        }
    }
}
