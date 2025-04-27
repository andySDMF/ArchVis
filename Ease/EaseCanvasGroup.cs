using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Ease;

[RequireComponent(typeof(CanvasGroup))]
public class EaseCanvasGroup : MonoBehaviour, IEase
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
            if (cGroup != null)
            {
                return (cGroup.alpha.Equals(easeInSettings.alphaStart)) ? false : true;
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

    private float percentage = 0.0f;
    private float runningTime = 0.0f;

    private CanvasGroup cGroup;

    private void Awake()
    {
        Name = gameObject.name;

        cGroup = GetComponent<CanvasGroup>();

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

        if (cGroup == null) cGroup = GetComponent<CanvasGroup>();

        if (state)
        {
            cGroup.alpha = easeInSettings.alphaStart;
        }
        else
        {
            cGroup.alpha = easeOutSettings.alphaStart;
        }
    }

    public void EaseIn()
    {
        if (IsEasing)
        {
            IsEasing = false;

            origin = cGroup.alpha;
            target = easeInSettings.alphaEnd;
            duration = easeInSettings.duration;
        }
        else
        {
            origin = easeInSettings.alphaStart;
            target = easeInSettings.alphaEnd;
            duration = easeInSettings.duration;
        }

        runningTime = 0.0f;
        percentage = 0.0f;

        easeParams.Set(easeInSettings.easeStyle);

        IsEasing = true;
    }

    public void EaseOut()
    {
        if (IsEasing)
        {
            IsEasing = false;

            origin = cGroup.alpha;
            target = easeInSettings.alphaEnd;
            duration = easeInSettings.duration;
        }
        else
        {
            origin = easeOutSettings.alphaStart;
            target = easeOutSettings.alphaEnd;
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

        float val = easeParams.EaseFunc(origin, target, percentage);

        cGroup.alpha = val;

        if (percentage >= 1.0f)
        {
            cGroup.alpha = target;
            IsEasing = false;
        }
    }

    [System.Serializable]
    private class EaseSettings
    {
        public float alphaStart;
        public float alphaEnd;
        public float duration = 1.0f;

        public EaseStyle easeStyle = EaseStyle._Linear;
    }
}
