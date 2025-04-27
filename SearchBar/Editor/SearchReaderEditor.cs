using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SearchReader))]
public class SearchReaderEditor : Editor
{
    private SearchReader script;

    private void OnEnable()
    {
        script = (SearchReader)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("asset"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("excludeHeader"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fileType"), true);
    
        EditorGUILayout.Space();

        GUILayout.Label("Instantiate Elements", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("associatedSearchBar"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("container"), true);

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Publish"))
        {
            script.Publish();
        }

        if(GUILayout.Button("Clear"))
        {
            script.Clear();
        }

        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(script);
    }
}
