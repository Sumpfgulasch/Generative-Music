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

    [HideInInspector] public List<Sequencer> sequencers;
    [HideInInspector] public bool isRecording = false;
    [HideInInspector] public bool isPreRecording = false;
    [HideInInspector] public int preRecCounter;
    public List<RecordObject>[] recordObjects;
    public float noteAdd = 0.1f;

    // Private
    public Recording recording = new Recording();
    private Coroutine recordBar;


    // Properties
    public bool Has1stRecord
    {
        get
        {
            foreach (Sequencer sequencer in sequencers)
            {
                var notes = sequencer.GetAllNotes();
                if (notes.Count != 0)
                    return true;
            }
            return false;
        }
    }
    
    private Sequencer CurSequencer { get { return MusicManager.inst.curSequencer; } }
    public int CurLayer { get                                               // scheiße aber mir egal
        { 
            for (int i=0; i<sequencers.Count; i++)
            {
                if (CurSequencer == sequencers[i])
                    return i;
            }
            Debug.LogError("curLayer not found");
            return 0; 
        } 
    }



    void Start()
    {
        inst = this;

        MeshRef.inst.recordText.enabled = false;
        MeshRef.inst.recordBar.enabled = false;
        MeshRef.inst.recordImage.enabled = false;
        MeshRef.inst.preRecordCounter.enabled = false;
        MeshRef.inst.recordBarFill.enabled = false;

        sequencers = MusicRef.inst.sequencers;
        recordObjects = new List<RecordObject>[MusicManager.inst.maxLayers];
        for (int i=0; i<recordObjects.Length; i++)
            recordObjects[i] = new List<RecordObject>();
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
            recording.loopStart = (float)CurSequencer.GetSequencerPosition();
            recording.loopEnd_extended = recording.loopStart + CurSequencer.length;
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
    public void ClearLayer(int layer)
    {
        recording = new Recording();

        // 1. Clear sequencer
        sequencers[layer].Clear();

        // 2. UI & objects
        if (!Has1stRecord)
        {
            DisableRecordBar();
        }
        // clear highlighted fieldSurface und highlightSurface
        foreach (RecordObject recordObj in recordObjects[layer])
        {
            if (recordObj.isPlaying)
            {
                Player.inst.curFieldSet[recordObj.fieldID].ActiveRecords--;
            }
        }
        RecordVisuals.inst.DestroyAllRecordObjects(layer);

        UIOps.inst.EnableRecordedTrackImage(false);

        MusicManager.inst.controller.AllNotesOff();
    }


    public void RemoveRecord(RecordObject recordObj)
    {
        // 1. clear notes
        foreach (int note in recordObj.notes)
        {
            recordObj.sequencer.RemoveNotesInRange(note, recordObj.start, recordObj.end);
            recordObj.sequencer.NoteOff(note);
        }

        // 2. gameObject & list
        RecordVisuals.inst.DestroyRecordObject(recordObj);

        // 3. UI
        if (recordObj.sequencer.GetAllNotes().Count == 0)
        {
            UIOps.inst.EnableRecordedTrackImage(false);
        }

        // 4. field.activeChords
        if (recordObj.isPlaying)
            Player.inst.curFieldSet[recordObj.fieldID].ActiveRecords--;
        
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
        float sequencerPos = (float)CurSequencer.GetSequencerPosition();

        // Quantization?
        if (MusicManager.inst.quantize)
        {
            float quantize = Quantize(sequencerPos);
            recording.start = quantize;

            if (quantize == 0 && sequencerPos > CurSequencer.length/2)
                quantize = CurSequencer.length;
            recording.quantizeOffset = quantize - sequencerPos;
            //if (quantize == 0)
            //    recording.quantizeOffset = ExtensionMethods.Modulo(quantize - sequencerPos, CurSequencer.length);
            print("START; sequencerPos: " + sequencerPos + ", quantize: " + quantize + ", offset: " + recording.quantizeOffset);
        }
        else
        {
            recording.start = sequencerPos;
            recording.quantizeOffset = 0;
        }

        recording.end = -1;
        recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        recording.fieldID = Player.inst.curField.ID;
        recording.sequencer = CurSequencer;
    }

    
    private void WriteToSequencer()
    {
        StartCoroutine(WriteToSequencer_delayed(1f));
    }

    /// <summary>
    /// Write into recording (end-time of the currently played chord), recalculate notes to prevent weird stuff and set notes to sequencer.
    /// </summary>
    private IEnumerator WriteToSequencer_delayed(float delay)
    {
        float curPos = (float)CurSequencer.GetSequencerPosition();
        float velocity = MusicManager.inst.velocity;
        var curNotes = AudioHelmHelper.GetAllNotesInRange(CurSequencer, curPos);
        var doubleNotes = AudioHelmHelper.DoubleNotes(recording.notes, curNotes);

        // 1. Write to recording
        recording.end = curPos;

        // 2. Quantize?
        if (MusicManager.inst.quantize)
        {
            curPos = Quantize(curPos);
            recording.end = curPos;
            if (recording.end == recording.start)
            {
                recording.end = (recording.end + MusicManager.inst.quantizeStep) % CurSequencer.length;
                Debug.Log("START == END");
            }
        }

        // 3. Calc additional notes, to prevent breaking notes
        var usualNotes = new List<NoteContainer>();
        var bridgeNotes = new List<NoteContainer>();
        foreach (Note doubleNote in doubleNotes)
        {
            // #1: Sequencer note.start < note.end, curNote plays within sequencer.chord; would disrupt and delete remaining note
            if (curPos > doubleNote.start && curPos < doubleNote.end)
            {
                int note = doubleNote.note;
                float start = curPos + noteAdd;
                float end = doubleNote.end;

                // 1.1. Shorten existing note
                doubleNote.end = recording.start;

                //if (!MusicManager.inst.quantize)
                    MusicManager.inst.controller.NoteOn(note, velocity, end - start);
                print("#1: add note for the end, that would be deleted; note: " + note);
                
                // 1.2. Add new note
                #region pos percentage
                //float doubleNoteEndPercentage = SequencerPositionPercentage(CurSequencer, end, recording);
                //float curPosPercentage = SequencerPositionPercentage(CurSequencer, curPos, recording);
                #endregion
                //CurSequencer.AddNote(note, start, end, velocity);
                usualNotes.Add(new NoteContainer(note, start, end, velocity));
            }

            // #2 Sequencer note extends over the sequencer bounds, would disrupt and stop playing the remaining note
            else if (doubleNote.start > doubleNote.end)
            {
                float oldEnd = doubleNote.end;

                // 2.1. Shorten existing sequencer note
                doubleNote.end = recording.start;

                // 2.2. Add note for remaining sequencer note?
                float oldEndPercentage = SequencerPositionPercentage(CurSequencer, oldEnd, recording.loopStart);
                float curPosPercentage = SequencerPositionPercentage(CurSequencer, curPos, recording.loopStart);
                if (oldEndPercentage > curPosPercentage)
                {
                    int note = doubleNote.note;
                    float start = curPos + 0.01f;
                    float end = oldEnd;

                    var temp = new NoteContainer(note, start, end, velocity);
                    bridgeNotes.Add(temp); // dont add now because it would be overwritten by usual notes; has to be added at last
                }
            }
        }

        // #3 Get bridges notes that are NOT being played
        var unplayedBridgeNotes = AudioHelmHelper.UnplayedBridgeNotes(CurSequencer, curPos);

        
        var recordCopy = recording.DeepCopy(); // DeepCopy, because otherwise wrong data at later point

        yield return new WaitForSeconds(delay);


        // 5. Add usual notes (#1)          // to do maybe: before 4.?
        foreach (NoteContainer note in usualNotes)
        {
            //if (CurSequencer.note)
            CurSequencer.AddNote(note.note, note.start, note.end, velocity);
        }


        // 4. Write CURRENTLY RECORDED notes
        foreach (int note in recordCopy.notes)
        {
            CurSequencer.AddNote(note, recordCopy.start, recordCopy.end, velocity);
        }

        

        // 5. Add bridge notes again
        foreach (Note note in unplayedBridgeNotes)  // #3
        {
            CurSequencer.AddNote(note.note, note.start, note.end, velocity);
        }
        foreach(NoteContainer note in bridgeNotes) // #2
        {
            CurSequencer.AddNote(note.note, note.start, note.end, velocity);
        }

        yield return null;
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
    private float SequencerPositionPercentage(Sequencer sequencer, float sequencerPos, float sequencerLoopStart)
    {
        if (sequencerPos < sequencerLoopStart)
            sequencerPos += sequencer.length;

        float percentage = (sequencerPos - sequencerLoopStart) / sequencer.length;

        return percentage;  // 0-1
    }

    

/// <summary>
/// Return the Vector3 position of a to-be-douplicated recordObject (with the given sequencer data).
/// </summary>
/// <param name="recordObject"></param>
/// <param name="sequencer">The relevant sequencer.</param>
/// <param name="recordObj_startPos">The start position of the midi note in the sequencer.</param>
/// <param name="recordObj_loopStart">The loop start position in the sequencer</param>
/// <returns></returns>
    public Vector3 NextLoopPosition(Sequencer sequencer, float recordObj_startPos, float recordObj_loopStart)
    {

        var playerPos = Player.inst.transform.position;
        float curSeqencerPos = (float)sequencer.GetSequencerPosition();

        var curSequencerPosPercentage = SequencerPositionPercentage(sequencer, curSeqencerPos, recordObj_loopStart);
        var chordStartPos = recordObj_startPos;
        var chordStartPosPercentage = SequencerPositionPercentage(sequencer, chordStartPos, recordObj_loopStart);

        var position = playerPos + 
            (1 - curSequencerPosPercentage) * LoopData.distancePerRecLoop * Vector3.forward +
            chordStartPosPercentage * LoopData.distancePerRecLoop * Vector3.forward;


        //var position = recordObject.transform.position + LoopData.distancePerRecLoop * Vector3.forward; // theoretisch korrekt, mit der Zeit aber asynchron


        return position;
    }


    /// <summary>
    /// Quantize a sequencer position. Sequencer and precision is defined by MusicManager.inst.quantization / quantizeSteps.
    /// </summary>
    /// <param name="sequencerPos"></param>
    /// <returns></returns>
    private int Quantize(float sequencerPos)
    {
        int closestStep = 100;
        float closestStepDistance = 100f;
        foreach (int step in MusicManager.inst.quantizeSteps)
        {
            float curStepDistance = Mathf.Abs(step - sequencerPos);

            if (curStepDistance < closestStepDistance)
            {
                closestStep = step;
                closestStepDistance = Mathf.Abs(closestStep - sequencerPos);
            }

            //print("step: " + step + ", sequencerPos: " + sequencerPos + ", curStepDistance: " + curStepDistance + ", closestStep: " + closestStep);
        }

        return closestStep;
    }




    // Visuals

    /// <summary>
    /// Instantiate 2 lane surfaces (current chord, looped chord) and keep scaling it by a coroutine upwards from zero until stopped. Writes into chordRecord (!).
    /// </summary>
    private void StartSpawnChordObject()
    {
        int layer = UIManager.inst.activeLayerButton.layer;         // Unschön; sollte Variable haben
        RecordVisuals.inst.CreateRecordObjectTwice(recording, recordObjects, layer);

        UIOps.inst.EnableRecordedTrackImage(true);
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
            float percentage = SequencerPositionPercentage(CurSequencer, curSequencerPos, recording.loopStart);

            MeshRef.inst.recordBarFill.fillAmount = percentage;

            yield return null;
        }

    }

}





/// <summary>
/// Class for momentarily storing data of ONE chord and the current record time loop. All data refers to a sequencer.
/// </summary>
public class Recording
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
    public float loopStart;
    /// <summary>
    /// Extended position in the sequencer (doesn't actually exist), measured in sixteenth. 
    /// </summary>
    public float loopEnd_extended;
    public Coroutine scaleRoutine;
    public RecordObject obj;
    public RecordObject loopObj;
    public float quantizeOffset;


    public Recording()
    {

    }
    public Recording(float start, float end, int[]notes)
    {
        this.start = start;
        this.end = end;
        this.notes = notes;
    }

    public Recording DeepCopy()
    {
        int[] newNotes = new int[this.notes.Length];
        for (int i = 0; i < notes.Length; i++)
            newNotes[i] = notes[i];

        return new Recording(start, end, newNotes);
    }
}



