using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineParticleFollower : MonoBehaviour
{
    [SerializeField]
    private List<ParticleSystem> systemScripts;

    [SerializeField]
    private LineObject lineScript;

    private void Awake()
    {
        lineScript.AddDelegate(Follow);
        lineScript.AddColorSetter(Set);
    }

    public void Set(Color color)
    {
        foreach(ParticleSystem pSystem in systemScripts)
        {
            var main = pSystem.main;
            main.startColor = color;
        }
    }

    public void Follow(Vector3 pos)
    {
        foreach (ParticleSystem pSystem in systemScripts)
        {
            if (!pSystem.isPlaying)
            {
                pSystem.Play();
            }

            if (pSystem.time >= 1.0f)
            {
                pSystem.Pause();
                pSystem.time = 0.99f;
            }
        }

        transform.position = pos;
    }

    public void Stop()
    {
        foreach (ParticleSystem pSystem in systemScripts)
        {
            if (pSystem.isPlaying)
            {
                pSystem.Stop();
            }
        }
    }
}
