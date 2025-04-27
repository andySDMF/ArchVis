using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Event_ScrollViewZoom))]
public class Event_ScrollViewZoomEditor : UnityEditor.UI.ScrollRectEditor
{
    private Event_ScrollViewZoom script;

    private new void OnEnable()
    {
        base.OnEnable();

        script = (Event_ScrollViewZoom)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        GUILayout.Label("Extensions", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("minZoom"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxZoom"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("zoomSpeed"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("disableInputs"), true);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(script);
    }
}
