using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrainCMSCategoriesCache))]
public class GrainCMSCategoriesCacheEditor : Editor
{
    private GrainCMSCategoriesCache script;

    public void OnEnable()
    {
        script = (GrainCMSCategoriesCache)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("debug"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("updateType"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("notificationPrefab"), true);

        EditorGUILayout.Space();

        GUILayout.Label("Events", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("onComplete"), true);

        EditorGUILayout.Space();

        GUILayout.Label("Grain Categories", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Category Cache"))
        {
            PlayerPrefs.SetString("CATEGORIES", "");
            PlayerPrefs.SetString("TIMESTAMP", "");
            script.Categories.Clear();
        }

        if (GUILayout.Button("Get Category Cache"))
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("CATEGORIES")))
            {
                Debug.Log("No categories exist in player prefs. Run the app to cache categories");
            }
            else script.RecieveCMSData(PlayerPrefs.GetString("CATEGORIES"));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("categories"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cache"), true);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(script);
    }
}
