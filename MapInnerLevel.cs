using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.Map;

public class MapInnerLevel : MonoBehaviour, IMapRegenerate
{
    [SerializeField]
    private int level = 0;

    public int GetLevel { get { return level; } }

    public void Clear()
    {

    }

    public void Regenerate(float val = 0.0f)
    {
        if(level <= (int)val)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }
        else
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }
    }
}
