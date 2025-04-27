using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using Grain.Map;
using Grain.Map.GoogleAPI;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    private Map script;

    private RectTransform rectT;
    private List<ZoomStep> steps;
    private float interval;
    private bool stepFoldout;
    private ZoomStep activeStep;

    private bool downloadingSegments;
    private float progress;
    private float total;

    private void OnEnable()
    {
        script = (Map)target;

        if (script.Display != null) rectT = script.Display.GetComponent<RectTransform>();

        interval = script.ConvertZoomInterval;
        steps = GetSteps();

        downloadingSegments = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseImage"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cam"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("processOnStart"), true);

        GUILayout.Label("Markers", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("markerContainer"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("markerVisualiser"), true);

        GUILayout.Label("Tiles", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tiles"), true);

        GUILayout.Label("Download Tools", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("directory"), true);

        stepFoldout = EditorGUILayout.Foldout(stepFoldout, "Zoom Intervals");

        if(stepFoldout)
        {
            EditorGUILayout.BeginVertical();

            if (steps.Count - 1 != script.ConvertZoomDistance)
            {
                steps = GetSteps();
            }

            if (interval != script.ConvertZoomInterval)
            {
                interval = script.ConvertZoomInterval;
                steps = GetSteps();
            }

            for(int i = 0; i < steps.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Step " + steps[i].step.ToString(), GUILayout.MaxWidth(100));

                if(activeStep.Equals(steps[i]))
                {
                    steps[i].active = true;
                }

                steps[i].active = EditorGUILayout.Toggle(steps[i].active);

                if (steps[i].active)
                {
                    script.ZoomLevel = steps[i].step;

                    if(activeStep.Equals(steps[i]))
                    {
                        activeStep.active = true;
                    }
                    else
                    {
                        activeStep.active = false;
                        activeStep = steps[i];
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.HelpBox("Map Type is " + script.Type, MessageType.Info, true);

        if (rectT != null) rectT.sizeDelta = script.Size;

        EditorGUILayout.Space();

        if (GUILayout.Button("Download Base Map"))
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Warning!", "Cannot perform action at runtime!", "OK");
                return;
            }

            script.DownloadBaseTexture();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Create Map Type Segment"))
        {
            if (script.Downloading) return;

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Warning!", "Cannot perform action at runtime!", "OK");
                return;
            }

            progress = 0.0f;
            total = (float)steps.Count;

            downloadingSegments = true;

            CreateSegment();
        }

        if (GUILayout.Button("Clear Map Type Segment"))
        {
            if (script.Downloading) return;

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Warning!", "Cannot perform action at runtime!", "OK");
                return;
            }

            string[] fileEntries = Directory.GetFiles(Application.dataPath + script.Directory + "/" + script.Type);

            for (int j = 0; j < fileEntries.Length; j++)
            {
                string[] split = fileEntries[j].Split('/');
                string[] file = split[split.Length - 1].Substring(script.Type.ToString().Length + 1).Split('_');

                if(file[0].Equals(script.ZoomLevel.ToString())) File.Delete(fileEntries[j]);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Create All Map Type Segments"))
        {
            if (script.Downloading) return;

            if(EditorUtility.DisplayDialog("Warning!", "Are you sure you want to create all segments for map type [" + script.Type + "]? This may take a few minutes.", "Create", "Cancel"))
            {
                progress = 0.0f;
                total = (float)steps.Count;

                downloadingSegments = true;

                script.StartCoroutine(DownloadSegments());
            }
        }

        if (GUILayout.Button("Clear All Map Type Segments"))
        {
            if (script.Downloading) return;

            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to clear all segments for map type [" + script.Type + "]? This may take a few minutes.", "Clear", "Cancel"))
            {
                string[] fileEntries = Directory.GetFiles(Application.dataPath + script.Directory + "/" + script.Type);

                for (int j = 0; j < fileEntries.Length; j++)
                {
                    File.Delete(fileEntries[j]);
                }
            }
        }

        if(downloadingSegments)
        {
            EditorUtility.DisplayProgressBar("Downloading " + script.Type + " segments", "Zoom level.... " + script.ZoomLevel.ToString(), progress / total);
        }
        else
        {
            EditorUtility.ClearProgressBar();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.Label("Tile Tools", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("resourcePath"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rawData"), true);

        GUILayout.Label("Journey Cache", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("scriptableCache"), true);

        EditorGUILayout.HelpBox("Journey Cache object will use download directory!", MessageType.Info, true);

        if (GUILayout.Button("Create Journey Cache Object"))
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Warning!", "Cannot perform action at runtime!", "OK");
                return;
            }

            if(script.MapCache == null)
            {
                MapScriptableCache asset = ScriptableObject.CreateInstance<MapScriptableCache>();

                AssetDatabase.CreateAsset(asset, "Assets/" + script.Directory + "/MapScriptableCache_" + script.MapCache.mapName + ".asset");
                AssetDatabase.SaveAssets();

                script.MapCache = asset;
                script.MapCache.mapName = script.ID;
            }
            else
            {
                EditorUtility.DisplayDialog("Warning!", "Scriptable Map Cache Object already exists.", "OK");
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Debug Mode", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugOn"), true);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(script);
    }

    private List<ZoomStep> GetSteps()
    {
        List<ZoomStep> steps = new List<ZoomStep>();

        int count = 0;

        for (float f = script.MinZoom; f <= script.MaxZoom;)
        {
            ZoomStep zStep = new ZoomStep(f, (count.Equals(0)) ? true : false);

            if(count.Equals(0))
            {
                activeStep = zStep;
            }

            steps.Add(zStep);

            f += script.ConvertZoomInterval;
            count++;
        }

        return steps;
    }

    private IEnumerator DownloadSegments()
    {
        for (int i = 0; i < steps.Count; i++)
        {
            script.ZoomLevel = steps[i].step;
            script.DownloadBaseTexture();

            while(script.Downloading)
            {
                yield return null;
            }

            CreateSegment();

            progress++;
        }

        downloadingSegments = false;

        EditorUtility.ClearProgressBar();
    }

    private void CreateSegment()
    {
        List<MapPNG> temp = MapUtils.CreateCache(script.Display.GetComponent<RectTransform>(), script.Cam, script.Type, script.ZoomLevel, Application.dataPath + script.Directory, script.ResourcePath);

        if (script.RawData.Count <= 0) script.RawData.AddRange(temp);
        else
        {
            foreach (MapPNG png in temp)
            {
                MapPNG current = script.RawData.FirstOrDefault(p => p.name.Equals(png.name));

                if (current != null)
                {
                    current.matrix = png.matrix;

                    foreach (MapZoomLevel zLevel in png.zoomLevels)
                    {
                        MapZoomLevel exists = current.zoomLevels.FirstOrDefault(z => z.zoom.Equals(zLevel.zoom));

                        if (exists != null)
                        {
                            exists.internalURL = zLevel.internalURL;
                        }
                        else current.zoomLevels.Add(zLevel);
                    }
                }
                else script.RawData.Add(png);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    [System.Serializable]
    private class ZoomStep
    {
        public float step;
        public bool active;

        public ZoomStep(float s, bool a)
        {
            step = s;
            active = a;
        }
    }
}
