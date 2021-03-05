using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;
using UnityEngine.Audio;

public class Recorder : MonoBehaviour
{
    // Public
    public static Recorder inst;

    [HideInInspector] public bool isRecording = false;
    [HideInInspector] public bool isPreRecording = false;
    [HideInInspector] public int preRecCounter;
    

    // Private
    private Recording recording = new Recording();
    private Coroutine recordBar;


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
    private Sequencer CurSequencer { get { return MusicManager.inst.curSequencer; } }




    void Start()
    {
        inst = this;

        MeshRef.inst.recordText.enabled = false;
        MeshRef.inst.recordBar.enabled = false;
        MeshRef.inst.recordImage.enabled = false;
        MeshRef.inst.preRecordCounter.enabled = false;
        MeshRef.inst.recordBarFill.enabled = false;
        
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
        MeshRef.inst.recordBarFill.enabled = true;
        MeshRef.inst.recordBarFill.color = recordColor;
        if (!Has1stRecord)
            recordBar = StartCoroutine(RecordingBar());
        MeshRef.inst.preRecordCounter.enabled = false;
        
        // 3. Record
        // 3.1. subscribe
        GameEvents.inst.onPlayField += SaveToSequencer_start;
        GameEvents.inst.onStopField += SaveToSequencer_end;
        // 3.2. Chord already playing?
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            recording.start = (float) CurSequencer.GetSequencerPosition();
            recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        }
        // 3.3. Sequencer data
        if (!Has1stRecord)
        {
            recording.sequencerStart = (float)CurSequencer.GetSequencerPosition();
            recording.sequencerEnd = recording.sequencerStart + CurSequencer.length;
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
        var color = VisualController.inst.preRecordColor;
        MeshRef.inst.recordText.enabled = false;
        MeshRef.inst.recordImage.enabled = false;
        MeshRef.inst.recordBar.color = color;
        MeshRef.inst.preRecordCounter.enabled = false;
        MeshRef.inst.recordBarFill.color = color;

        // Variables
        isRecording = false;
        if (isPreRecording)
        {
            GameEvents.inst.onQuarter -= OnStartRecordDelayed;
        }
        isPreRecording = false;

        // If there's a chord being played
        if (recording.end == -1)
        {
            SaveToSequencer_end();
        }

        // 3.1. UNsubscribe
        GameEvents.inst.onPlayField -= SaveToSequencer_start;
        GameEvents.inst.onStopField -= SaveToSequencer_end;
    }




    public void ClearSequencer(int layer)
    {
        MusicRef.inst.sequencers[layer].Clear();
        if (!Has1stRecord)
        {
            StopCoroutine(recordBar);
            MeshRef.inst.recordBar.enabled = false;
            MeshRef.inst.recordBarFill.enabled = false;
        }
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

    /// <summary>
    /// Save start time and notes of the currently played chord.
    /// </summary>
    private void SaveToSequencer_start()
    {
        recording.start = (float) MusicManager.inst.curSequencer.GetSequencerPosition();
        recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        recording.end = -1;
    }


    /// <summary>
    /// Save end-time of the currently played chord and store it in the sequencer.
    /// </summary>
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
        MeshRef.inst.recordBarFill.enabled = false;

        MeshRef.inst.preRecordCounter.enabled = true;
        MeshRef.inst.preRecordCounter.text = "";

      
        // Reset1sBeats()                   // TO DO maybe
    }



    // Visuals

    private IEnumerator RecordingBar()
    {
        while (true)
        {
            float curSequencerPos = (float)CurSequencer.GetSequencerPosition();
            if (curSequencerPos < recording.sequencerStart)
                curSequencerPos += CurSequencer.length;

            float fillPercentage = (curSequencerPos - recording.sequencerStart) / CurSequencer.length;

            MeshRef.inst.recordBarFill.fillAmount = fillPercentage;

            yield return null;
        }

    }






    public class Recording
    {
        public float start;
        public float end;
        public int[] notes;
        /// <summary>
        /// Position in the sequencer, measured in sixteenth.
        /// </summary>
        public float sequencerStart;
        /// <summary>
        /// Extended position in the sequencer (doesn't actually exist), measured in sixteenth. 
        /// </summary>
        public float sequencerEnd;
    }

}



