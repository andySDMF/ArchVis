using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_LookAtObject : MonoBehaviour
{
    [SerializeField]
    private Transform lookAt;

    [SerializeField]
    protected Contraints contraints;

    [SerializeField]
    private bool processOnEnable = true;

    private bool process = false;
    private Vector3 targetPostition;

    private void OnEnable()
    {
        if (processOnEnable) Begin();
    }

    private void LateUpdate()
    {
        if (!process && lookAt != null) return;

        targetPostition = new Vector3((!contraints.contrainX) ? lookAt.position.x : this.transform.position.x, (!contraints.contrainY) ? lookAt.position.y : this.transform.position.y, (!contraints.contrainZ) ? lookAt.position.z : this.transform.position.z);

        transform.LookAt(targetPostition);
    }

    public void Begin()
    {
        process = true;
    }

    public void End()
    {
        process = false;
    }

    [System.Serializable]
    protected class Contraints
    {
        public bool contrainX = false;
        public bool contrainY = false;
        public bool contrainZ = false;
    }

    [System.Serializable]
    protected class Limits
    {
        public float from;
        public float to;
    }
}
