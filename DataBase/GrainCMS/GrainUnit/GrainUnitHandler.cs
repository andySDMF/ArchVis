using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Grain.CMS;

public class GrainUnitHandler : MonoBehaviour, IGrainUnit
{
    [SerializeField]
    protected List<GameObject> unitObjects;

    [Header("Events")]
    [SerializeField]
    protected UnityEvent onInitComplete = new UnityEvent();

    protected bool init = false;

    private void Awake()
    {
        GrainCMSUtils.UnitHandler = this;
    }

    public virtual void Init()
    {
        Apply();

        if (!init) onInitComplete.Invoke();

        init = true;
    }

    public virtual void Apply()
    {
        foreach (GameObject go in unitObjects)
        {
            IGrainUnit[] interfaces = go.GetComponentsInChildren<IGrainUnit>();

            for (int i = 0; i < interfaces.Length; i++)
            {
                interfaces[i].Init();
            }
        }
    }
}
