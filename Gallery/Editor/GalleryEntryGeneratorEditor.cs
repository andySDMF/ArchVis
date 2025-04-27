using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GalleryEntryGenerator))]
[CanEditMultipleObjects]
public class GalleryEntryGeneratorEditor : Editor
{
    private GalleryEntryGenerator script;

    private void OnEnable()
    {
        script = (GalleryEntryGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("gallery"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("useElementsStructure"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("elementsStructure"), true);

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Editor"))
        {
            //opens editor window and sends this target script
            GalleryEntryGeneratorWindow.Init(script);
        }

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(script);
    }
}
