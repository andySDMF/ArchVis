using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrainCMSUnitPropertiesCache))]
public class GrainCMSUnitPropertiesCacheEditor : Editor
{
    private GrainCMSUnitPropertiesCache script;

    public void OnEnable()
    {
        script = (GrainCMSUnitPropertiesCache)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("debug"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("derivedFromUnits"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultAllValue"), true);

        EditorGUILayout.Space();

        GUILayout.Label("Grain Unit Properties", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Property Cache"))
        {
            PlayerPrefs.SetString("UNIT-PROPERTIES", "");
            script.PropertyFields.Clear();
        }

        if (GUILayout.Button("Get Property Cache"))
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("UNIT-PROPERTIES")))
            {
                Debug.Log("No properties exist in player prefs. Run the app to cache properties");
            }
            else script.RecieveCMSData(PlayerPrefs.GetString("UNIT-PROPERTIES"));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("propertyFields"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cache"), true);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(script);
    }
}
