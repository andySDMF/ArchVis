using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MWM.Ease;

public class EaseLayoutElement : MonoBehaviour, IEase
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
        get; private set;
    }

    private delegate float Ease(float start, float end, float percent);

    public string Name { get; private set; }

    private EaseParams easeParams = new EaseParams();

    private Vector2 originMin;
    private Vector2 targetMin;
    private Vector2 originPrefered;
    private Vector2 targetPrefered;
    private float duration;
    private float delay;

    private float percentage = 0.0f;
    private float runningTime = 0.0f;
    private LayoutElement element;

    private void Awake()
    {
        Name = gameObject.name;

        if (element == null) element = GetComponent<LayoutElement>();

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

        if (element == null) element = GetComponent<LayoutElement>();

        if (state)
        {
            if(easeInSettings.useMin)
            {
                element.minHeight = easeInSettings.fromMin.y;
                element.minWidth = easeInSettings.fromMin.x;
            }

            if (easeInSettings.usePrefered)
            {
                element.preferredHeight = easeInSettings.fromPrefered.y;
                element.preferredWidth = easeInSettings.fromPrefered.x;
            }
        }
        else
        {
            if (easeOutSettings.useMin)
            {
                element.minHeight = easeInSettings.fromMin.y;
                element.minWidth = easeInSettings.fromMin.x;
            }

            if (easeOutSettings.usePrefered)
            {
                element.preferredHeight = easeInSettings.fromPrefered.y;
                element.preferredWidth = easeInSettings.fromPrefered.x;
            }
        }

        State = state;
    }


    public void EaseIn()
    {
        if (IsEasing)
        {
            IsEasing = false;

            if (easeInSettings.useMin)
            {
                originMin = new Vector2(element.minWidth, element.minHeight);
                targetMin = new Vector2(easeInSettings.toMin.x, easeInSettings.toMin.y);
            }

            if (easeInSettings.usePrefered)
            {
                originPrefered = new Vector2(element.preferredWidth, element.preferredHeight);
                targetPrefered = new Vector2(easeInSettings.toPrefered.x, easeInSettings.toPrefered.y);
            }

            duration = easeInSettings.duration;
        }
        else
        {
            if (easeInSettings.useMin)
            {
                originMin = new Vector2(easeInSettings.fromMin.x, easeInSettings.fromMin.y);
                targetMin = new Vector2(easeInSettings.toMin.x, easeInSettings.toMin.y);
            }

            if (easeInSettings.usePrefered)
            {
                originPrefered = new Vector2(easeInSettings.fromPrefered.x, easeInSettings.fromPrefered.y);
                targetPrefered = new Vector2(easeInSettings.toPrefered.x, easeInSettings.toPrefered.y);
            }

            duration = easeInSettings.duration;
        }

        runningTime = 0.0f;
        percentage = 0.0f;

        State = true;

        easeParams.Set(easeInSettings.easeStyle);

        IsEasing = true;
    }

    public void EaseOut()
    {
        if (IsEasing)
        {
            IsEasing = false;

            if (easeInSettings.useMin)
            {
                originMin = new Vector2(element.minWidth, element.minHeight);
                targetMin = new Vector2(easeInSettings.fromMin.x, easeInSettings.fromMin.y);
            }

            if (easeInSettings.usePrefered)
            {
                originPrefered = new Vector2(element.preferredWidth, element.preferredHeight);
                targetPrefered = new Vector2(easeInSettings.fromPrefered.x, easeInSettings.fromPrefered.y);
            }

            duration = easeInSettings.duration;
        }
        else
        {
            if (easeInSettings.useMin)
            {
                originMin = new Vector2(easeInSettings.toMin.x, easeInSettings.toMin.y);
                targetMin = new Vector2(easeInSettings.fromMin.x, easeInSettings.fromMin.y);
            }

            if (easeInSettings.usePrefered)
            {
                originPrefered = new Vector2(easeInSettings.toPrefered.x, easeInSettings.toPrefered.y);
                targetPrefered = new Vector2(easeInSettings.fromPrefered.x, easeInSettings.fromPrefered.y);
            }

            duration = easeOutSettings.duration;
        }

        runningTime = 0.0f;
        percentage = 0.0f;

        State = false;

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

        Vector2 min = new Vector2(easeParams.EaseFunc(originMin.x, targetMin.x, percentage), easeParams.EaseFunc(originMin.y, targetMin.y, percentage));
        Vector2 pref = new Vector2(easeParams.EaseFunc(originPrefered.x, targetPrefered.x, percentage), easeParams.EaseFunc(originPrefered.y, targetPrefered.y, percentage));

        element.minHeight = min.y;
        element.minWidth = min.x;
        element.preferredHeight = pref.y;
        element.preferredWidth = pref.x;

        if (percentage >= 1.0f)
        {
            element.minHeight = targetMin.y;
            element.minWidth = targetMin.x;
            element.preferredHeight = targetPrefered.y;
            element.preferredWidth = targetPrefered.x;

            IsEasing = false;
        }
    }

    [System.Serializable]
    private class EaseSettings
    {
        public bool useMin = true;
        public Vector2 fromMin;
        public Vector2 toMin;

        public bool usePrefered = false;
        public Vector2 fromPrefered;
        public Vector2 toPrefered;

        public float duration = 1.0f;
        public EaseStyle easeStyle = EaseStyle._Linear;
    }
}
