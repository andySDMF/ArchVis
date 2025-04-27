using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Text;

public class GrainCMSUnitLoader : GrainCMSLoader
{
    [Header("Grain CMS Units")]
    [SerializeField]
    private string projectID = "";

    [SerializeField]
    private string apiToken = "";

    [SerializeField]
    private bool debug = false;

    [SerializeField]
    private List<CMSTarget> cmsTargets = new List<CMSTarget>();

    public override void PerformUpdate()
    {
        Download();
    }

    public void Download()
    {
        StartCoroutine(PerformDownload());
    }

    private IEnumerator PerformDownload()
    {
        using (UnityWebRequest request = UnityWebRequest.Get("https://cms.grain.london/api/projects/" + projectID + "/plots?token=" + apiToken))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            string rawData = "";

            if(request.isNetworkError || request.isHttpError)
            {
                if(debug) Debug.Log("CMS request error " + request.responseCode.ToString() + "[" + request.error + "]");

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
