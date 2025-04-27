using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.UI;
using MWM.Video;
using TMPro;

public class VideoControl : MonoBehaviour, IVideoPlayer
{
	[SerializeField]
	private VideoPlayer player;

	[SerializeField]
	private new AudioSource audio;

    [SerializeField]
    private bool seekEnabled = false;

    [Header("UI Assets")]
	[SerializeField]
	private Slider seekSlider;

	[SerializeField]
	private MaskableGraphic durationText;

    [SerializeField]
    private MaskableGraphic lengthText;

    [Header("Events")]
	[SerializeField]
	private UnityEvent onPlay = new UnityEvent();

	[SerializeField]
	private UnityEvent onStop = new UnityEvent();

	[Header("Debug Mode")]
	[SerializeField]
	private bool debugOn = false;


	public bool IsPlaying { get; private set; }

	public bool IsPaused { get; private set; }

	private bool State { get { return player != null; } }

	public System.TimeSpan currentTime { get; private set; }
	public System.TimeSpan durationTime { get; private set; }
    private float elapsedTime = 0.0f;

	private string cacheURL;
    private IVideoAsset videoObject;

    private bool hasInit = false;

    private void Awake()
	{
        if (hasInit) return;

		if (State) 
		{
            hasInit = true;

			player.loopPointReached += OnStopCallback;

            videoObject = player.GetComponent<IVideoAsset>();

            if (videoObject != null) 
			{
                videoObject.OnLoadEvent += Flush;
                videoObject.OnLoadEvent += Play;
			}
		}

        if (seekSlider != null) seekSlider.interactable = seekEnabled;
	}

