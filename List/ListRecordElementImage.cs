using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MWM.DynamicList;

public class ListRecordElementImage : MonoBehaviour, IDynamicRecord
{
	[SerializeField]
	private string id = "";

	[Header("Load Handler")]
	[SerializeField]
	private bool fromMemory = false;

	[SerializeField]
	private MemoryHandler handler = MemoryHandler._WebRequest;

	public string ID { get { return id; } }

	public LRecord Record { get; private set; }

	private RawImage imageScript;

	private void OnDisable()
	{
		if (imageScript != null) Destroy(imageScript.texture);
	}

	public void Apply(string data = "All")
	{
		if (imageScript == null)
			imageScript = GetComponent<RawImage> ();

		if (imageScript != null) 
		{
			if (!fromMemory) 
			{
				imageScript.texture = Resources.Load(data) as Texture2D;
			} 
			else 
			{
				if (handler == MemoryHandler._WebRequest) 
				{
					StartCoroutine(WebRequest(data));
				} 
				else 
				{
					imageScript.texture = LoadFile(data);
				}
			}
		}
	}

	private IEnumerator WebRequest(string data)
	{
		using (UnityWebRequest request = new UnityWebRequest(data)) 
		{
			yield return request.SendWebRequest();

			if (request.isNetworkError || request.isNetworkError) 
			{

			} 
			else 
			{
				imageScript.texture = DownloadHandlerTexture.GetContent(request);
			}
		}
	}

	private Texture2D LoadFile(string data)
	{
		Texture2D tex = null;

		if (File.Exists(data))
		{
			byte[] bytes = File.ReadAllBytes(data);
			tex = new Texture2D(2, 2);

			tex.LoadImage(bytes);
		}

		return tex;
	}

	[System.Serializable]
	private enum MemoryHandler { _WebRequest, _FileLoad }
}
