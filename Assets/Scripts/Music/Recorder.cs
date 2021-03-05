using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;
using UnityEngine.Audio;

public class Recorder : MonoBehaviour
{
    public static Recorder inst;

    public bool isRecording = false;
    public bool isPreRecording = false;
    public int beatCounter;
    public Canvas canvas;

    //private float velocity;
    private Recording recording = new Recording();


    // Properties
    public bool Has1stRecord
    {
        get
        {
            foreach (Sequencer sequencer in MusicRef.inst.sequencers)
            {
                var notes = sequencer.GetAllNotes();
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

        // 3. Record
        // 3.1. subscribe
        GameEvents.inst.onPlayField += SaveToSequencer_start;
        GameEvents.inst.onStopField += SaveToSequencer_end;

        if (Player.inst.actionState == Player.ActionState.Play)
        {
            recording.start = (float) MusicManager.inst.curSequencer.GetSequencerPosition();
            recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        }

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

        if (isPreRecording)
        {
            isPreRecording = false;
            GameEvents.inst.onQuarter -= OnStartRecordDelayed;
        }

        EnableVisuals(false);

        // 3.1. UNsubscribe
        GameEvents.inst.onPlayField -= SaveToSequencer_start;
        GameEvents.inst.onStopField -= SaveToSequencer_end;

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
            isPreRecording = false;
        }

        beatCounter++;
    }



    // ------------------------------ Private functions ------------------------------


    // Recording

    private void SaveToSequencer_start()
    {
        recording.start = (float)MusicManager.inst.curSequencer.GetSequencerPosition();
        recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
    }


    private void SaveToSequencer_end()
    {
        Sequencer sequencer = MusicManager.inst.curSequencer;

        recording.end = (float)sequencer.GetSequencerPosition();
        float velocity = MusicManager.inst.velocity;

        foreach (int note in recording.notes)
        {
            sequencer.AddNote(note, recording.start, recording.end, velocity);
        }
    }





    /// <summary>
    /// Set variables and update canvas.
    /// </summary>
    private void InitPreRecord(int delayInBeats)
    {
        // Variables
        isPreRecording = true;
        beatCounter = -delayInBeats;
        //isRecording = true;

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


    public class Recording
    {
        public float start;
        public float end;
        public int[] notes;
    }

}



