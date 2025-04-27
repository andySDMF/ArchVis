using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Ease;
using UnityEngine.UI;

public class EaseImageFill : MonoBehaviour
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
            if (subject != null)
            {
                return (subject.fillAmount.Equals(easeInSettings.start)) ? false : true;
            }
            else return false;
        }
    }

    private delegate float Ease(float start, float end, float percent);

    public string Name { get; private set; }

    private EaseParams easeParams = new EaseParams();

    private float origin;
    private float target;
    private float duration;
    private float delay;

    private Image subject;

    private float percentage = 0.0f;
    private float runningTime = 0.0f;

    private bool valid = true;

    private void Awake()
    {
        Name = gameObject.name;

        if (easeGroup != null) easeGroup.Listeners.Add((IEase)this);

        GetSubject();
    }

    private void Update()
    {
        if (!IsEasing) return;

        Fire();
    }

    public void Jump(bool state)
    {
        GetSubject();

        IsEasing = false;

        if (!valid) return;

        if (state)
        {
            subject.fillAmount = easeInSettings.start;
        }
        else
        {
            subject.fillAmount = easeOutSettings.start;
        }
    }

    public void EaseIn()
    {
        GetSubject();

        if (!valid) return;

        if (IsEasing)
        {
            IsEasing = false;

            origin = subject.fillAmount;
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
        GetSubject();

        if (!valid) return;

        if (IsEasing)
        {
            IsEasing = false;

            origin = subject.fillAmount;
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

    private void GetSubject()
    {
        if (subject != null) return;

        subject = GetComponent<Image>();
        valid = true;

        if (subject != null)
        {
            if (subject.type != Image.Type.Filled) valid = false;
        }
        else valid = false;
    }

    private void Fire()
    {
        runningTime += Time.deltaTime;
        percentage = runningTime / duration;

        float val = easeParams.EaseFunc(origin, target, percentage);

        subject.fillAmount = val;

        if (percentage >= 1.0f)
        {
            subject.fillAmount = target;
            IsEasing = false;
        }
    }

    [System.Serializable]
    private class EaseSettings
    {
        [Range(0.0f, 1.0f)]
        public float start;
        [Range(0.0f, 1.0f)]
        public float end;
        public float duration = 1.0f;

        public EaseStyle easeStyle = EaseStyle._Linear;
    }
}
