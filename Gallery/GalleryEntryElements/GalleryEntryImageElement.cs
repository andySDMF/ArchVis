using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Grain.Gallery;

[RequireComponent(typeof(RawImage))]
public class GalleryEntryImageElement : GalleryBaseEntryElement, IGalleryElement
{
    [Header("Load Handler")]
	[SerializeField]
	private bool fromMemory = false;

    [SerializeField]
    private string tempDirectory = "";

    [SerializeField]
	private MemoryHandler handler = MemoryHandler._WebRequest;

	public string Reference { get { return reference; } }

	private RawImage imageScript;

	private void OnDisable()
	{
		if (imageScript != null) 
		{
			if (fromMemory)
				Destroy (imageScript.texture);
			else
				imageScript.texture = null;
		}
	}
	
	public void Recieve (string data)
	{
        if (string.IsNullOrEmpty(data))
			return;

		if (imageScript == null)
			imageScript = GetComponent<RawImage> ();

        Clear();

        if (imageScript != null) 
		{
			if (!fromMemory) 
			{
				imageScript.texture = Resources.Load(data) as Texture2D;

                if(imageScript.texture == null) imageScript.texture = Resources.Load(tempDirectory) as Texture2D;

                imageScript.CrossFadeAlpha(1.0f, 0.5f, true);
            } 
			else 
			{
				if (handler == MemoryHandler._WebRequest) 
				{
                    StartCoroutine(WebRequest(data));
				}
                else if(handler == MemoryHandler._WWW)
                {
                    StartCoroutine(WWWRequest(data));
                }
				else 
				{
					imageScript.texture = LoadFile(data);

                    if (imageScript.texture == null) imageScript.texture = Resources.Load(tempDirectory) as Texture2D;

                    imageScript.CrossFadeAlpha(1.0f, 0.5f, true);
                }
			}
		}
	}

	public void Clear()
	{
        if (imageScript == null)
            imageScript = GetComponent<RawImage>();

        imageScript.CrossFadeAlpha(0.0f, 0.0f, true);

        if (fromMemory)
			Destroy (imageScript.texture);
		else
			imageScript.texture = null;
	}

	private IEnumerator WebRequest(string data)
	{
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(data, true);

        yield return request.SendWebRequest();

        if(!request.isHttpError || !request.isNetworkError)
        {
            imageScript.texture = DownloadHandlerTexture.GetContent(request);
        }

        request.Dispose();

        if (imageScript.texture == null) imageScript.texture = Resources.Load(tempDirectory) as Texture2D;

        imageScript.CrossFadeAlpha(1.0f, 0.5f, true);
    }

    private IEnumerator WWWRequest(string data)
    {
        WWW request = new WWW(data);

        yield return request;

        Texture2D tex = new Texture2D(2048, 2048, TextureFormat.ARGB32, false);

        request.LoadImageIntoTexture(tex);
        imageScript.texture = tex;

        request.Dispose();

        if (imageScript.texture == null) imageScript.texture = Resources.Load(tempDirectory) as Texture2D;

        imageScript.CrossFadeAlpha(1.0f, 0.5f, true);
    }

	private Texture2D LoadFile(string data)
	{
		Texture2D tex = null;

        if (data.Contains("file://"))
        {
            data = data.Replace("file://", "");
        }

        if (File.Exists(data))
		{
			byte[] bytes = File.ReadAllBytes(data);
			tex = new Texture2D(2, 2);

			tex.LoadImage(bytes);
		}

		return tex;
	}

	[System.Serializable]
	private enum MemoryHandler { _WebRequest, _FileLoad, _WWW }
}
