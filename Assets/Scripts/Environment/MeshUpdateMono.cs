using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshUpdateMono : MonoBehaviour
{
    public static MeshUpdateMono inst;

    delegate void QuarterEvent();
    QuarterEvent quarterEvent;

    void Start()
    {
        inst = this;
        //quarterEvent = GameEvents.inst.onQuarter;
    }


    

    /// <summary>
    /// Highlight beat triangle for a certain time.
    /// </summary>
    public void ShowBeat()
    {
        var material = MeshRef.inst.tunnelEdges_lr.material;
        var colorName1 = "_BaseColor";
        var colorName2 = "_EmissionColor";
        var startColor1 = VisualController.inst.highlightBeatColor;
        var startColor2 = VisualController.inst.highlightBeatColor * VisualController.inst.highlightBeatIntensity;
        var endColor = new Color(0, 0, 0, 0);
        var curve = VisualController.inst.highlightBeatCurve;
        var time = VisualController.inst.highlightBeatTime;

        StartCoroutine(LerpColor(material, colorName1, startColor1, endColor, curve, time));
        StartCoroutine(LerpColor(material, colorName2, startColor2, endColor, curve, time));
    }


    /// <summary>
    /// Lerps and sets a material color.
    /// </summary>
    private static IEnumerator LerpColor(Material material, string colorName, Color startColor, Color endColor, AnimationCurve curve, float time, bool disableWhenDone = false)
    {
        float timer = 0;
        while (timer < time)
        {
            float t = curve.Evaluate(timer / time);
            Color color = Color.Lerp(endColor, startColor, t);
            
            material.SetColor(colorName, color);
            timer += Time.deltaTime;
            yield return null;
        }

    }

}
