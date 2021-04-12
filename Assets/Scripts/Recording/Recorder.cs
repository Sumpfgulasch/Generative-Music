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
    public Recording recording = new Recording();
    [HideInInspector] public float loopStart;
    [HideInInspector] public float loopEnd_extended;

    // Private

    private Coroutine recordBar, checkRecordLength;


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
            for (int i = 0; i < sequencers.Count; i++)
            {
                if (CurSequencer == sequencers[i])
                    return i;
            }
            Debug.LogError("curLayer not found");
            return 0;

            //return UIManager.inst.activeLayerButton.layer;
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
            SaveRecordStartData();
            StartSpawnChordObject();
        }

        // 3.3. Set sequencer loop start?
        if (!Has1stRecord)
        {
            loopStart = recording.start;
            loopEnd_extended = loopStart + CurSequencer.length;
        }

        // 4. Max record length
        checkRecordLength = StartCoroutine(MaxRecordLength());
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
        if (recording.isRunning)
        {
            WriteToSequencer();
            StopSpawnChordObject();
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

        // Max record length
        StopCoroutine(checkRecordLength);
    }



    /// <summary>
    /// Clear all notes from a given sequencer. Disable recordBar maybe.
    /// </summary>
    public void ClearLayer(int layer)
    {
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

        UIOps.inst.EnableRecordedTrackImage(layer, false);

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
            UIOps.inst.EnableRecordedTrackImage(recordObj.trackLayer, false);
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
    /// Save data of currently one recording chord to the RECORDING-variable (start time, notes, fieldID, sequencer, quantizeOffset).
    /// </summary>
    private void SaveRecordStartData()
    {
        float sequencerPos = (float)CurSequencer.GetSequencerPosition();

        // 1. Start & quantize offset
        if (MusicManager.inst.quantize)
        {
            float quantize = Quantize(sequencerPos);
            recording.start = quantize;

            // hack für sequencerPos bei sequencer.length
            if (quantize == 0 && sequencerPos > CurSequencer.length/2)
                quantize = CurSequencer.length;

            recording.startQuantizeOffset = quantize - sequencerPos;
        }
        else
        {
            recording.start = sequencerPos;
            recording.startQuantizeOffset = 0;
        }

        // 2. End, notes, ID, sequencer
        recording.end = -1;                             // hack; nicht mehr nötig
        recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        recording.fieldID = Player.inst.curField.ID;
        recording.sequencer = CurSequencer;
        recording.isRunning = true;
    }

    
    private void WriteToSequencer()
    {
        if (recording.isRunning)
        {
            StartCoroutine(WriteToSequencer_delayed());
        }
    }

    /// <summary>
    /// Write into recording (end-time of the currently played chord), recalculate notes to prevent weird stuff and set notes to sequencer.
    /// </summary>
    private IEnumerator WriteToSequencer_delayed()
    {
        float curPos = (float)CurSequencer.GetSequencerPosition();
        float velocity = MusicManager.inst.velocity;
        var curNotes = AudioHelmHelper.GetCurrentNotes(CurSequencer, curPos);
        var doubleNotes = AudioHelmHelper.DoubleNotes(recording.notes, curNotes);

        // 1. Write to recording
        // 2. Quantize?
        if (MusicManager.inst.quantize)
        {
            // Legato: quantize recording.end, too
            if (Player.inst.actionState == Player.ActionState.Play)
            {
                float quantize = Quantize(curPos);
                recording.endQuantizeOffset = quantize - curPos;
                recording.end = Quantize(curPos);
            }
            // Staccato: Dont quantize recording.end
            else
            {
                // get the time the chord was pressed
                recording.end = (curPos + recording.startQuantizeOffset).Modulo(recording.sequencer.length);
                recording.endQuantizeOffset = recording.startQuantizeOffset;
            }
        }
        else
        {
            recording.end = curPos;
        }


        // Special case: recording exceeds self
        if (recording.endExceedsStart)
        {
            recording.end = (recording.start - 0.01f).Modulo(recording.sequencer.length);
            recording.endExceedsStart = false;

            foreach (int note in recording.notes)
            {
                recording.sequencer.NoteOn(note, velocity);
            }
        }

        // 3. Calc additional notes, to prevent breaking notes
        var usualNotes = new List<NoteContainer>();
        var remainingNotes = new List<NoteContainer>();
        foreach (Note doubleNote in doubleNotes)
        {
            // #1: Existing note within bounds (Sequencer note.start < note.end), curNote plays within sequencer.chord (curNote > start); would disrupt and delete remaining note
            if (recording.end > doubleNote.start && recording.end < doubleNote.end)
            {
                int note = doubleNote.note;
                float start = recording.end + noteAdd; // note add == 0 currently
                float end = doubleNote.end;

                // 1.1. Shorten existing note
                //if (recording.start != doubleNote.start)    // sonst macht das keinen sinn
                //{
                //    doubleNote.end = recording.start;
                //}
                
                // Note on
                MusicManager.inst.controller.NoteOn(note, velocity, end - start);
                
                // 1.2. Add new note for remaining part (== 3/3; recording == 2/3, existing note == 1/3)
                usualNotes.Add(new NoteContainer(note, start, end, velocity));
            }

            // #2 Existing note extends over the sequencer bounds, would disrupt and stop playing the remaining note
            else if (doubleNote.start > doubleNote.end)
            {
                float oldEnd = doubleNote.end;

                // 2.1. Shorten existing sequencer note
                //if (recording.start != doubleNote.start)    // sonst macht das keinen sinn
                //{
                //    if (recording.start != 0)
                //        doubleNote.end = recording.start;
                //    else
                //        doubleNote.end = recording.sequencer.length - 0.01f;
                //}

                // 2.2. Add note for remaining sequencer note?
                float oldEndPercentage = SequencerPositionPercentage(recording.sequencer, oldEnd, recording.loopStart);
                float curPosPercentage = SequencerPositionPercentage(recording.sequencer, recording.end, recording.loopStart);
                int note = doubleNote.note;
                float start = recording.end;
                float end = oldEnd;

                var temp = new NoteContainer(note, start, end, velocity);
                remainingNotes.Add(temp); // dont add now because it would be overwritten by usual notes; has to be added at last
                MusicManager.inst.controller.NoteOn(note, velocity, end - start);
            }
        }

        
        var recordCopy = recording.DeepCopy(); // DeepCopy, because otherwise wrong data at later point



        // WAIT to add notes; otherwise notes will disrupt unintendedly
        if (recordCopy.endQuantizeOffset > 0)
        {
            float delay = recordCopy.endQuantizeOffset * LoopData.timePerSixteenth;

            yield return new WaitForSeconds(delay);
        }



        // #3 Get bridges notes that are NOT being played
        curPos = (float)recordCopy.sequencer.GetSequencerPosition();
        var unplayedBridgeNotes = AudioHelmHelper.UnplayedBridgeNotes(CurSequencer, curPos);


        //// 5. Add remaining usual notes (#1)
        //foreach (NoteContainer note in usualNotes)
        //{
        //    note.TryRemoveIdenticalStartNotes(doubleNotes, recordCopy.sequencer);

        //    recordCopy.sequencer.AddNote(note.note, note.start, note.end, velocity);
        //}


        // 4. Write CURRENTLY RECORDED notes
        foreach (int noteNote in recordCopy.notes)
        {
            var note = new Note { note = noteNote, start = recordCopy.start, end = recordCopy.end };

            if (note.IsUnplayedBridgeNote(curPos))
            {
                unplayedBridgeNotes.Add(note);
            }
            else
            {
                // delete doubleNotes with the same length
                for (int i = 0; i < doubleNotes.Count; i++)
                {
                    if (note.start == doubleNotes[i].start)
                    {
                        recordCopy.sequencer.RemoveNote(doubleNotes[i]);
                    }
                }

                //note.TryRemoveIdenticalStartNotes(doubleNotes, recordCopy.sequencer);

                // add to sequencer
                recordCopy.sequencer.AddNote(noteNote, recordCopy.start, recordCopy.end, velocity);
            }
        }



        //// 5. Add bridge notes again
        //foreach (NoteContainer note in remainingNotes) // #2: remaining notes of case #2
        //{
        //    if (note.IsUnplayedBridgeNote(curPos))
        //    {
        //        var helmNote = new Note { note = note.note, start = note.start, end = note.end };
        //        unplayedBridgeNotes.Add(helmNote);
        //    }
        //    else
        //    {
        //        note.TryRemoveIdenticalStartNotes(doubleNotes, recordCopy.sequencer);

        //        recordCopy.sequencer.AddNote(note.note, note.start, note.end, velocity);
        //    }
        //}
        //foreach (Note note in unplayedBridgeNotes)  // #3: unplayed bridge notes
        //{
        //    note.TryRemoveIdenticalStartNotes(doubleNotes, recordCopy.sequencer);

        //    recordCopy.sequencer.AddNote(note.note, note.start, note.end, velocity);
        //}

        yield return null;
    }



    /// <summary>
    /// Guarantee max record length. If currently recorded obj == sequencer.length, then stop spawning (WriteToSequencer and StopSpawnChordObject).
    /// </summary>
    /// <returns></returns>
    private IEnumerator MaxRecordLength()
    {
        while (true)
        {
            if (recording.isRunning)
            {
                if (recording.loopObj.transform.position.z <= Player.inst.transform.position.z)
                {
                    recording.endExceedsStart = true;

                    WriteToSequencer();
                    StopSpawnChordObject();
                }
            }
            yield return null;
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
    private float Quantize(float sequencerPos)
    {
        float closestStep = 100f;
        float closestStepDistance = 100f;
        foreach (float step in MusicManager.inst.quantizeSteps)
        {
            float curStepDistance = Mathf.Abs(step - sequencerPos);

            if (curStepDistance < closestStepDistance)
            {
                closestStep = step;
                closestStepDistance = Mathf.Abs(closestStep - sequencerPos);
            }
        }

        if (closestStep == 100)
        {
            Debug.LogError("Quantize closest step == 100");
            MusicManager.inst.quantize = false;
            return sequencerPos;
        }

        return closestStep;
    }




    // Visuals

    /// <summary>
    /// Instantiate 2 lane surfaces (current chord, looped chord) and keep scaling it by a coroutine upwards from zero until stopped. Writes into RECORDING variable (!).
    /// </summary>
    private void StartSpawnChordObject()
    {
        RecordVisuals.inst.CreateRecordObjectTwice(recording, recordObjects, CurLayer);
        UIOps.inst.EnableRecordedTrackImage(CurLayer, true);
    }

    /// <summary>
    /// Stops scaling and sets variables of instantiated recordObjects.
    /// </summary>
    private void StopSpawnChordObject()
    {
        if (recording.isRunning)
        {
            RecordVisuals.inst.StopCreateChordObject(recording);
            recording.isRunning = false;
        }
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
    /// <summary>
    /// In sixteenth.
    /// </summary>
    public float startQuantizeOffset;
    public float endQuantizeOffset;
    public bool isRunning;
    public bool endExceedsStart;


    /// <summary>
    /// Get the length of one record / chord, in sixteenth.
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
            return (end - start); // in sixteenth
        }
    }


    public Recording()
    {

    }
    public Recording(Sequencer sequencer, float start, float end, int[]notes, float startQuantizeOffset, float endQuantizeOffset)
    {
        this.sequencer = sequencer;
        this.start = start;
        this.end = end;
        this.notes = notes;
        this.startQuantizeOffset = startQuantizeOffset;
        this.endQuantizeOffset = endQuantizeOffset;
    }

    /// <summary>
    /// INCOMPLETE !!!
    /// </summary>
    /// <returns></returns>
    public Recording DeepCopy()
    {
        
        int[] newNotes = new int[notes.Length];
        for (int i = 0; i < notes.Length; i++)
            newNotes[i] = notes[i];

        return new Recording(sequencer, start, end, newNotes, startQuantizeOffset, endQuantizeOffset);
    }
}



