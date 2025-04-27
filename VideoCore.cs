using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using MWM.Video;

namespace MWM.Video
{
	[System.Serializable]
	public enum PathType { _PersistantDataPath, _DataPath}

	public interface IVideo
	{
		bool HasLoaded { get; }

		void Load(string data);

		void Unload();
	}

    public interface IVideoAsset
    {
        RawImage Image { get; set; }

        System.Action OnLoadEvent { get; set; }
    }

	public interface IVideoResource
	{
		string Get(string reference);

		void Find(string json);

		string Random();
	}

	public interface IVideoPlayer
	{
		bool IsPlaying { get; }

		bool IsPaused { get; }

		void Play();

		void Play(string url);

		void Play(VideoClip clip);

		void Stop();

		void Pause(bool pause);

		void Scrub(float val);
	}
}

public static class VideoUtils
{
	public static Dictionary<string, IVideoResource> Interfaces { get; private set; }

	public static string Add(string id, IVideoResource i)
	{
		if (Interfaces == null) Interfaces = new Dictionary<string, IVideoResource>();

		if (!Interfaces.ContainsKey(id))
		{
			Interfaces.Add(id, i);
		}

		return id;
	}

	public static void BatchLoad(string directory, ref List<string> output, PathType type)
	{
		string destination = ((int)type > 0) ? Application.dataPath : Application.persistentDataPath;

		if(Directory.Exists(destination + "/" + directory))
		{
			string[] paths = Directory.GetFiles(destination + "/" + directory);

			if (paths.Length > 0) 
			{
				for(int i = 0; i < paths.Length; i++)
				{
					if (paths[i].Contains (".meta")) continue;

					if(!output.Contains(paths[i])) output.Add(paths [i]);
				}
			}
		}
	}

	public static bool Check(string current, string next)
	{
		return current.Equals(next);
	}
}
