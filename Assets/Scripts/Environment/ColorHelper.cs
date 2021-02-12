using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorHelper : MonoBehaviour
{
    public static ColorHelper inst;

    void Start()
    {
        inst = this;
    }

    public Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax)
    {
        return Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax);
    }

    
}
