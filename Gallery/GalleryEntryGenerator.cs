using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.Gallery;

public class GalleryEntryGenerator : MonoBehaviour
{
    [SerializeField]
    private Gallery gallery;

    [SerializeField]
    private bool useElementsStructure = true;

    [SerializeField]
    private List<GEntryElement> elementsStructure = new List<GEntryElement>();

    public List<GEntry> Entries { get { return gallery.Entries; } }

    public List<GEntryElement> ElementdStructure {  get { return elementsStructure; } }

    public bool UseStructure {  get { return useElementsStructure; } }
}
