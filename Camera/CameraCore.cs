using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grain.Camera
{
    [System.Serializable]
    public enum CameraControlFingers
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4
    }

    public interface ICameraTarget
    {
        void OnClick();
    }
}

public static class CameraUtils
{
    public static void Switch(Camera cam1, Camera cam2)
    {
        if (cam1 != null) cam1.enabled = false;

        if (cam2 != null) cam2.enabled = true;
    }
}