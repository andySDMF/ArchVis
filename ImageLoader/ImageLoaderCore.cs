using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Grain.ImageLoader
{
    public enum RequestType { _WWW, _WebRequest, _FileLoad }

    public interface IImageLoader
    {
        void Append(string data);

        void Load();

        void Unload();
    }
}

public static class ImageLoaderUtils
{
    public static IEnumerator WWWRequest(string data, RawImage returned, bool setNativeSize = true, bool fade = true)
    {
        WWW request = new WWW(data);

        yield return request;

        Texture2D tex = new Texture2D(2048, 2048, TextureFormat.ARGB32, false);

        request.LoadImageIntoTexture(tex);
        returned.texture = tex;

        request.Dispose();

        if (setNativeSize) returned.SetNativeSize();

        if (fade) returned.CrossFadeAlpha(1.0f, 0.5f, true);
    }

    public static IEnumerator WebRequest(string data, RawImage returned, bool setNativeSize = true, bool fade = true)
    {
        if (string.IsNullOrEmpty(data)) yield break;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(data, true);

        yield return request.SendWebRequest();

        if (!request.isHttpError || !request.isNetworkError)
        {
            returned.texture = DownloadHandlerTexture.GetContent(request);
        }

        request.Dispose();

        if (setNativeSize) returned.SetNativeSize();

        if (fade) returned.CrossFadeAlpha(1.0f, 0.5f, true);
    }

    public static Texture2D LoadFile(string data)
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

    public static bool IsCulled(RectTransform elem)
    {
        Vector3[] v = new Vector3[4];
        elem.GetWorldCorners(v);

        float maxY = Mathf.Max(v[0].y, v[1].y, v[2].y, v[3].y);
        float minY = Mathf.Min(v[0].y, v[1].y, v[2].y, v[3].y);

        float maxX = Mathf.Max(v[0].x, v[1].x, v[2].x, v[3].x);
        float minX = Mathf.Min(v[0].x, v[1].x, v[2].x, v[3].x);

        if (maxY < 0 || minY > Screen.height || maxX < 0 || minX > Screen.width)
        {
            return false;
        }

        return true;
    }
}
