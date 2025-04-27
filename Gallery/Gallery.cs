using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Grain.Gallery;

public class Gallery : MonoBehaviour, IGallery, IPointerUpHandler, IPointerDownHandler, IDragHandler
{
	[SerializeField]
	private string id = "";

	[SerializeField]
	private bool publishOnAppend = false;

	[SerializeField]
	private Transform indicationController;

	[SerializeField]
	private List<GEntry> entries = new List<GEntry>();

	[Header("Control")]
	[SerializeField]
	private GControlDirection controlDirection = GControlDirection._Horizontal;

	[SerializeField]
	private bool containDragableArea = true;

	[SerializeField]
	[Range(0.1f, 1)]
	private float releaseDuration = 1.0f;

	[SerializeField]
	private float releaseThreshold = 50.0f;

    [Header("On Release Event")]
    [SerializeField]
    private UnityEvent onRelease = new UnityEvent();

	[Header("Data")]
	[SerializeField]
	private RectTransform below;
	[SerializeField]
	private RectTransform center;
	[SerializeField]
	private RectTransform above;

	[SerializeField]
	private float spacing = 0.0f;

	[SerializeField]
	private GalleryBounds gBounds;

	[Header("Debug Mode")]
	[SerializeField]
	private bool debugOn = false;

	public RectTransform BelowGalleryItem { get { return below; } set { below = value; } }

	public RectTransform CenterGalleryItem { get { return center; } set { center = value; } }

	public RectTransform AboveGalleryItem { get { return above; } set { above = value; } }

	public float Spacing { get { return spacing; } set { spacing = value; } }

	public GalleryBounds Bounds { get { return gBounds; } set { gBounds = value; } }

	public string ID { get { return id; } }

    public int Current {  get { return attributes.Index; } }

	public List<GEntry> Entries { get { return entries; } }

    public System.Action<int> OnChange { get; set; }

	private string rawSource = "";
	private GalleryAttributes attributes = new GalleryAttributes();
	private bool released = false;
	private bool returning = false;
	private bool freeze = false;

	private float runningTime = 0.0f;
	private float percentage = 0.0f;

	private Vector3 belowOrigin = Vector3.zero;
	private Vector3 centerOrigin = Vector3.zero;
	private Vector3 aboveOrigin = Vector3.zero;

	private string belowState = "B";
	private string centerState = "C";
	private string aboveState = "A";

	private string belowCache = "B";
	private string centerCache = "C";
	private string aboveCache = "A";

	private Vector3 belowTarget = Vector3.zero;
	private Vector3 centerTarget = Vector3.zero;
	private Vector3 aboveTarget = Vector3.zero;

    private bool onReleasedCalled = false;
    private bool appendOnJump = false;

	private void Awake()
	{
        if (appendOnJump) return;

		if (below == null || center == null || above == null) 
		{
			if (debugOn)
				Debug.Log ("Not all data objects exists. Turning off Gallery [" + gameObject.name + "] behaviour");

			this.enabled = false;
			return;
		}

		attributes.State = "C";
		attributes.Index = 0;

        if (indicationController != null && indicationController.GetComponent<IGallery>() == null) indicationController = null;
	}

