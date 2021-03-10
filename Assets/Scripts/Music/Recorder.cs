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
    //[HideInInspector] 
    public List<RecordObject> recordObjects;
    

    // Private
    public RecordData chordRecordData = new RecordData();
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

        #region visuals
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
        #endregion

        // 3. Record
        // 3.1. subscribe
        GameEvents.inst.onPlayField += SaveRecordStartData;     // Music   
        GameEvents.inst.onPlayField += StartSpawnChordObject;   // Visual
        GameEvents.inst.onStopField += WriteToSequencer;        // Music
        GameEvents.inst.onStopField += StopSpawnChordObject;    // Visual

        // 3.2. Chord already playing?
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            chordRecordData.start = (float) CurSequencer.GetSequencerPosition();
            chordRecordData.notes = MusicManager.inst.curChord.DeepCopy().notes;
        }

        // 3.3. Set sequencer loop start?
        if (!Has1stRecord)
        {
            chordRecordData.sequencerLoopStart = (float)CurSequencer.GetSequencerPosition();
            chordRecordData.sequencerLoopEnd_extended = chordRecordData.sequencerLoopStart + CurSequencer.length;
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
        #region visuals
        // 1. Visuals
        var color = VisualController.inst.nonRecordColor;
        MeshRef.inst.recordText.enabled = false;
        MeshRef.inst.recordImage.enabled = false;
        MeshRef.inst.recordBar.color = color;
        MeshRef.inst.preRecordCounter.enabled = false;
        MeshRef.inst.recordBarFill.color = color;
        #endregion

        // 2. Variables
        isRecording = false;
        if (isPreRecording)
        {
            GameEvents.inst.onQuarter -= OnStartRecordDelayed;
        }
        isPreRecording = false;

        // If there's a chord being played
        if (chordRecordData.end == -1)
        {
            WriteToSequencer();
        }

        // Disable record bar?
        if (!Has1stRecord)
        {
            DisableRecordBar();
        }

        // 3.1. UNsubscribe
        GameEvents.inst.onPlayField -= SaveRecordStartData;
        GameEvents.inst.onPlayField -= StartSpawnChordObject;   // Visual
        GameEvents.inst.onStopField -= WriteToSequencer;
        GameEvents.inst.onStopField -= StopSpawnChordObject;    // Visual
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
        chordRecordData.start = (float)CurSequencer.GetSequencerPosition();
        chordRecordData.notes = MusicManager.inst.curChord.DeepCopy().notes;
        chordRecordData.end = -1;
        chordRecordData.fieldID = Player.inst.curField.ID;
    }


    /// <summary>
    /// Save end-time of the currently played chord and store it in the sequencer.
    /// </summary>
    private void WriteToSequencer()
    {
        chordRecordData.end = (float) CurSequencer.GetSequencerPosition();
        float velocity = MusicManager.inst.velocity;

        foreach (int note in chordRecordData.notes)
        {
            CurSequencer.AddNote(note, chordRecordData.start, chordRecordData.end, velocity);
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

        #region visuals
        // Canvas
        var color = VisualController.inst.nonRecordColor;
        MeshRef.inst.recordText.enabled = true;
        MeshRef.inst.recordText.color = color;
        MeshRef.inst.recordImage.enabled = true;
        MeshRef.inst.recordImage.color = color;
        MeshRef.inst.recordBar.enabled = true;
        MeshRef.inst.recordBar.color = color;
        MeshRef.inst.recordBarFill.enabled = false;

        MeshRef.inst.preRecordCounter.enabled = true;
        MeshRef.inst.preRecordCounter.text = "";
        #endregion

        // Reset1sBeats()                   // TO DO maybe
    }

    /// <summary>
    /// Get the percentage of the current position of a given sequencer, with start and end point defined by recording-variable (!).
    /// </summary>
    private float SequencerPositionPercentage(Sequencer sequencer, float sequencerPos, RecordData chordRecordData)
    {
        if (sequencerPos < chordRecordData.sequencerLoopStart)
            sequencerPos += sequencer.length;

        float percentage = (sequencerPos - chordRecordData.sequencerLoopStart) / sequencer.length;

        return percentage;  // 0-1
    }

    public Vector3 NextLoopPosition()
    {

        var playerPos = Player.inst.transform.position;
        //float curSeqencerPos = (float)CurSequencer.GetSequencerPosition();
        //var curSequencerPosPercentage = SequencerPositionPercentage(CurSequencer, curSeqencerPos, chordRecordData);
        //var chordStartPos = chordRecordData.start;
        //var chordStartPosPercentage = SequencerPositionPercentage(CurSequencer, chordStartPos, chordRecordData);

        //var position = playerPos + (1 - curSequencerPosPercentage) * LoopData.distancePerRecLoop * Vector3.forward + 
        //    chordStartPosPercentage * LoopData.distancePerRecLoop * Vector3.forward;

        var position = playerPos + LoopData.distancePerRecLoop * Vector3.forward;


        return position;
    }


    // Visuals

    /// <summary>
    /// Instantiate 2 lane surfaces (current chord, looped chord) and keep scaling it by a coroutine upwards from zero until stopped. Writes into chordRecord (!).
    /// </summary>
    /// <param name="chordRecord"></param>
    private void StartSpawnChordObject()
    {
        RecordVisuals.inst.CreateRecordObject(chordRecordData, recordObjects);
    }

    /// <summary>
    /// Stops scaling and sets variables of instantiated recordObjects.
    /// </summary>
    private void StopSpawnChordObject()
    {
        RecordVisuals.inst.StopCreateChordObject(chordRecordData);
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
            float percentage = SequencerPositionPercentage(CurSequencer, curSequencerPos, chordRecordData);

            MeshRef.inst.recordBarFill.fillAmount = percentage;

            yield return null;
        }

    }

}





/// <summary>
/// Class for momentarily storing data of ONE chord and the current record time loop. All data refers to a sequencer.
/// </summary>
public class RecordData
{
    public int fieldID;
    public Sequencer sequencer;
    /// <summary>
    /// Position in the sequencer, measured in sixteenth.
    /// </summary>
    public float start;
    /// <summary>
    /// Position in the sequencer, measured in sixteenth.
    /// </summary>
    public float end;
    public int[] notes;
    /// <summary>
    /// Position in the sequencer, measured in sixteenth.
    /// </summary>
    public float sequencerLoopStart;
    /// <summary>
    /// Extended position in the sequencer (doesn't actually exist), measured in sixteenth. 
    /// </summary>
    public float sequencerLoopEnd_extended;
    public Coroutine scaleRoutine;
    public RecordObject obj;
    public RecordObject loopObj;
}



