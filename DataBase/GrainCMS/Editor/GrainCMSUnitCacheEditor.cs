using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrainCMSUnitCache))]
public class GrainCMSUnitCacheEditor : Editor
{
    private GrainCMSUnitCache script;

    public void OnEnable()
    {
        script = (GrainCMSUnitCache)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("debug"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditions"), true);

        EditorGUILayout.Space();

        GUILayout.Label("Grain Units", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Unit Cache"))
        {
            PlayerPrefs.SetString("UNITS", "");
            script.Units.Clear();
            script.Clear();

            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Get Unit Cache"))
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("UNITS")))
            {
                Debug.Log("No units exist in player prefs. Run the app to cache units");
            }
            else
            {
                if(script.CacheObject == null)
                {
                    script.RecieveCMSData(PlayerPrefs.GetString("UNITS"));
                }
                else
                {
                    PlayerPrefs.SetString("UNITS", script.CacheObject.rawPropertiesData);
                    script.RecieveCMSData(script.CacheObject.rawPropertiesData);
                }
            }

            EditorUtility.SetDirty(script);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("units"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cache"), true);

        serializedObject.ApplyModifiedProperties();

        //EditorUtility.SetDirty(script);
    }
}
