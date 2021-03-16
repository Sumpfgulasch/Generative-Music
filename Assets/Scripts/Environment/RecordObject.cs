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
    public Sequencer sequencer;
    public float start;
    public float end;
    public float loopStart;
    public float loopEnd_extended;


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

    public RecordObject(GameObject obj, Vector3 position, int fieldID, int[] notes, Sequencer sequencer, float start, float end, float loopStart, float loopEnd_extended)
    {
        // set
        this.obj = obj;
        this.obj.transform.position = position;
        this.fieldID = fieldID;
        this.notes = notes;
        this.sequencer = sequencer;
        this.start = start;
        this.end = end;
        this.loopStart = loopStart;
        this.loopEnd_extended = loopEnd_extended;

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
