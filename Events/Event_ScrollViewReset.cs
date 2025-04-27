using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class Event_ScrollViewReset : MonoBehaviour
{
    [SerializeField]
    private ResetType resetOn = ResetType._OnEnable;

    [SerializeField]
    private ResetVector resetPosition = ResetVector._Up;

    private ScrollRect scrollRect;

    private void Awake()
    {
        if(scrollRect ==  null) scrollRect = GetComponent<ScrollRect>();
    }

    private void OnEnable()
    {
        if(resetOn.Equals(ResetType._OnEnable))
        {
            ResetNow();
        }
    }

    private void OnDisable()
    {
        if (resetOn.Equals(ResetType._OnDisable))
        {
            ResetNow();
        }
    }

    public void ResetNow()
    {
        Awake();

        switch(resetPosition)
        {
            case ResetVector._Up:
                scrollRect.normalizedPosition = Vector2.up;
                break;
            case ResetVector._Down:
                scrollRect.normalizedPosition = Vector2.down;
                break;
            case ResetVector._Left:
                scrollRect.normalizedPosition = Vector2.left;
                break;
            case ResetVector._Right:
                scrollRect.normalizedPosition = Vector2.right;
                break;
            default:
                scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
                break;
        }
    }

    [System.Serializable]
    private enum ResetType { _OnEnable, _OnDisable }

    [System.Serializable]
    private enum ResetVector { _Up, _Down, _Left, _Right, _Center }
}
