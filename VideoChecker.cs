using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWM.Video;

public class VideoChecker : MonoBehaviour
{
    [SerializeField]
    private GameObject videoObject;

    [SerializeField]
    private string videoReference = "";

    [SerializeField]
    private bool isInternalResource = false;

    private IVideo video;

    public bool Check(bool isPlaying)
    {
        if(video == null) video = videoObject.GetComponent<IVideo>();

        if(videoObject != null)
        {
            return video.HasLoaded.Equals(isPlaying);
        }

        return false;
    }

    public void CheckThenPerform(bool isPlaying)
    {
        if (video == null) video = videoObject.GetComponent<IVideo>();

        if (videoObject != null)
        {
            if(video.HasLoaded.Equals(isPlaying))
            {
                if(isPlaying)
                {
                    video.Unload();
                }
                else
                {
                    string path = "";

                    if (!isInternalResource)
                    {
                        path = (Application.isEditor) ? Application.dataPath.Substring(0, Application.dataPath.Length - 6) : Application.dataPath.Substring(0, Application.dataPath.Length - 10);
                        path += "/" + videoReference;
                    }
                    else
                    {
                        path = videoReference;
                    }

                    video.Load(path);
                }
            }
        }
    }
}
