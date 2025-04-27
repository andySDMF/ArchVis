using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Grain.Gallery;

public class GalleryEntryGeneratorWindow : EditorWindow
{
    private static GalleryEntryGenerator entryGenerator;

    [SerializeField]
    private static List<EditorEntry> entries = new List<EditorEntry>();

    [SerializeField]
    private static List<GEntryElement> elementsStructure;

    private static bool IsStructuredElement;

    private static GalleryEntryGeneratorWindow window;

    private Vector2 scrollPos;

    public static void Init(GalleryEntryGenerator script)
    {
        entryGenerator = script;
        Generate();

        window = (GalleryEntryGeneratorWindow)EditorWindow.GetWindow(typeof(GalleryEntryGeneratorWindow));

        window.minSize = new Vector2(800, 600);
        window.maxSize = new Vector2(800, 600);
        window.Show();
        
    }

    private void OnGUI()
    {
        GUILayout.Label("Entries", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(window.maxSize.x), GUILayout.Height(window.maxSize.y - 200));

        for (int i = 0; i < entries.Count; i++)
        {
            //need to display each entry
            entries[i].Display();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add New Entry"))
        {
            Add();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Auto ID Now"))
        {
            AutoId();
        }

        if (GUILayout.Button("Clear All Entries"))
        {
            ClearAll();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh"))
        {
            Generate();
        }
    }

    private static void Generate()
    {
        if (entryGenerator == null) return;

        entries.Clear();

        IsStructuredElement = entryGenerator.UseStructure;
        elementsStructure = entryGenerator.ElementdStructure;

        foreach (GEntry entry in entryGenerator.Entries)
        {
            EditorEntry eEntry = new EditorEntry(entry);
            entries.Add(eEntry);

            eEntry.Remove = Remove;
        }
    }

    private static void MoveUp(EditorEntry e)
    {
       
    }

    private static void MoveDown(EditorEntry e)
    {

    }

    private static void Remove(EditorEntry e)
    {
        if(entryGenerator != null)
        {
            entryGenerator.Entries.Remove(e.Get());
        }

        entries.Remove(e);
    }

    private void ClearAll()
    {
        entries.Clear();

        if (entryGenerator != null) entryGenerator.Entries.Clear();
    }

    private void Add()
    {
        EditorEntry eEntry = new EditorEntry(new GEntry());
        entries.Add(eEntry);

        eEntry.Remove = Remove;

        if (entryGenerator != null)
        {
            entryGenerator.Entries.Add(eEntry.Get());
            eEntry.Get().elements = new List<GEntryElement>();
        }
    }

    private void AutoId()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            System.Guid guid = System.Guid.NewGuid();
            entries[i].SetID(guid.ToString());
        }
    }

    [System.Serializable]
    private class EditorEntry
    {
        [SerializeField]
        private GEntry entry;

        public System.Action<EditorEntry> Up;
        public System.Action<EditorEntry> Down;
        public System.Action<EditorEntry> Remove;

        private int cachedElementCount = 0;

        public EditorEntry(GEntry e)
        {
            entry = e;
        }

        public GEntry Get()
        {
            return entry;
        }

        public void SetID(string s)
        {
            entry.id = s;
        }

        public void Display()
        {
            GUILayout.Label("Entry " + entry.id, EditorStyles.boldLabel);

            entry.id = EditorGUILayout.TextField("ID: ", entry.id);

            EditorGUILayout.Space();

            if (entry.elements != null)
            {
                if (GalleryEntryGeneratorWindow.IsStructuredElement)
                {
                    if(entry.elements.Count != GalleryEntryGeneratorWindow.elementsStructure.Count)
                    {
                        if (entry.elements.Count > GalleryEntryGeneratorWindow.elementsStructure.Count)
                        {
                            int count = cachedElementCount;

                            for (int i = count; i > GalleryEntryGeneratorWindow.elementsStructure.Count; i--)
                            {
                                entry.elements.Remove(entry.elements[i - 1]);
                            }
                        }
                        else
                        {
                            for (int i = entry.elements.Count; i < GalleryEntryGeneratorWindow.elementsStructure.Count; i++)
                            {
                                GEntryElement gElement = new GEntryElement();
                                entry.elements.Add(gElement);

                                entry.elements[i].id = GalleryEntryGeneratorWindow.elementsStructure[i].id;
                                entry.elements[i].data = GalleryEntryGeneratorWindow.elementsStructure[i].data;
                            }
                        }
                    }
                }

                for (int i = 0; i < entry.elements.Count; i++)
                {
                    GUILayout.Label("Element " + i, EditorStyles.boldLabel);

                    entry.elements[i].id = EditorGUILayout.TextField("ID: ", entry.elements[i].id);
                    entry.elements[i].data = EditorGUILayout.TextField("Data: ", entry.elements[i].data);

                    if (!GalleryEntryGeneratorWindow.IsStructuredElement)
                    {
                        if (GUILayout.Button("Remove Element"))
                        {
                            entry.elements.Remove(entry.elements[i]);
                        }
                    }
                }

                cachedElementCount = entry.elements.Count;
            }
            else
            {
                cachedElementCount = 0;
            }

            if (!GalleryEntryGeneratorWindow.IsStructuredElement)
            {
                if (GUILayout.Button("Add Element"))
                {
                    GEntryElement gElement = new GEntryElement();
                    entry.elements.Add(gElement);
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Move Up"))
            {
               
            }

            if (GUILayout.Button("Move Down"))
            {

            }

            if (GUILayout.Button("Remove Entry"))
            {
                Remove(this);
            }

            EditorGUILayout.EndHorizontal();

            EditorUtility.SetDirty(GalleryEntryGeneratorWindow.window);
        }
    }
}
