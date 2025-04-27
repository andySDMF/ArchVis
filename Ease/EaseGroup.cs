using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Ease;

public class EaseGroup : MonoBehaviour, IEase
{
	public bool IsEasing { get; private set; }
    public bool State
    {
        get
        {
            return listeners.TrueForAll(l => l.State);
        }
    }

    public string Name { get; private set; }

	private List<IEase> listeners = new List<IEase>();

	public List<IEase> Listeners { get { return listeners; } }

	private void Awake()
	{
		Name = gameObject.name;
	}

    private void Update()
    {
        if (IsEasing)
        {
            IsEasing = !listeners.TrueForAll(l => l.IsEasing == false);
        }
    }

    public void EaseIn()
	{
        IsEasing = true;

        listeners.ForEach(l => l.EaseIn());
	}

    public void EaseIn(float interval)
    {
        IsEasing = true;

        StopCoroutine("EaseInterval");
        StartCoroutine(EaseInterval(true, interval));
    }

    public void EaseOut()
	{
        IsEasing = true;

        listeners.ForEach(l => l.EaseOut());
	}

    public void EaseOut(float interval)
    {
        IsEasing = true;

        StopCoroutine("EaseInterval");
        StartCoroutine(EaseInterval(false, interval));
    }

    private IEnumerator EaseInterval(bool state, float interval)
    {
        if(state)
        {
            for(int i = 0; i < listeners.Count; i++)
            {
                listeners[i].EaseIn();

                yield return new WaitForSeconds(interval);
            }
        }
        else
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].EaseOut();

                yield return new WaitForSeconds(interval);
            }
        }
    }

    public void Jump(bool state)
    {
        listeners.ForEach(l => l.Jump(state));
    }

	public void Toggle(bool state)
	{
		listeners.ForEach(l => l.Toggle(state));
	}

	public void Fire(List<string> keys, int state)
	{
		if (state <= 0)
			state = 0;
		else
			state = 1;

        if(keys.Count > 0)
        {
            IsEasing = true;

            for (int i = 0; i < Listeners.Count; i++)
            {
                if (keys.Contains(listeners[i].Name))
                {
                    if (state.Equals(0)) listeners[i].EaseIn();
                    else listeners[i].EaseOut();
                }
            }
        }
	}

	public void FireJson(string json)
	{
		JsonCommand comms = JsonUtility.FromJson<JsonCommand> (json);

		if (comms == null)
			return;

		Fire(comms.keys, comms.state);
	}

	[System.Serializable]
	private class JsonCommand
	{
		public List<string> keys;
		public int state;
	}
}
