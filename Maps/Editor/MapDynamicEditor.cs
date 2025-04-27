using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapDynamic))]
public class MapDynamicEditor : Editor
{
    private MapDynamic script;

    private bool downloadingSegments;
    private bool creatingTextures;

    private float textCount;
    private float texProgress;

    private float interval;
    private List<ZoomStep> steps;
    private bool stepFoldout;
    private ZoomStep activeStep;

    private void OnEnable()
    {
        script = (MapDynamic)target;

        script.CancelDownload();

        downloadingSegments = false;
        creatingTextures = false;

        interval = script.ConvertZoomInterval;
        steps = GetSteps();

        EditorUtility.ClearProgressBar();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("processOnStart"), true);

        GUILayout.Label("Tiling (Utils)", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("unityTileSize"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("root"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseMaterial"), true);

        GUILayout.Label("Tiling (Size)", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("west"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("north"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("east"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("south"), true);

        GUILayout.Label("Download Tools", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("directory"), true);

        stepFoldout = EditorGUILayout.Foldout(stepFoldout, "Zoom Intervals");

        if (stepFoldout)
        {
            EditorGUILayout.BeginVertical();

            if(steps.Count - 1 != script.ConvertZoomDistance)
            {
                steps = GetSteps();
            }

            if (interval != script.ConvertZoomInterval)
            {
                interval = script.ConvertZoomInterval;
                steps = GetSteps();
            }

            for (int i = 0; i < steps.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Step " + steps[i].step.ToString(), GUILayout.MaxWidth(100));

                if (activeStep.Equals(steps[i]))
                {
                    steps[i].active = true;
                }

                steps[i].active = EditorGUILayout.Toggle(steps[i].active);

                if (steps[i].active)
                {
                    script.ZoomLevel = steps[i].step;

                    if (activeStep.Equals(steps[i]))
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

        if (GUILayout.Button("Download Base Map"))
        {
            if (script.Downloading) return;

            if (creatingTextures) return;

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Warning!", "Cannot perform action at runtime!", "OK");
                return;
            }

            script.DownloadBaseTexture();

            downloadingSegments = true;
        }

        if (GUILayout.Button("Create Segments"))
        {
            if (downloadingSegments) return;

            if (creatingTextures) return;

            creatingTextures = true;
            textCount = (float)script.TileCount;
            texProgress = 0.0f;

            script.StartCoroutine(CreateTextures());
        }

        if (downloadingSegments)
        {
            EditorUtility.DisplayProgressBar("Downloading " + script.Type + " segments", "Zoom level.... " + script.ZoomLevel.ToString() , (float)script.Progress / (float)script.TileCount);

            if(!script.Downloading) EditorUtility.ClearProgressBar();
        }

        if(creatingTextures)
        {
            EditorUtility.DisplayProgressBar("Creating " + script.Type + " textures", "Zoom level.... " + script.ZoomLevel.ToString() , texProgress / textCount);

            if(texProgress >= script.TileCount) EditorUtility.ClearProgressBar();
        }

        if (downloadingSegments && script.Progress >= script.TileCount) downloadingSegments = false;

        if (creatingTextures && texProgress >= script.TileCount) creatingTextures = false;

        GUILayout.Label("Journey Cache", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("scriptableCache"), true);

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

            if (count.Equals(0))
            {
                activeStep = zStep;
            }

            steps.Add(zStep);

            f += script.ConvertZoomInterval;
            count++;
        }

        return steps;
    }

    private IEnumerator CreateTextures()
    {
        for (int i = 0; i < script.Tiles.Length; i++)
        {
            Texture2D tex = (Texture2D)script.Tiles[i].MeshRenderer.material.mainTexture;

            byte[] bytes = tex.EncodeToPNG();

            if (!Directory.Exists(script.Directory + "/" + script.Type.ToString() + "/" + script.ZoomLevel.ToString()))
            {
                Directory.CreateDirectory(script.Directory + "/" + script.Type.ToString() + "/" + script.ZoomLevel.ToString());
            }

            File.WriteAllBytes(script.Directory + "/" + script.Type.ToString() + "/" + script.ZoomLevel.ToString() + "/" + script.Tiles[i].gameObject.name.Replace("/", "_") + "_" + script.Tiles[i].ID + ".png", bytes);

            texProgress += 1.0f;

            yield return new WaitForSeconds(0.1f);
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
