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
        GameEvents.inst.onPlayField += SaveRecordStartData;
        GameEvents.inst.onStopField += SaveToSequencer;
        GameEvents.inst.onStopField += InstantiateRecordedChord;
        // 3.2. Chord already playing?
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            recording.curChordStart = (float) CurSequencer.GetSequencerPosition();
            recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        }
        // 3.3. Sequencer data
        if (!Has1stRecord)
        {
            recording.sequencerLoopStart = (float)CurSequencer.GetSequencerPosition();
            recording.sequencerLoopEnd_extended = recording.sequencerLoopStart + CurSequencer.length;
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
        if (recording.curChordEnd == -1)
        {
            SaveToSequencer();
        }

        // Disable record bar?
        if (!Has1stRecord)
        {
            DisableRecordBar();
        }

        // 3.1. UNsubscribe
        GameEvents.inst.onPlayField -= SaveRecordStartData;
        GameEvents.inst.onStopField -= SaveToSequencer;
        GameEvents.inst.onStopField -= InstantiateRecordedChord;
    }



    /// <summary>
    /// Clear all notes from a given sequencer. Disable recordBar maybe.
    /// </summary>
    public void ClearSequencer(int layer)
    {
        MusicRef.inst.sequencers[layer].Clear();
        if (!Has1stRecord)
        {
            DisableRecordBar();
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
    private void SaveRecordStartData()
    {
        recording.curChordStart = (float)CurSequencer.GetSequencerPosition();
        recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        recording.curChordEnd = -1;
        recording.curID = Player.inst.curField.ID;
    }


    /// <summary>
    /// Save end-time of the currently played chord and store it in the sequencer.
    /// </summary>
    private void SaveToSequencer()
    {
        recording.curChordEnd = (float) CurSequencer.GetSequencerPosition();
        float velocity = MusicManager.inst.velocity;

        foreach (int note in recording.notes)
        {
            CurSequencer.AddNote(note, recording.curChordStart, recording.curChordEnd, velocity);
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

    /// <summary>
    /// Get the percentage of the current position of a given sequencer, with start and end point defined by recording-variable (!).
    /// </summary>
    private float SequencerPositionPercentage(Sequencer sequencer, float sequencerPos, Recording recording)
    {
        if (sequencerPos < recording.sequencerLoopStart)
            sequencerPos += sequencer.length;

        float percentage = (sequencerPos - recording.sequencerLoopStart) / sequencer.length; // 0-1

        return percentage;
    }


    // Visuals

    private void InstantiateRecordedChord()
    {
        float curSeqencerPos = (float) CurSequencer.GetSequencerPosition();
        float percentageToNextCycle = 1 - SequencerPositionPercentage(CurSequencer, curSeqencerPos, recording);
        float distanceToNextCycle = percentageToNextCycle * LoopData.distancePerRecLoop;
        Vector3 nextCycleStartPos = Player.inst.transform.position + Vector3.forward * distanceToNextCycle;

        float startPos = recording.curChordEnd;
    }

    private void DisableRecordBar()
    {
        StopCoroutine(recordBar);
        MeshRef.inst.recordBar.enabled = false;
        MeshRef.inst.recordBarFill.enabled = false;
    }

    private IEnumerator RecordingBar()
    {
        while (true)
        {
            float curSequencerPos = (float) CurSequencer.GetSequencerPosition();
            float percentage = SequencerPositionPercentage(CurSequencer, curSequencerPos, recording);

            MeshRef.inst.recordBarFill.fillAmount = percentage;

            yield return null;
        }

    }





    /// <summary>
    /// Class for momentarily recording data (e.g. current start). All data refers to a sequencer. Always holding data for ONE chord.
    /// </summary>
    public class Recording
    {
        public int curID;
        public Sequencer sequencer;
        /// <summary>
        /// Position in the sequencer, measured in sixteenth.
        /// </summary>
        public float curChordStart;
        /// <summary>
        /// Position in the sequencer, measured in sixteenth.
        /// </summary>
        public float curChordEnd;
        public int[] notes;
        /// <summary>
        /// Position in the sequencer, measured in sixteenth.
        /// </summary>
        public float sequencerLoopStart;
        /// <summary>
        /// Extended position in the sequencer (doesn't actually exist), measured in sixteenth. 
        /// </summary>
        public float sequencerLoopEnd_extended;
    }

}



