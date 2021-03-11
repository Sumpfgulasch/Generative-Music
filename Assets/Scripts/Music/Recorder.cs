using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;
using UnityEngine.Audio;
using System.Linq;
using UnityEngine.InputSystem;

public class Recorder : MonoBehaviour
{
    // Public
    public static Recorder inst;

    [HideInInspector] public bool isRecording = false;
    [HideInInspector] public bool isPreRecording = false;
    [HideInInspector] public int preRecCounter;
    //[HideInInspector] 
    public List<RecordObject> recordObjects;
    public float noteAdd = 0.1f;

    // Private
    public RecordData recording = new RecordData();
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
            recording.start = (float) CurSequencer.GetSequencerPosition();
            recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        }

        // 3.3. Set sequencer loop start?
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
        if (recording.end == -1)
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

        RecordVisuals.inst.DestroyRecordObjects();

        MusicManager.inst.controller.AllNotesOff();
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


    public void OnDebug(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            
        }
    }



    // ------------------------------ Private functions ------------------------------


    // Recording

    /// <summary>
    /// Save start time and notes of the currently played chord.
    /// </summary>
    private void SaveRecordStartData()
    {
        recording.start = (float)CurSequencer.GetSequencerPosition();
        recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        recording.end = -1;
        recording.fieldID = Player.inst.curField.ID;
    }


    /// <summary>
    /// Save end-time of the currently played chord and store it in the sequencer.
    /// </summary>
    private void WriteToSequencer()
    {
        float curPos = (float)CurSequencer.GetSequencerPosition();

        recording.end = curPos;
        float velocity = MusicManager.inst.velocity;


        // 1. Get currently played notes
        var curNotes = AudioHelmHelper.GetAllNotesInRange(CurSequencer, curPos);

        // 2. Get double notes
        var doubleNotes = AudioHelmHelper.DoubleNotes(recording.notes, curNotes);

        // 3. Calc additional notes, to prevent breaking notes
        var addNotes = new List<NoteContainer>();
        foreach (Note doubleNote in doubleNotes)
        {
            // #1: Sequencer note within sequencer bounds, would disrupt and lose remaining note
            if (curPos > doubleNote.start && curPos < doubleNote.end)
            {
                int note = doubleNote.note;
                float start = curPos + noteAdd;
                float end = doubleNote.end;

                // 1.1. Shorten existing note
                print("case 1; before notify");
                //CurSequencer.NotifyNoteEndChanged(doubleNote, end);
                doubleNote.end = recording.start;

                MusicManager.inst.controller.NoteOn(note, velocity, end - start);
                
                

                // 1.2. Add new note
                #region pos percentage
                //float doubleNoteEndPercentage = SequencerPositionPercentage(CurSequencer, end, recording);
                //float curPosPercentage = SequencerPositionPercentage(CurSequencer, curPos, recording);
                #endregion
                CurSequencer.AddNote(note, start, end, velocity);
            }
            // #2 Sequencer note extends over the sequencer bounds, would disrupt and stop playing the remaining note

            // TO DO: klappt nicht (aber #3 und #1 klappen)
            else if (doubleNote.start > doubleNote.end)
            {
                float oldEnd = doubleNote.end;

                // 2.1. Shorten existing sequencer note
                doubleNote.end = recording.start;
                //CurSequencer.NotifyNoteEndChanged(doubleNote, oldEnd);

                // 2.2. Add note for remaining sequencer note?
                float oldEndPercentage = SequencerPositionPercentage(CurSequencer, oldEnd, recording);
                float curPosPercentage = SequencerPositionPercentage(CurSequencer, curPos, recording);
                if (oldEndPercentage > curPosPercentage)
                {
                    int note = doubleNote.note;
                    float start = curPos + 0.01f;
                    float end = oldEnd;

                    var temp = new NoteContainer(note, start, end, velocity);
                    addNotes.Add(temp);
                    //print("fall #2: (1) doubleNote-begin: " + doubleNote.start + ", end: " + doubleNote.end + ", (3) addedNote, start: "  + start + ", end: " + end);
                }
            }
        }

        // #3 Get bridges notes that are NOT being played
        var unplayedBridgeNotes = AudioHelmHelper.UnplayedBridgeNotes(CurSequencer, curPos);




        // 4. Write CURRENTLY RECORDED notes
        foreach (int note in recording.notes)
        {
            //if (counter<=2)
                CurSequencer.AddNote(note, recording.start, recording.end, velocity);
            //print("write; (2); start: " + recording.start + ", end: " + recording.end);
        }

        // 5. Add bridge notes again
        foreach (Note note in unplayedBridgeNotes)  // #3
        {
            CurSequencer.AddNote(note.note, note.start, note.end, velocity);
            //print("fall 3: add bridge notes again");
        }
        foreach(NoteContainer note in addNotes) // #2
        {
            CurSequencer.AddNote(note.note, note.start, note.end, velocity);
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

    public Vector3 NextLoopPosition(RecordObject recordObject)
    {

        //var playerPos = Player.inst.transform.position;
        //float curSeqencerPos = (float)CurSequencer.GetSequencerPosition();
        //var curSequencerPosPercentage = SequencerPositionPercentage(CurSequencer, curSeqencerPos, chordRecordData);
        //var chordStartPos = chordRecordData.start;
        //var chordStartPosPercentage = SequencerPositionPercentage(CurSequencer, chordStartPos, chordRecordData);

        //var position = playerPos + (1 - curSequencerPosPercentage) * LoopData.distancePerRecLoop * Vector3.forward + 
        //    chordStartPosPercentage * LoopData.distancePerRecLoop * Vector3.forward;

        var position = recordObject.transform.position + LoopData.distancePerRecLoop * Vector3.forward;


        return position;
    }


    // Visuals

    /// <summary>
    /// Instantiate 2 lane surfaces (current chord, looped chord) and keep scaling it by a coroutine upwards from zero until stopped. Writes into chordRecord (!).
    /// </summary>
    private void StartSpawnChordObject()
    {
        RecordVisuals.inst.CreateRecordObject(recording, recordObjects);
    }

    /// <summary>
    /// Stops scaling and sets variables of instantiated recordObjects.
    /// </summary>
    private void StopSpawnChordObject()
    {
        RecordVisuals.inst.StopCreateChordObject(recording);
    }

    

    private void DisableRecordBar()
    {
        StopCoroutine(recordBar);
        MeshRef.inst.recordBar.enabled = false;
        MeshRef.inst.recordBarFill.enabled = false;

        var x = new Note();
        
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



