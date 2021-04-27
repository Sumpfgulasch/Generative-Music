using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

/// <summary>
/// Contains ONE chord always.
/// </summary>
/// 
public class ChordObject : RecordObject
{
    public int fieldID;
    public int[] notes;

    public new ChordObject douplicate;
    public Sequencer sequencer;
    public int trackLayer;
    public MeshRenderer meshRenderer;
    public Color startColor;


    // Properties

    /// <summary>
    /// Get the length of one record / chord, between 0 and 1 (1 == sequencer.length).
    /// </summary>
    public float Length
    {
        get
        {
            float end = this.end;
            if (end < start)
            {
                end += sequencer.length;
            }
            return (end - start) / sequencer.length; // 0-1
        }
    }

    //private float FPS;




    // Update
    private void Update()
    {
        //InvokeFieldEvents();
    }


    




    // ------------------------------ Public functions ------------------------------


    /// <summary>
    /// Add a ChordObject-component to the first parameter. Set remaining variables.
    /// </summary>
    public static ChordObject Create(GameObject obj, ChordObject douplicate, Vector3 position, int fieldID, int[] notes, Sequencer sequencer, int trackLayer, float start, float end, float loopStart, float loopEnd_extended)
    {
        var thisObj = obj.AddComponent<ChordObject>();

        // set
        thisObj.obj = obj;
        thisObj.douplicate = douplicate;
        thisObj.obj.transform.position = position;
        thisObj.fieldID = fieldID;
        thisObj.notes = notes;
        thisObj.sequencer = sequencer;
        thisObj.trackLayer = trackLayer;
        thisObj.start = start;
        thisObj.end = end;
        thisObj.loopStart = loopStart;
        thisObj.loopEnd_extended = loopEnd_extended;

        // Mesn & colors
        thisObj.meshRenderer = obj.GetComponent<MeshRenderer>();
        if (thisObj.meshRenderer == null)
            Debug.LogError("mesh rend sollte nich null sein");
        var color = VisualController.inst.colorPalette[trackLayer];
        color.a = VisualController.inst.chordObjectsOpacity;
        thisObj.meshRenderer.material.color = color;
        thisObj.startColor = thisObj.meshRenderer.material.color;

        // add
        thisObj.obj.AddComponent<Move>();

        // stuff
        //thisObj.FPS = Screen.currentResolution.refreshRate;

        return thisObj;
    }


   
    /// <summary>
    /// Set the scale, appropriate to start- and end-position in the sequencer.
    /// </summary>
    public void UpdateScale()
    {
        float scale = LoopData.distancePerRecLoop * Length;
        obj.transform.localScale = new Vector3(1, 1, scale);
    }



    // ------------------------------ Private functions ------------------------------



   

}
