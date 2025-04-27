using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(ScrollViewDisplay))]
public class ScrollViewDisplayEditor : UnityEditor.UI.HorizontalOrVerticalLayoutGroupEditor
{
    private ScrollViewDisplay script;

    private ContentSizeFitter contentSizeFillter;
    private RectTransform rectT;
    private HorizontalLayoutGroup layout;

    private new void OnEnable()
    {
        base.OnEnable();

        script = (ScrollViewDisplay)target;

        contentSizeFillter = script.GetComponent<ContentSizeFitter>();
        rectT = script.GetComponent<RectTransform>();
        layout = script.GetComponent<HorizontalLayoutGroup>();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Scroll View", EditorStyles.boldLabel);

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("globalDuration"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("useConstantIndexOnAppend"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("constantIndex"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("appender"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemFullSize"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemReducedSize"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("setActiveCurrent"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("onMoveBegin"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("onMoveEnd"), true);

        if(rectT != null)
        {
            rectT.anchorMin = new Vector2(0, 0.5f);
            rectT.anchorMax = new Vector2(0, 0.5f);
            rectT.pivot = new Vector2(0, 0.5f);
        }

        if(layout != null)
        {
            layout.childAlignment = TextAnchor.MiddleLeft;
        }

        if(contentSizeFillter != null)
        {
            contentSizeFillter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFillter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(this);
    }
}

