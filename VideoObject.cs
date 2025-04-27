using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.UI;
using MWM.Video;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(AudioSource))]
public class VideoObject : MonoBehaviour, IVideo, IVideoAsset
{
	[SerializeField]
	private UnityEvent onLoadComplete = new UnityEvent();

	[Header("Enable & Disable")]
	[SerializeField]
	private bool on = false;

	[Header("Debug Mode")]
	[SerializeField]
	private bool debugOn = false;


	public bool HasLoaded { get; private set; }

	public RawImage Image { get; set; }

	public System.Action OnLoadEvent { get; set; }

	private VideoPlayer player;
	private new AudioSource audio;

	private string current = "";
	private string cache = "";

	private void Awake()
	{
		Image = GetComponent<RawImage>();
		player = GetComponent<VideoPlayer>();
		audio = GetComponent<AudioSource>();

		player.Stop();
		audio.Stop();

		player.playOnAwake = false;
		audio.playOnAwake = false;

		player.source = VideoSource.Url;

		player.EnableAudioTrack(9, true);
		player.SetTargetAudioSource(0, audio);
	}

	private void OnEnable()
	{
		if (on) Load(cache);
	}

	private void OnDisable()
	{
		if (on) Unload();
	}

	public void Load(string data)
	{
        if (data.Contains("file://"))
        {
            data = data.Replace("file://", "");
        }

        cache = data;

		if (debugOn) Debug.Log ("Loading image to [" + gameObject.name + "], path : " + cache + "/" + cache);

        player.url = cache;
		current = cache;

		StartCoroutine(Prepare());

		HasLoaded = true;
	}

	public void Unload()
	{
		player.Stop();
		audio.Stop();

        player.clip = null;
        Image.texture = null;


        HasLoaded = false;

        Resources.UnloadUnusedAssets();
	}

	public void Override(string path)
	{
		player.source = VideoSource.Url;

        Load(path);
	}

	private IEnumerator Prepare()
	{
		player.Prepare();

		WaitForSeconds wait = new WaitForSeconds(0.1f);

		while (!player.isPrepared) 
		{
			yield return wait;
		}

		Image.texture = player.texture;

        if (OnLoadEvent != null)
        {
            OnLoadEvent.Invoke();
        }

        onLoadComplete.Invoke();

        player.Play();
		audio.Play();
	}
}
