using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(MapMarkerVisualiser))]
public class MapMarkerVisualiserEditor : Editor
{
    private MapMarkerVisualiser script;

    private void OnEnable()
    {
        script = (MapMarkerVisualiser)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("markerContainer"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("alignment"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onMarkerSelect"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("markerUILookup"), true);

        GUILayout.Label("Journeys", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("journeyContainer"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("originReference"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("journeyColor"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("journeySpeed"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("setExtents"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tooltip"), true);

        GUILayout.Label("Extents Tool", EditorStyles.boldLabel);

        if (GUILayout.Button("Get Journey Extents"))
        {
            if(!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Warning!", "Action must be performed at runtime!", "OK");
                return;
            }

            if(script.MapAssigned)
            {
                if(script.Current != null)
                {
                    MapUtils.Maps[script.MapName].MapCache.AddJourneyExtent(script.Current.ID, script.TravelMode, script.transform.localPosition, MapUtils.Maps[script.MapName].ZoomStep);
                }
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Debug Mode", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugOn"), true);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(script);
    }
}
