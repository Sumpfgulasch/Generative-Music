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
    public int layer;
    public float start;
    public float end;
    public float loopStart;
    public float loopEnd_extended;

    public MeshRenderer meshRenderer;
    public Color startColor;


    //[HideInInspector] 
    public bool isRecording = true;

    //private bool hasRespawned = false;
    private bool hasEnteredField = false;
    private bool hasLeftField = false;
    private bool hasLeftScreen = false;
    

    // Properties
    public float StartZPos { get { return obj.transform.position.z; } }
    public float EndZPos { get { return StartZPos + obj.transform.localScale.z; } }
    private float DeltaTime { get { return Time.deltaTime * FPS; } }

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
        if (StartZPos <= Player.inst.transform.position.z + 0.05f)
        {
            if (!hasEnteredField)
            {
                GameEvents.inst.onRecObjFieldEnter?.Invoke(this);
                hasEnteredField = true;
                print("enter field invoke");
            }
        }

        // Exit
        if (EndZPos <= Player.inst.transform.position.z)
        {
            if (!hasLeftField)
            {
                GameEvents.inst.onRecObjFieldExit?.Invoke(this);
                hasLeftField = true;
                print("exit field invoke");
            }
        }

        // Exit screen
        if (EndZPos <= -2f)
        {
            if (!hasLeftField)
            {
                GameEvents.inst.onRecObjFieldExit?.Invoke(this);
                hasLeftField = true;
                print("exit SCREEN invoke");
            }
        }
    }




    // ------------------------------ Public functions ------------------------------


    /// <summary>
    /// Add a RecordObject-component to the first parameter. Set remaining variables.
    /// </summary>
    public static RecordObject Create(GameObject obj, RecordObject douplicate, Vector3 position, int fieldID, int[] notes, Sequencer sequencer, int layer, float start, float end, float loopStart, float loopEnd_extended)
    {
        var thisObj = obj.AddComponent<RecordObject>();

        // set
        thisObj.obj = obj;
        thisObj.douplicate = douplicate;
        thisObj.obj.transform.position = position;
        thisObj.fieldID = fieldID;
        thisObj.notes = notes;
        thisObj.sequencer = sequencer;
        thisObj.layer = layer;
        thisObj.start = start;
        thisObj.end = end;
        thisObj.loopStart = loopStart;
        thisObj.loopEnd_extended = loopEnd_extended;

        // Mesn & colors
        thisObj.meshRenderer = obj.GetComponent<MeshRenderer>();
        if (thisObj.meshRenderer == null)
            Debug.LogError("mesh rend sollte nich null sein");
        var color = VisualController.inst.colorPalette[layer];
        color.a = VisualController.inst.recordObjectsAlpha;
        thisObj.meshRenderer.material.color = color;
        thisObj.startColor = thisObj.meshRenderer.material.color;

        // add
        thisObj.obj.AddComponent<Move>();

        // stuff
        thisObj.FPS = Screen.currentResolution.refreshRate;

        return thisObj;
    }


    public void Set()
    {
        
    }



    // ------------------------------ Private functions ------------------------------



    //private void Move()
    //{
    //    //this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    //}

}
