using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

/// <summary>
/// Contains ONE chord always.
/// </summary>
/// 
[SerializeField]
public class RecordObject
{
    public int fieldID;
    public int[] notes;
    public GameObject obj;
    public RecordObject douplicate;
    public Sequencer sequencer;
    public int layer;
    public float start;
    public float end;
    public float loopStart;
    public float loopEnd_extended;

    public MeshRenderer meshRenderer;
    public Color startColor;


    //[HideInInspector] 
    public bool isRecording = true;
    //[HideInInspector] 
    public bool hasRespawned = false;
    //[HideInInspector] 
    //public Recording data;
    

    // Properties
    public float StartZPos { get { return obj.transform.position.z; } }
    public float EndZPos { get { return StartZPos + obj.transform.localScale.z; } }
    private float DeltaTime { get { return Time.deltaTime * FPS; } }

    private float FPS;




    // Constructor

    public RecordObject(GameObject obj, RecordObject douplicate, Vector3 position, int fieldID, int[] notes, Sequencer sequencer, int layer, float start, float end, float loopStart, float loopEnd_extended)
    {
        // set
        this.obj = obj;
        this.douplicate = douplicate;
        this.obj.transform.position = position;
        this.fieldID = fieldID;
        this.notes = notes;
        this.sequencer = sequencer;
        this.layer = layer;
        this.start = start;
        this.end = end;
        this.loopStart = loopStart;
        this.loopEnd_extended = loopEnd_extended;

        meshRenderer = obj.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            Debug.LogError("mesh rend sollte nich null sein");
        var color = VisualController.inst.colorPalette[layer];
        color.a = VisualController.inst.recordObjectsAlpha;
        meshRenderer.material.color = color;
        startColor = meshRenderer.material.color;

        // add
        this.obj.AddComponent<Move>();

        FPS = Screen.currentResolution.refreshRate;
    }







    // ------------------------------ Public functions ------------------------------


    
    public void Set()
    {
        
    }



    // ------------------------------ Private functions ------------------------------



    //private void Move()
    //{
    //    //this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    //}

}