	public void Update()
	{
		if (released) 
		{
			runningTime += Time.deltaTime;
			percentage = runningTime / releaseDuration;

			if (below != null) 
			{
				below.localPosition = new Vector3 (Ease (belowOrigin.x, belowTarget.x, percentage),
					Ease (belowOrigin.y, belowTarget.y, percentage),
				0.0f);
			}

			if (center != null) 
			{
				center.localPosition = new Vector3 (Ease (centerOrigin.x, centerTarget.x, percentage),
					Ease (centerOrigin.y, centerTarget.y, percentage),
					0.0f);
			}

			if (above != null) 
			{
				above.localPosition = new Vector3 (Ease (aboveOrigin.x, aboveTarget.x, percentage),
					Ease (belowOrigin.y, aboveTarget.y, percentage),
					0.0f);
			}

            if (!onReleasedCalled)
            {
                onRelease.Invoke();
                onReleasedCalled = true;
            }

            if (percentage >= 1.0f) 
			{
				released = false;

				if (below != null) 
				{
					if (belowState.Equals ("C"))
						below.localPosition = Vector3.zero;
					else if(belowState.Equals("A"))
						below.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection < 1) ? gBounds.Top : 0), 0);
					else
						below.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection < 1) ? gBounds.Bottom : 0), 0);
				}

				if (center != null) 
				{
					if (centerState.Equals ("C"))
						center.localPosition = Vector3.zero;
					else if(centerState.Equals("A"))
						center.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection < 1) ? gBounds.Top : 0), 0);
					else
						center.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection < 1) ? gBounds.Bottom : 0), 0);
				}

				if (above != null) 
				{
					if (aboveState.Equals ("C"))
						above.localPosition = Vector3.zero;
					else if(aboveState.Equals("A"))
						above.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection < 1) ? gBounds.Top : 0), 0);
					else
						above.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection < 1) ? gBounds.Bottom : 0), 0);
				}

                if (entries.Count > 1 && !returning) 
				{
					attributes.Index = (attributes.State.Equals ("B")) ? attributes.Index - 1 : attributes.Index + 1;

                    if (attributes.Index < 0)
						attributes.Index = entries.Count - 1;
					else if (attributes.Index > entries.Count - 1)
						attributes.Index = 0;

                    //if (OnChange != null) OnChange.Invoke(attributes.Index);

                    //Xin sending network data.

                    //print(attributes.Index + "file: " + entries[attributes.Index].elements[0].data);
                    // NetworkUtils.RCClient.Send("ViewDisplay", entries[attributes.Index].elements[0].data);

                    attributes.Previous = ((attributes.Index - 1) < 0) ? entries.Count - 1 : attributes.Index - 1;
					attributes.Next = ((attributes.Index + 1) > entries.Count - 1) ? 0 : attributes.Index + 1;

					if(debugOn) Debug.Log("Gallery [" + gameObject.name + "]indexes are: Indes = " + attributes.Index + ", Previous = " + attributes.Previous + ", Next = " + attributes.Next);

                    onReleasedCalled = false;

                    Publish(1);
				}

                if (OnChange != null) OnChange.Invoke(attributes.Index);

                if (returning)
                {
                    if (indicationController != null)
                        indicationController.GetComponent<IGallery>().Publish(attributes.Index);
                }

				returning = false;
                
			}
		}
	}

	public void Append(string data)
	{
		if (string.IsNullOrEmpty(data))
			return;

		Clear();

		rawSource = data;

		GEntryGroup eGroup = JsonUtility.FromJson<GEntryGroup> (rawSource);

		if (eGroup != null) 
		{
			if(string.IsNullOrEmpty(eGroup.id) || !eGroup.id.Equals(ID))
			{
				return;
			}

			eGroup.entries.ForEach(e => entries.Add(e));
		}

		if(debugOn) Debug.Log("Data appended on Gallery [" + gameObject.name + "]");

		if (indicationController != null)
			indicationController.GetComponent<IGallery>().Append (entries.Count.ToString ());

		freeze = (entries.Count > 1) ? false : true;
			
		if (publishOnAppend) Publish();
	}

	public void Publish(int control = -1)
	{
		if (entries.Count < 1)
			return;

		int n = 0;
		GalleryEntry e;

		if (below != null && !freeze) 
		{	
			e = below.GetComponent<GalleryEntry> ();

			if (control <= 0) 
			{
				n = ((attributes.Index - 1) < 0) ? entries.Count - 1 : attributes.Index - 1;
				e.Append (JsonUtility.ToJson (entries [n]));

				e.Publish ();
			} 
			else 
			{
				if (belowCache.Equals ("B") && belowState.Equals ("A")) 
				{
					e.Append (JsonUtility.ToJson (entries [attributes.Next]));

					e.Publish ();
				} else if (belowCache.Equals ("A") && belowState.Equals ("B")) 
				{
					e.Append (JsonUtility.ToJson (entries [attributes.Previous]));

					e.Publish ();
				}
            }
		}

		if (center != null) 
		{	
			e = center.GetComponent<GalleryEntry>();

			if (control <= 0) 
			{
				e.Append (JsonUtility.ToJson (entries [attributes.Index]));

				e.Publish ();
			} 
			else 
			{
				if (centerCache.Equals ("B") && centerState.Equals ("A")) 
				{
					e.Append (JsonUtility.ToJson (entries [attributes.Next]));

					e.Publish();
				} 
				else if (centerCache.Equals ("A") && centerState.Equals ("B")) 
				{
					e.Append (JsonUtility.ToJson (entries [attributes.Previous]));

					e.Publish();
				}
			}
		}

		if (above != null && !freeze) 
		{	
			e = above.GetComponent<GalleryEntry>();

			if (control <= 0) 
			{
				n = ((attributes.Index + 1) >= entries.Count) ? 0 : attributes.Index + 1;
				e.Append (JsonUtility.ToJson (entries [n]));

				e.Publish ();
			} 
			else 
			{
				if (aboveCache.Equals ("B") && aboveState.Equals ("A")) 
				{
					e.Append (JsonUtility.ToJson (entries [attributes.Next]));

					e.Publish();
				} 
				else if (aboveCache.Equals ("A") && aboveState.Equals ("B")) 
				{
					e.Append (JsonUtility.ToJson (entries [attributes.Previous]));

					e.Publish();
				}
			}
		}

		if (indicationController != null)
            indicationController.GetComponent<IGallery>().Publish (attributes.Index);

        //Xin sending network 
        //if(!entries[attributes.Index].elements[0].data.Contains("url"))
            //NetworkUtils.RCClient.Send("ViewDisplay", entries[attributes.Index].elements[0].data);
        
        if (debugOn) Debug.Log("Published Gallery [" + gameObject.name + "]");
	}

	public void Clear()
	{
		rawSource = "";

		belowState = "B";
		centerState = "C";
		aboveState = "A";

		belowCache = "B";
		centerCache = "C";
		aboveCache = "A";

		attributes.Index = 0;
        
        //------------------------------------

		attributes.State = "C";

		if (below != null) 
		{	
			below.GetComponent<GalleryEntry> ().Clear();

			below.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection < 1) ? gBounds.Bottom : 0), 0);
		}

		if (center != null) 
		{	
			center.GetComponent<GalleryEntry> ().Clear();

			center.localPosition = Vector3.zero;
		}

		if (above != null) 
		{	
			above.GetComponent<GalleryEntry>().Clear();

			above.localPosition = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection < 1) ? gBounds.Top : 0), 0);
		}

		entries.Clear();

		if (indicationController != null)
            indicationController.GetComponent<IGallery>().Clear ();

		if(debugOn) Debug.Log("Cleared Gallery [" + gameObject.name + "]");
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (released || freeze)
			return;

		attributes.PointerPosition = ((int)controlDirection < 1) ? eventData.delta.x : eventData.delta.y;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (released || freeze)
			return;
		
		attributes.State = Identify();

		bool change = ((attributes.PointerPosition + Mathf.Abs(attributes.CurrentPosition)) > releaseThreshold) ? true : false;

		if (!change) 
		{
			Return();
		} 
		else 
		{
			if (attributes.State.Equals ("A")) {
				if (below != null) {

					belowCache = belowState;

					if (belowState.Equals ("A")) {
						belowTarget = Vector3.zero;
						belowState = "C";
					} else if (belowState.Equals ("C")) {
						belowTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection > 1) ? gBounds.Bottom : 0), 0);
						belowState = "B";
					} else {
						belowTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left * 2 : 0), (((int)controlDirection > 1) ? gBounds.Bottom * 2 : 0), 0);
						belowState = "A";
					}
				}

				if (center != null) {

					centerCache = centerState;

					if (centerState.Equals ("A")) {
						centerTarget = Vector3.zero;
						centerState = "C";
					} else if (centerState.Equals ("C")) {
						centerTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection > 1) ? gBounds.Bottom : 0), 0);
						centerState = "B";
					} else {
						centerTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left * 2 : 0), (((int)controlDirection > 1) ? gBounds.Bottom * 2 : 0), 0);
						centerState = "A";
					}
				}

				if (above != null) {

					aboveCache = aboveState;

					if (aboveState.Equals ("A")) {
						aboveTarget = Vector3.zero;
						aboveState = "C";
					} else if (aboveState.Equals ("C")) {
						aboveTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection > 1) ? gBounds.Bottom : 0), 0);
						aboveState = "B";
					} else {
						aboveTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left * 2 : 0), (((int)controlDirection > 1) ? gBounds.Bottom * 2 : 0), 0);
						aboveState = "A";
					}
				}
			} 
			else 
			{
				if (below != null) {
					
					belowCache = belowState;

					if (belowState.Equals ("B")) {
						belowTarget = Vector3.zero;
						belowState = "C";
					} else if (belowState.Equals ("C")) {
						belowTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection > 1) ? gBounds.Top : 0), 0);
						belowState = "A";
					} else {
						belowTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right * 2 : 0), (((int)controlDirection > 1) ? gBounds.Top * 2 : 0), 0);
						belowState = "B";
					}
				}

				if (center != null) {

					centerCache = centerState;

					if (centerState.Equals ("B")) {
						centerTarget = Vector3.zero;
						centerState = "C";
					} else if (centerState.Equals ("C")) {
						centerTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection > 1) ? gBounds.Top : 0), 0);
						centerState = "A";
					} else {
						centerTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right * 2 : 0), (((int)controlDirection > 1) ? gBounds.Top * 2 : 0), 0);
						centerState = "B";
					}
				}

				if (above != null) {

					aboveCache = aboveState;

					if (aboveState.Equals ("B")) {
						aboveTarget = Vector3.zero;
						aboveState = "C";
					} else if (aboveState.Equals ("C")) {
						aboveTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection > 1) ? gBounds.Top : 0), 0);
						aboveState = "A";
					} else {
						aboveTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right * 2 : 0), (((int)controlDirection > 1) ? gBounds.Top * 2 : 0), 0);
						aboveState = "B";
					}
				}

			}
		}

		runningTime = 0.0f;
		percentage = 0.0f;

		attributes.CurrentPosition = 0.0f;

		SetOrigin();

		released = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (released || freeze)
			return;
		
		if (containDragableArea && !eventData.hovered.Contains (gameObject)) 
		{
			runningTime = 0.0f;
			percentage = 0.0f;

			attributes.CurrentPosition = 0.0f;

			Return();
			SetOrigin();

			released = true;
			return;
		}

		attributes.CurrentPosition += ((int)controlDirection > 0) ? eventData.delta.x : eventData.delta.y;

		if (below != null) 
		{
			if (belowState.Equals ("B"))
				below.localPosition = new Vector3 ((((int)controlDirection > 0) ? attributes.CurrentPosition - (below.sizeDelta.x + spacing) : 0), (((int)controlDirection < 1) ? attributes.CurrentPosition - (below.sizeDelta.y + spacing) : 0), 0);
			else if(belowState.Equals("C"))
				below.localPosition = new Vector3 (attributes.CurrentPosition, 0, 0);
			else
				below.localPosition = new Vector3 ((((int)controlDirection > 0) ? attributes.CurrentPosition + (below.sizeDelta.x + spacing) : 0), (((int)controlDirection < 1) ? attributes.CurrentPosition + (below.sizeDelta.y + spacing) : 0), 0);
		}

		if (center != null) 
		{
			if(centerState.Equals("C"))
				center.localPosition = new Vector3 (attributes.CurrentPosition, 0, 0);
			else if(centerState.Equals("B"))
				center.localPosition = new Vector3 ((((int)controlDirection > 0) ? attributes.CurrentPosition - (center.sizeDelta.x + spacing) : 0), (((int)controlDirection < 1) ? attributes.CurrentPosition - (center.sizeDelta.y + spacing) : 0), 0);
			else
				center.localPosition = new Vector3 ((((int)controlDirection > 0) ? attributes.CurrentPosition + (center.sizeDelta.x + spacing) : 0), (((int)controlDirection < 1) ? attributes.CurrentPosition + (center.sizeDelta.y + spacing) : 0), 0);
		}

		if (above != null) 
		{
			if(aboveState.Equals("A"))
				above.localPosition = new Vector3 ((((int)controlDirection > 0) ? attributes.CurrentPosition + (above.sizeDelta.x + spacing) : 0), (((int)controlDirection < 1) ? attributes.CurrentPosition + (above.sizeDelta.y + spacing) : 0), 0);
			else if(aboveState.Equals("C"))
				above.localPosition = new Vector3 (attributes.CurrentPosition, 0, 0);
			else
				above.localPosition = new Vector3 ((((int)controlDirection > 0) ? attributes.CurrentPosition - (above.sizeDelta.x + spacing) : 0), (((int)controlDirection < 1) ? attributes.CurrentPosition - (above.sizeDelta.y + spacing) : 0), 0);
		}
	}

    public void Next()
    {
        attributes.CurrentPosition = 10 + releaseThreshold;

        OnPointerUp(null);
    }

    public void Previous()
    {
        attributes.CurrentPosition = -(10 + releaseThreshold);

        OnPointerUp(null);
    }

    public void Jump(int val)
    {
        released = false;
        appendOnJump = true;

        belowState = "B";
        centerState = "C";
        aboveState = "A";

        belowCache = "B";
        centerCache = "C";
        aboveCache = "A";

        attributes.State = "C";
        attributes.CurrentPosition = 0.0f;

        attributes.Index = val;
        attributes.Previous = ((attributes.Index - 1) < 0) ? entries.Count - 1 : attributes.Index - 1;
        attributes.Next = ((attributes.Index + 1) > entries.Count - 1) ? 0 : attributes.Index + 1;


        if (below != null)
        {
            below.GetComponent<GalleryEntry>().Clear();

            below.localPosition = new Vector3((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection < 1) ? gBounds.Bottom : 0), 0);
        }

        if (center != null)
        {
            center.GetComponent<GalleryEntry>().Clear();

            center.localPosition = Vector3.zero;
        }

        if (above != null)
        {
            above.GetComponent<GalleryEntry>().Clear();

            above.localPosition = new Vector3((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection < 1) ? gBounds.Top : 0), 0);
        }

        Publish(0);
    }

	private string Identify()
	{
		return (attributes.CurrentPosition < 0) ? "A" : "B";
	}

	private float Ease(float start, float end, float val)
	{
		end -= start;
		return -end * val * (val - 2) + start;
	}

	private void Return()
	{
		if (below != null) 
		{
			if (belowState.Equals ("C"))
				belowTarget = Vector3.zero;
			else if (belowState.Equals ("B"))
				belowTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection > 1) ? gBounds.Bottom : 0), 0);
			else 
				belowTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection > 1) ? gBounds.Top : 0), 0);
		}

		if (center != null)
		{
			if (centerState.Equals ("C"))
				centerTarget = Vector3.zero;
			else if (centerState.Equals ("B"))
				centerTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection > 1) ? gBounds.Bottom : 0), 0);
			else 
				centerTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection > 1) ? gBounds.Top : 0), 0);
		}

		if (above != null)
		{
			if (aboveState.Equals ("C"))
				aboveTarget = Vector3.zero;
			else if (aboveState.Equals ("B"))
				aboveTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Left : 0), (((int)controlDirection > 1) ? gBounds.Bottom : 0), 0);
			else 
				aboveTarget = new Vector3 ((((int)controlDirection > 0) ? gBounds.Right : 0), (((int)controlDirection > 1) ? gBounds.Top : 0), 0);
		}

		returning = true;
	}

	private void SetOrigin()
	{
		if (below != null)
			belowOrigin = new Vector3 (below.localPosition.x, below.localPosition.y, below.localPosition.z);

		if (center != null)
			centerOrigin = new Vector3 (center.localPosition.x, center.localPosition.y, center.localPosition.z);

		if (above != null)
			aboveOrigin = new Vector3 (above.localPosition.x, above.localPosition.y, above.localPosition.z);
	}
}
