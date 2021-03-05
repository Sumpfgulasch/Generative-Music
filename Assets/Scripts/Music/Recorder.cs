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
    public int preRecCounter;
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
        MeshRef.inst.recordText.enabled = false;
        MeshRef.inst.recordBar.enabled = false;
        MeshRef.inst.recordImage.enabled = false;
        MeshRef.inst.preRecordCounter.enabled = false;
        
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
        var recordColor = VisualController.inst.recordColor;
        MeshRef.inst.recordText.enabled = true;
        MeshRef.inst.recordText.color = recordColor;
        MeshRef.inst.recordImage.enabled = true;
        MeshRef.inst.recordImage.color = recordColor;
        MeshRef.inst.recordBar.enabled = true;
        MeshRef.inst.recordBar.color = recordColor;
        if (!Has1stRecord)
            StartCoroutine(RecordingBar());
        MeshRef.inst.preRecordCounter.enabled = false;
        
        // 3. Record
        // 3.1. subscribe
        GameEvents.inst.onPlayField += SaveToSequencer_start;
        GameEvents.inst.onStopField += SaveToSequencer_end;

        if (Player.inst.actionState == Player.ActionState.Play)
        {
            recording.start = (float) MusicManager.inst.curSequencer.GetSequencerPosition();
            recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        }
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
        // Visuals
        MeshRef.inst.recordText.enabled = false;
        MeshRef.inst.recordImage.enabled = false;
        var color = VisualController.inst.preRecordColor;
        MeshRef.inst.recordBar.color = color;
        MeshRef.inst.preRecordCounter.enabled = false;

        // Variables
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
    }



    // ---------------------------------- Events ----------------------------------



    /// <summary>
    /// Show pre-recording. Start record after that. Used to subscribe to onBeat-Event.
    /// </summary>
    private void OnStartRecordDelayed()
    {
        // 1. Count down and show on canvas
        var text = Mathf.Abs(preRecCounter).ToString();
        MeshRef.inst.preRecordCounter.text = text;
        
        // 2. Start record and unsubscribe
        if (preRecCounter == 0)
        {
            StartRecord();
            GameEvents.inst.onQuarter -= OnStartRecordDelayed;
            isPreRecording = false;
        }

        preRecCounter--;
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
        preRecCounter = delayInBeats;

        // Canvas
        var color = VisualController.inst.preRecordColor;
        MeshRef.inst.recordText.enabled = true;
        MeshRef.inst.recordText.color = color;
        MeshRef.inst.recordImage.enabled = true;
        MeshRef.inst.recordImage.color = color;
        MeshRef.inst.recordBar.enabled = true;
        MeshRef.inst.recordBar.color = color;

        MeshRef.inst.preRecordCounter.enabled = true;
        //var text = preRecCounter.ToString();
        MeshRef.inst.preRecordCounter.text = "";

      
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



