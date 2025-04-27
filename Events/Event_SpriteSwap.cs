using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Event_SpriteSwap : MonoBehaviour
{
    [SerializeField]
    private Image imageScript;

    [SerializeField]
    private string spriteOnTrue;

    [SerializeField]
    private bool isOnTrueMultipleSprite = false;

    [SerializeField]
    private int spriteOnTrueValue = 0;

    [SerializeField]
    private string spriteOnFalse;

    [SerializeField]
    private bool isOnFalseMultipleSprite = false;

    [SerializeField]
    private int spriteOnFalseValue = 0;

    public void Toggle(bool state)
    {
        if (imageScript == null) return;

        if(state)
        {
            if(!isOnTrueMultipleSprite)
            {
                imageScript.sprite = Resources.Load<Sprite>(spriteOnTrue);
            }
            else
            {
                imageScript.sprite = Resources.LoadAll<Sprite>(spriteOnTrue)[spriteOnTrueValue];
            }
        }
        else
        {
            if(!isOnFalseMultipleSprite)
            {
                imageScript.sprite = Resources.Load<Sprite>(spriteOnFalse);
            }
            else
            {
                imageScript.sprite = Resources.LoadAll<Sprite>(spriteOnFalse)[spriteOnFalseValue];
            }
        }

        imageScript.SetNativeSize();
    }
}
