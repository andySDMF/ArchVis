using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Text;
using Grain.CMS;

public class GrainCMSCategoriesLoader : GrainCMSLoader
{
    [Header("Grain CMS Categories")]
    [SerializeField]
    private string projectID = "";

    [SerializeField]
    private string apiToken = "";

    [SerializeField]
    private bool debug = false;

    [SerializeField]
    private List<CMSTarget> cmsTargets = new List<CMSTarget>();

    private bool initialized = false;

    public override void PerformUpdate()
    {
        if (initialized) return;

        initialized = true;

        Download();
    }

    public void Download()
    {
        StartCoroutine(PerformDownload());
    }

    private IEnumerator PerformDownload()
    {
        GrainCMSUtils.TimeStamp = PlayerPrefs.GetString("TIMESTAMP");

        print(GrainCMSUtils.TimeStamp);

        using (UnityWebRequest request = UnityWebRequest.Get("https://cms.grain.london/api/projects/" + projectID + "/updates?token=" + apiToken + "&ts=" + GrainCMSUtils.TimeStamp))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            string rawData = "";

            if (request.isNetworkError || request.isHttpError)
            {
                if (debug) Debug.Log("CMS request error " + request.responseCode.ToString() + "[" + request.error + "]");

                rawData = "";
            }
            else
            {
                if (debug) Debug.Log("CMS request text [" + request.downloadHandler.text + "]");

                Encoding enc = new UTF8Encoding(true, true);
                byte[] bytes = enc.GetBytes(request.downloadHandler.text);

                rawData = enc.GetString(bytes);
            }

            foreach (CMSTarget target in cmsTargets)
            {
                target.Send(rawData);
            }

            request.Dispose();
        }

        loadingProgress = 100;
        loadingCompleted = true;

        yield return null;
    }
}
