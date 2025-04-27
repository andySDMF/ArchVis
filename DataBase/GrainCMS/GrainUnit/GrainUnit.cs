using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Grain.CMS;

public class GrainUnit : MonoBehaviour, IGrainUnit
{
    [SerializeField]
    protected List<SelectTarget> onSelectTargets = new List<SelectTarget>();

    public bool IsSelected { get; private set; }

    public GrainUnitData UnitData { get; private set; }

    public virtual void Init()
    {
        UnitData = GetComponent<GrainUnitData>();

        if (UnitData != null) UnitData.Init();
    }

    public virtual void SetSelected(bool set)
    {
        IsSelected = set;
    }

    public virtual void OnClick()
    {
        SetSelected(true);

        onSelectTargets.ForEach(t => t.Send(this));
    }

    [System.Serializable]
    public class UnitInformation
    {
        public GameObject GO;

        public UnitInformation(MonoBehaviour mono)
        {
            GO = mono.gameObject;
        }
    }

    [System.Serializable]
    public class SelectTarget
    {
        public GameObject target;
        public string command = "";

        public void Send(MonoBehaviour mono)
        {
            if(target != null)
            {
                UnitInformation unitInfo = new UnitInformation(mono);

                target.SendMessage(command, JsonUtility.ToJson(unitInfo));
            }
        }
    }
}
