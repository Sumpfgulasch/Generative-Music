using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

/// <summary>
/// Contains ONE chord always.
/// </summary>
/// 
public class RecordObject : MonoBehaviour
{
    public int fieldID;
    public int[] notes;
    public GameObject obj;
    public RecordObject douplicate;
    public Sequencer sequencer;
    public int trackLayer;
    public float start;
    public float end;
    public float loopStart;
    public float loopEnd_extended;
    public bool isPlaying;
    

    public MeshRenderer meshRenderer;
    public Color startColor;


    //[HideInInspector] 
    //public bool isRecording = true;

    public bool hasRespawned = false;
    private bool hasEnteredField = false;
    private bool hasLeftField = false;
    private bool hasLeftScreen = false;
    

    // Properties
    public float StartZPos { get { return obj.transform.position.z; } }
    public float EndZPos { get { return StartZPos + obj.transform.localScale.z; } }
    private float DeltaTime { get { return Time.deltaTime * FPS; } }

    private float length;
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

    private float FPS;



    #region Constructor

    //public RecordObject(GameObject obj, RecordObject douplicate, Vector3 position, int fieldID, int[] notes, Sequencer sequencer, int layer, float start, float end, float loopStart, float loopEnd_extended)
    //{
    //    // set
    //    this.obj = obj;
    //    this.douplicate = douplicate;
    //    this.obj.transform.position = position;
    //    this.fieldID = fieldID;
    //    this.notes = notes;
    //    this.sequencer = sequencer;
    //    this.layer = layer;
    //    this.start = start;
    //    this.end = end;
    //    this.loopStart = loopStart;
    //    this.loopEnd_extended = loopEnd_extended;

    //    meshRenderer = obj.GetComponent<MeshRenderer>();
    //    if (meshRenderer == null)
    //        Debug.LogError("mesh rend sollte nich null sein");
    //    var color = VisualController.inst.colorPalette[layer];
    //    color.a = VisualController.inst.recordObjectsAlpha;
    //    meshRenderer.material.color = color;
    //    startColor = meshRenderer.material.color;

    //    // add
    //    this.obj.AddComponent<Move>();

    //    // stuff
    //    FPS = Screen.currentResolution.refreshRate;
    //}
    #endregion


    // Update
    private void Update()
    {
        InvokeFieldEvents();
    }


    /// <summary>
    /// Invoke Enter- and Exit-events for fields.
    /// </summary>
    private void InvokeFieldEvents()
    {
        // Enter
        if (StartZPos <= Player.inst.transform.position.z + 0.0f)
        {
            if (!hasEnteredField)
            {
                GameEvents.inst.onRecObjFieldEnter?.Invoke(this);
                hasEnteredField = true;
                isPlaying = true;
            }
        }

        // Exit
        if (EndZPos <= Player.inst.transform.position.z)
        {
            if (!hasLeftField)
            {
                GameEvents.inst.onRecObjFieldExit?.Invoke(this);
                hasLeftField = true;
                isPlaying = false;
            }
        }

        // Exit screen
        if (EndZPos <= -2f)
        {
            if (!hasLeftScreen)
            {
                GameEvents.inst.onRecObjScreenExit?.Invoke(this);
                hasLeftScreen = true;
            }
        }
    }




    // ------------------------------ Public functions ------------------------------


    /// <summary>
    /// Add a RecordObject-component to the first parameter. Set remaining variables.
    /// </summary>
    public static RecordObject Create(GameObject obj, RecordObject douplicate, Vector3 position, int fieldID, int[] notes, Sequencer sequencer, int trackLayer, float start, float end, float loopStart, float loopEnd_extended)
    {
        var thisObj = obj.AddComponent<RecordObject>();

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
        color.a = VisualController.inst.recordObjectsOpacity;
        thisObj.meshRenderer.material.color = color;
        thisObj.startColor = thisObj.meshRenderer.material.color;

        // add
        thisObj.obj.AddComponent<Move>();

        // stuff
        thisObj.FPS = Screen.currentResolution.refreshRate;

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



    //private void Move()
    //{
    //    //this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    //}

}
