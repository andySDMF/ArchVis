using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Ease;

[RequireComponent(typeof(RectTransform))]
public class EaseDeltaSize : MonoBehaviour, IEase
{
    [Header("Ease In")]
    [SerializeField]
    private EaseSettings easeInSettings = new EaseSettings();

    [Header("Ease Out")]
    [SerializeField]
    private EaseSettings easeOutSettings = new EaseSettings();

    [SerializeField]
    public EaseGroup easeGroup;

    public bool IsEasing { get; private set; }
    public bool State
    {
        get
        {
            if (rectT != null)
            {
                return (rectT.sizeDelta.Equals(easeInSettings.start)) ? false : true;
            }
            else return false;
        }
    }

    private delegate float Ease(float start, float end, float percent);

    public string Name { get; private set; }

    private EaseParams easeParams = new EaseParams();

    private Vector3 origin;
    private Vector3 target;
    private float duration;
    private float delay;

    private float percentage = 0.0f;
    private float runningTime = 0.0f;

    private RectTransform rectT;

    private void Awake()
    {
        Name = gameObject.name;

        if (rectT == null)
        {
            rectT = GetComponent<RectTransform>();
        }

        if (easeGroup != null) easeGroup.Listeners.Add((IEase)this);
    }

    private void Update()
    {
        if (!IsEasing) return;

        Fire();
    }

    public void Jump(bool state)
    {
        IsEasing = false;

        if (rectT == null)
        {
            rectT = GetComponent<RectTransform>();
        }

        if (state)
        {
            rectT.sizeDelta = easeInSettings.start;
        }
        else
        {
            rectT.sizeDelta = easeOutSettings.start;
        }
    }

    public void EaseIn()
    {
        if (rectT == null)
        {
            rectT = GetComponent<RectTransform>();
        }

        if (IsEasing)
        {
            IsEasing = false;

            origin = rectT.sizeDelta;
            target = easeInSettings.end;
            duration = easeInSettings.duration;
        }
        else
        {
            origin = easeInSettings.start;
            target = easeInSettings.end;
            duration = easeInSettings.duration;
        }

        runningTime = 0.0f;
        percentage = 0.0f;

        easeParams.Set(easeInSettings.easeStyle);

        IsEasing = true;
    }

    public void EaseOut()
    {
        if (rectT == null)
        {
            rectT = GetComponent<RectTransform>();
        }

        if (IsEasing)
        {
            IsEasing = false;

            origin = rectT.sizeDelta;
            target = easeInSettings.end;
            duration = easeInSettings.duration;
        }
        else
        {
            origin = easeOutSettings.start;
            target = easeOutSettings.end;
            duration = easeOutSettings.duration;
        }

        runningTime = 0.0f;
        percentage = 0.0f;

        easeParams.Set(easeOutSettings.easeStyle);

        IsEasing = true;
    }

    public void Toggle(bool state)
    {
        if (state) EaseIn();
        else EaseOut();
    }

    private void Fire()
    {
        runningTime += Time.deltaTime;
        percentage = runningTime / duration;

        Vector2 val = new Vector3(easeParams.EaseFunc(origin.x, target.x, percentage),
                          easeParams.EaseFunc(origin.y, target.y, percentage));

        rectT.sizeDelta = val;

        if (percentage >= 1.0f)
        {
            rectT.sizeDelta = target;
            IsEasing = false;
        }
    }

    [System.Serializable]
    private class EaseSettings
    {
        public Vector2 start;
        public Vector2 end;
        public float duration = 1.0f;

        public EaseStyle easeStyle = EaseStyle._Linear;
    }
}
