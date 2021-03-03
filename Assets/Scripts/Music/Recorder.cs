using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class Recorder : MonoBehaviour
{
    public static Recorder inst;

    public bool isRecording = false;
    public bool preRecording = false;
    public int beatCounter;
    public Canvas canvas;

    public List<Sequencer> layers;


    // Properties
    //private VisualController Visuals { get { return VisualController.inst; } }
    public bool Has1stRecord
    {
        get
        {
            foreach (Sequencer layer in layers)
            {
                var notes = layer.GetAllNotes();
                print("sequencer notes.count: " + notes.Count);
                if (notes.Count != 0)
                    return true;
            }
            return false;
        }
    }


    void Start()
    {
        inst = this;
    }



    // ------------------------------ Public functions ------------------------------

     

        
    /// <summary>
    /// Record a layer.
    /// </summary>
    public void StartRecord()
    {
        // 1. Variables
        isRecording = true;
        
        // 2. Visuals
        var text = "";
        var color = VisualController.inst.recordColor;
        SetText(text, color);

        SetRecordingBar(color);
        StartCoroutine(RecordingBar());

        var frameWidth = VisualController.inst.frameWidth;
        SetFrame(color, frameWidth);

        EnableVisuals(true);

        Debug.Log("RECORD");
    }


    /// <summary>
    /// Start record after counting one bar. Show canvas. Beats in quarters.
    /// </summary>
    public void StartRecordDelayed(int delayInBeats)
    {
        // 1. Init
        InitPreRecord(delayInBeats);

        // 2. Subscribe
        GameEvents.inst.onQuarter += OnStartRecordDelayed;
    }



    public void StopRecord()
    {
        isRecording = false;

        EnableVisuals(false);

        Debug.Log("stop record");
    }



    // ---------------------------------- Events ----------------------------------



    /// <summary>
    /// Show pre-recording. Start record after that. Used to subscribe to onBeat-Event.
    /// </summary>
    private void OnStartRecordDelayed()
    {
        // 1. Count down and show on canvas
        var text = Mathf.Abs(beatCounter).ToString();
        var color = VisualController.inst.preRecordColor;
        SetText(text, color);

        Debug.Log("pre rec: " + text);
        
        // 2. Start record and unsubscribe
        if (beatCounter == 0)
        {
            StartRecord();
            GameEvents.inst.onQuarter -= OnStartRecordDelayed;
        }

        beatCounter++;
    }



    // ------------------------------ Private functions ------------------------------



    /// <summary>
    /// Set variables and update canvas.
    /// </summary>
    private void InitPreRecord(int delayInBeats)
    {
        // Variables
        beatCounter = -delayInBeats;
        preRecording = true;

        // Canvas
        EnableVisuals(true);
        var text = "";
        var color = VisualController.inst.preRecordColor;
        SetText(text, color);
        

        // Reset1sBeats()                   // TO DO maybe
    }



    // Visuals


    /// <summary>
    /// Set text.
    /// </summary>
    private void SetText(string text, Color color)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    private void SetRecordingBar(Color color)
    {

    }

    /// <summary>
    /// Set a window frame.
    /// </summary>
    private void SetFrame(Color color, float width)
    {

    }

    private void EnableVisuals(bool value)
    {

    }

    private IEnumerator RecordingBar()
    {

        yield return null;
    }

}