	private void Update()
	{
		if (!IsPlaying) return;

		elapsedTime += Time.deltaTime;
		currentTime = System.TimeSpan.FromSeconds (System.Math.Round (elapsedTime, 0));

		if(currentTime <= durationTime)
		{
            if (durationText != null)
            {
                if(durationText is TextMeshProUGUI)
                {
                    ((TextMeshProUGUI)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
                }
                else
                {
                    ((Text)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
                }
            }

		}

		if (seekSlider != null) 
		{
			seekSlider.value = player.frame;
		}
	}

	public void Play()
	{
        if (!State)
        {
            Debug.Log("fvweagv");

            return;
        }

        Awake();

        cacheURL = (videoObject.Image.texture == null) ? "" : player.url;

		if (string.IsNullOrEmpty(cacheURL) && player.clip == null)
			return;

		if ((int)player.frame >= (int)player.frameCount) 
		{
			player.frame = 0;
			if (audio != null) audio.time = 0.0f;
		}

		IsPaused = false;
		IsPlaying = true;

		if (videoObject != null) videoObject.Image.texture = player.texture;

		player.Play ();
		if (audio != null) audio.Play ();

		onPlay.Invoke ();
	}

	public void Play(string url)
	{
        if (!State)
        {
            Debug.Log("fvweagv");

            return;
        }

        Awake();

        player.source = VideoSource.Url;

		cacheURL = url;

		if (!string.IsNullOrEmpty(cacheURL) && player.url.Equals(cacheURL)) 
		{
			player.frame = 0;
			if (audio != null) audio.time = 0.0f;

			Flush ();

			player.Play();
			if (audio != null) audio.Play();

			return;
		}

		player.url = cacheURL;

		StartCoroutine(Prepare());
	}

	public void Play(VideoClip clip)
	{
        if (!State)
        {
            Debug.Log("fvweagv");

            return;
        }

		player.source = VideoSource.VideoClip;

		if (player.clip != null && player.clip.Equals (clip)) 
		{
			player.frame = 0;
			if (audio != null) audio.time = 0.0f;

			Flush ();

			player.Play();
			if (audio != null) audio.Play();

			return;
		}

		player.clip = clip;

		StartCoroutine(Prepare());
	}

	public void Stop()
	{
		if (!State) return;

		if (string.IsNullOrEmpty(player.url) && player.clip == null)
			return;

		IsPaused = false;
		IsPlaying = false;

		player.Stop();
		if (audio != null) audio.Stop();

		cacheURL = "";
		player.clip = null;

		elapsedTime = 0.0f;
		durationTime = System.TimeSpan.FromSeconds (System.Math.Round (0.0f, 0));

		if (seekSlider != null) 
		{
			seekSlider.wholeNumbers = false;
			seekSlider.minValue = 0;
			seekSlider.maxValue = player.frameCount;
		}

		currentTime = System.TimeSpan.FromSeconds (System.Math.Round (elapsedTime, 0));

        if (durationText != null)
        {
            if (durationText is TextMeshProUGUI)
            {
                ((TextMeshProUGUI)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
            }
            else
            {
                ((Text)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
            }
        }

        onStop.Invoke ();
	}

	public void Pause(bool pause)
	{
		if (!State) return;

		if (string.IsNullOrEmpty(player.url) && player.clip == null)
			return;

		if (pause) 
		{
			player.Pause ();
			if (audio != null) audio.Pause ();
		} 
		else 
		{
			player.Play ();
			if (audio != null) audio.UnPause ();
		}

		IsPaused = pause;
		IsPlaying = !IsPaused;
	}

	public void Scrub(float val)
	{
		if (!State) return;

        if (!seekEnabled) return;

        player.frame = (long)val;
        audio.time = val; 

        elapsedTime = (float)player.time;
        currentTime = System.TimeSpan.FromSeconds(System.Math.Round(elapsedTime, 0));

        if (currentTime <= durationTime)
        {
            if (durationText != null)
            {
                if (durationText is TextMeshProUGUI)
                {
                    ((TextMeshProUGUI)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
                }
                else
                {
                    ((Text)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
                }
            }
        }
    }

	private IEnumerator Prepare()
	{
		player.isLooping = false;
		if (audio != null) audio.loop = false;

		player.Prepare();

		WaitForSeconds wait = new WaitForSeconds(0.1f);

		while (!player.isPrepared) 
		{
			yield return wait;
		}
			
		Flush ();

		IsPaused = false;
		IsPlaying = true;

		if (videoObject != null) videoObject.Image.texture = player.texture;

		player.Play();
		if (audio != null) audio.Play();

		onPlay.Invoke ();
	}

	private void Flush()
	{
		elapsedTime = 0.0f;

        if (player.source == VideoSource.VideoClip)
        {
            if (player.clip == null)
            {
                durationTime = System.TimeSpan.FromSeconds(System.Math.Round(0.0f, 0));
            }
            else durationTime = System.TimeSpan.FromSeconds(System.Math.Round(player.clip.length, 0));
        }
        else
        {
            if (player.GetComponent<VideoObject>().Image.texture == null)
            {
                durationTime = System.TimeSpan.FromSeconds(System.Math.Round(0.0f, 0));
            }
            else durationTime = System.TimeSpan.FromSeconds(System.Math.Round(player.frameCount / player.frameRate, 0));
        }

		if (seekSlider != null) 
		{
			seekSlider.wholeNumbers = false;
			seekSlider.minValue = 0;
			seekSlider.maxValue = player.frameCount;
		}

		currentTime = System.TimeSpan.FromSeconds (System.Math.Round (elapsedTime, 0));

        if(lengthText != null)
        {
            if (lengthText is TextMeshProUGUI)
            {
                ((TextMeshProUGUI)lengthText).text = durationTime.ToString();
            }
            else
            {
                ((Text)lengthText).text = durationTime.ToString();
            }
        }

        if (durationText != null)
        {
            if (durationText is TextMeshProUGUI)
            {
                ((TextMeshProUGUI)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
            }
            else
            {
                ((Text)durationText).text = currentTime.ToString() + " / " + durationTime.ToString();
            }
        }
    }

	private void OnStopCallback(VideoPlayer source)
	{
		IsPaused = false;
		IsPlaying = false;
	}
}
