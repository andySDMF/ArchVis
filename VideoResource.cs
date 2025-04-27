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
public class VideoResource : MonoBehaviour, IVideo, IVideoAsset
{
    [SerializeField]
    private UnityEvent onLoadComplete = new UnityEvent();

    [SerializeField]
    private string videoResource = "";

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

    private void Awake()
    {
        if(Image == null) Image = GetComponent<RawImage>();
        if (player == null) player = GetComponent<VideoPlayer>();
        if (audio == null) audio = GetComponent<AudioSource>();

        player.Stop();
        audio.Stop();

        player.playOnAwake = false;
        audio.playOnAwake = false;

        player.source = VideoSource.VideoClip;

        player.EnableAudioTrack(9, true);
        player.SetTargetAudioSource(0, audio);
    }

    private void OnEnable()
    {
        if (on) Load(videoResource);
    }

    private void OnDisable()
    {
        if (on) Unload();
    }

    public void Append(string str)
    {
        videoResource = str;
    }

    public void Load(string data)
    {
        if (debugOn) Debug.Log("Loading image to [" + gameObject.name + "], path : " + data + "/" + data);

        if (Image == null) Image = GetComponent<RawImage>();
        if (player == null) player = GetComponent<VideoPlayer>();
        if (audio == null) audio = GetComponent<AudioSource>();

        player.clip = Resources.Load(data) as VideoClip;
        current = data;

        StartCoroutine(Prepare());

        HasLoaded = true;
    }

    public void Load()
    {
        Load(videoResource);
    }

    public void Unload()
    {
        player.Stop();
        audio.Stop();

        HasLoaded = false;
        Image.texture = null;
        player.clip = null;

        Resources.UnloadUnusedAssets();
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
        player.SetDirectAudioVolume(0, audio.volume);


        if (OnLoadEvent != null)
        {
            OnLoadEvent.Invoke();
        }

        onLoadComplete.Invoke();

        player.Play();
        audio.Play();
    }
}
