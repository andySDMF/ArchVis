using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GalleryDesigner))]
[CanEditMultipleObjects]
public class GalleryDesignerEditor : Editor 
{
	private GalleryDesigner script;

	private Vector2 current;

	private void OnEnable()
	{
		script = (GalleryDesigner)target;

		current = serializedObject.FindProperty ("size").vector2Value;
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("entryPrefab"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("entryContainer"), true);

		EditorGUILayout.PropertyField(serializedObject.FindProperty("spacing"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("size"), true);

		EditorGUILayout.Space ();

		EditorGUILayout.BeginHorizontal ();

		if (GUILayout.Button ("Append")) 
		{
			script.Append ();
		}

		if (GUILayout.Button ("Delete")) 
		{
			script.Delete ();
		}

		EditorGUILayout.EndHorizontal ();

		serializedObject.ApplyModifiedProperties ();

		EditorUtility.SetDirty (script);
	}
}
