using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class XrayManager : MonoBehaviour
{
    public Camera cam;
	public Color solidBG;
	public Color xrayBG;
	
	public LayerMask SolidLayers;
	public LayerMask XrayLayers;
	public UnityEngine.UI.Toggle XrayToggle;

    [SerializeField]
    private UnityEvent onTrue = new UnityEvent();

    [SerializeField]
    private UnityEvent onFalse = new UnityEvent();

    public static XrayManager instance;

    public bool IsOn { get; private set; }

    void Start()
    {
        instance = this;
    }
	
	public void ToggleXray(bool _TurnXrayOn)
	{

        //RemoteControlClient.SendRemoteControlCommand("ToggleXray", _TurnXrayOn.ToString());
        if (cam == null) return;

        if (_TurnXrayOn)
		{
            cam.backgroundColor = xrayBG;
            cam.cullingMask = XrayLayers;
			//GetComponent<PostProcessingBehaviour>().enabled = false;
			if(XrayToggle)
			    XrayToggle.isOn = true;

            onTrue.Invoke();

            IsOn = true;


        }
		
		else{

            cam.backgroundColor = solidBG;
            cam.cullingMask = SolidLayers;
            //GetComponent<PostProcessingBehaviour>().enabled = true;
            if (XrayToggle)
                XrayToggle.isOn = false;

            onFalse.Invoke();

            IsOn = false;

        }
	}

    public void SyncXray(bool _XrayOn)
    {
        if(_XrayOn)
        {
            cam.backgroundColor = xrayBG;
            cam.cullingMask = XrayLayers;
        }

        {
            cam.backgroundColor = solidBG;
            cam.cullingMask = SolidLayers;
        }

    }


}
