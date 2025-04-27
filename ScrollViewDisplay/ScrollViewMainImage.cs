using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollViewMainImage : MonoBehaviour
{
    [SerializeField]
    private ScrollViewDisplay displayController;

    [SerializeField]
    private string path = "";

    private RawImage img;

    private void Awake()
    {
        if(displayController != null)
        {
            displayController.OnSelectEvent += UpdateImage;
        }

        img = GetComponent<RawImage>();
    }

    private void OnDisable()
    {
        if(img != null)
        {
            img.texture = null;
        }
    }

    public void UpdateImage(GameObject obj)
    {
        if(img != null)
        {
            string imageName = obj.GetComponentInChildren<RawImage>().texture.name;

            if(imageName.Length > 0)
            {
                img.texture = Resources.Load<Texture>(path + imageName);
            }
        }
    }
}
