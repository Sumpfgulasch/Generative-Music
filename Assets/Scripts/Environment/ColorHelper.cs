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

public class ColorHSV
{
    public float hue;
    public float saturation;
    public float value;

    public ColorHSV(float hue, float saturation, float value)
    {
        this.hue = hue;
        this.saturation = saturation;
        this.value = value;
    }
}
