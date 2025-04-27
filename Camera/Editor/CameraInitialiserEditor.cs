using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraInitialiser))]
public class CameraInitialiserEditor : Editor
{
    private CameraInitialiser script;
    private Camera cam;

    private void OnEnable()
    {
        script = (CameraInitialiser)target;

        cam = script.GetComponent<Camera>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("onInitComplete"), true);

        if(cam != null)
        {
            if(cam.enabled)
            {
                cam.enabled = false;
            }
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(script);
    }
}
