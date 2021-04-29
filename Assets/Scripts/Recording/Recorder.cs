using AudioHelm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    // Public
    public static Recorder inst;

    [HideInInspector] public List<Sequencer> sequencers;
    [HideInInspector] public bool isRecording = false;
    [HideInInspector] public bool isPreRecording = false;
    [HideInInspector] public int preRecCounter;
    public List<ChordObject>[] chordObjects;
    public List<LoopObject> loopObjects;
    public float noteAdd = 0.1f;
    public Recording recording = new Recording();
    [HideInInspector] public float loopStart;
    [HideInInspector] public float loopEnd_extended;

    // Private

    //private Coroutine recordBar;
    private Coroutine checkRecordLength;
    private bool isWaitingToWrite = false;

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

            if (isWaitingToWrite)
                return true;

            return false;
        }
    }

    public bool HasExactlyOneLayer
    {
        get
        {
            int counter = 0;
            foreach(Sequencer sequencer in sequencers)
            {
                var notes = sequencer.GetAllNotes();
                if (notes.Count != 0)
                {
                    counter++;
                }
            }
            print("layer counter: " + counter);

            if (counter == 1)
            {
                return true;
            }
            else if (counter == 0 && isWaitingToWrite)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool HasExactlyOneChord
    {
        get
        {
            int counter = 0;
            foreach (List<ChordObject> chordObjects in chordObjects)
            {
                // Exact 1 chord auf layer
                if (chordObjects.Count == 1)
                {
                    counter++;
                }
                // Mehrere chords auf layer
                else if (chordObjects.Count > 1)
                {
                    return false;
                }
            }

            if (counter == 1)
            {
                return true;
            }

            return false;
        }
    }
    
    private Sequencer CurSequencer { get { return MusicManager.inst.curSequencer; } }
    public int CurLayer { get                                               // scheiﬂe aber mir egal
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

        sequencers = MusicRef.inst.sequencers;
        chordObjects = new List<ChordObject>[MusicManager.inst.maxLayers];
        for (int i=0; i<chordObjects.Length; i++)
            chordObjects[i] = new List<ChordObject>();
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
        UIManager.inst.activeLayerButton.EnableRecordLabel(true);

        if (!Has1stRecord)
        {
            //recordBar = StartCoroutine(RecordingBar());
        }
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
        //var color = VisualController.inst.nonRecordColor;
        //MeshRef.inst.recordText.enabled = false;
        //MeshRef.inst.recordImage.enabled = false;
        //MeshRef.inst.preRecordCounter.enabled = false;
        UIManager.inst.activeLayerButton.EnableRecordLabel(false);
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
        print("hasExactlyOneLayer: " + HasExactlyOneLayer);
        if (!Has1stRecord)
        {
            DestroyLoopObjects();
        }
        // clear highlighted fieldSurface und highlightSurface
        foreach (ChordObject recordObj in chordObjects[layer])
        {
            if (recordObj.isPlaying)
            {
                Player.inst.curFieldSet[recordObj.fieldID].ActiveRecords--;
            }
        }
        RecordVisuals.inst.DestroyAllChordObjects(layer);

        UIOps.inst.EnableRecordedTrackImage(layer, false);

        MusicManager.inst.controller.AllNotesOff();
    }


    /// <summary>
    /// Delete over time. Make transparent and delete after that.
    /// </summary>
    /// <param name="chordObj"></param>
    /// <returns></returns>
    public IEnumerator DeleteRoutine(ChordObject chordObj)
    {
        chordObj.isBeingDeleted = true;

        float timer = 0;
        float maxTime = UIManager.inst.deleteRecordTime;
        bool hasExactlyOneChord = HasExactlyOneChord;

        // 1. Fade out chordObject (& loopObject)
        while (timer < maxTime)
        {
            var curveValue = MeshRef.inst.deleteChordCurve.Evaluate(timer / maxTime);
            var chordColorLerp = Color.Lerp(Color.black * 0, MeshRef.inst.recordColor, curveValue);


            // chordObject
            chordObj.meshRenderer.material.color = chordColorLerp;

            // loopObject
            if (hasExactlyOneChord)
            {
                foreach(LoopObject loopObject in loopObjects)
                {
                    loopObject.meshRenderer.material.color = chordColorLerp;
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }

        // 2. Destroy
        RemoveRecord(chordObj);
    }

    /// <summary>
    /// Remove a single chord. Maybe also LoopObject. Reset every necessary UI stuff.
    /// </summary>
    /// <param name="chordObj"></param>
    private void RemoveRecord(ChordObject chordObj)
    {
        // 1. clear notes
        foreach (int note in chordObj.notes)
        {
            chordObj.sequencer.RemoveNotesInRange(note, chordObj.start, chordObj.end);
            chordObj.sequencer.NoteOff(note);
        }

        // 2. gameObject & list
        RecordVisuals.inst.DestroyChordObject(chordObj);

        // 3. UI
        if (chordObj.sequencer.GetAllNotes().Count == 0)
        {
            UIOps.inst.EnableRecordedTrackImage(chordObj.trackLayer, false);
        }

        // 4. field.activeChords
        if (chordObj.isPlaying)
            Player.inst.curFieldSet[chordObj.fieldID].ActiveRecords--;

        // 5. LoopObject
        if (!Has1stRecord)
        {
            DestroyLoopObjects();
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
    /// Save data of currently one recording chord to the RECORDING-variable (start time, notes, fieldID, sequencer, quantizeOffset).
    /// </summary>
    private void SaveRecordStartData()
    {
        print("OnStartField");

        float sequencerPos = (float) CurSequencer.GetSequencerPosition();

        // 1. Start & quantize offset
        if (MusicManager.inst.quantize)
        {
            float quantize = Quantize(sequencerPos);
            recording.start = quantize;

            // hack f¸r sequencerPos bei sequencer.length: quantize 0 oder 32 (sequencer.length)
            if (quantize == 0 && sequencerPos > CurSequencer.length / 2)
            {
                quantize = CurSequencer.length;
            }

            recording.startQuantizeOffset = quantize - sequencerPos;
        }
        else
        {
            recording.start = sequencerPos;
            recording.startQuantizeOffset = 0;
        }

        // 2. End, notes, ID, sequencer
        recording.notes = MusicManager.inst.curChord.DeepCopy().notes;
        recording.fieldID = Player.inst.curField.ID;
        recording.sequencer = CurSequencer;
        recording.isRunning = true;



        // 3. LoopStart object?
        if (!Has1stRecord)
        {
            loopStart = recording.start;
            loopEnd_extended = loopStart + CurSequencer.length;

            var obj = MeshRef.inst.loopObject;
            var parent = MeshRef.inst.recordObj_parent;
            var position = Player.inst.transform.position;

            LoopObject.Create(obj, parent, position, loopStart, loopEnd_extended, loopObjects);
        }
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

                recording.end = quantize;
                recording.endQuantizeOffset = quantize - curPos;

                // Special case: very short notes
                if (recording.end == recording.start)
                {
                    recording.end = (recording.end + MusicManager.inst.quantizeStep) % recording.sequencer.length;
                }
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



        // 3. IF THERE ARE CURRENTLY EXISTING NOTES: Calc additional notes, to prevent breaking notes
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

                // 1.1. Shorten existing note (1/3)
                if (recording.start != doubleNote.start)    // sonst macht das keinen sinn
                {
                    doubleNote.end = recording.start;
                }

                // Note on
                MusicManager.inst.controller.NoteOn(note, velocity, end - start);

                // 1.2. Add new note for remaining part (== 3/3; recording == 2/3, existing note == 1/3)
                usualNotes.Add(new NoteContainer(note, start, end, velocity));

                //print("existing note, case #1; remaining usual note gets added (3/3)");
            }

            // #2 Existing note extends over the sequencer bounds, would disrupt and stop playing the remaining note
            else if (doubleNote.start > doubleNote.end)
            {
                float oldEnd = doubleNote.end;

                // 2.1. Shorten existing sequencer note
                if (recording.start != doubleNote.start)    // sonst macht das keinen sinn
                {
                    if (recording.start != 0)
                        doubleNote.end = recording.start;
                    else
                        doubleNote.end = recording.sequencer.length - 0.01f;
                }

                // 2.2. Add note for remaining sequencer note?
                //float oldEndPercentage = SequencerPositionPercentage(recording.sequencer, oldEnd, recording.loopStart);
                //float curPosPercentage = SequencerPositionPercentage(recording.sequencer, recording.end, recording.loopStart);
                int note = doubleNote.note;
                float start = recording.end;
                float end = oldEnd;

                var temp = new NoteContainer(note, start, end, velocity);
                remainingNotes.Add(temp); // dont add now because it would be overwritten by usual notes; has to be added at last
                MusicManager.inst.controller.NoteOn(note, velocity, end - start);

                //print("existing note, case #2; remaining undefined note gets added (3/3)");
            }
        }


        var recordCopy = recording.DeepCopy(); // DeepCopy, because otherwise wrong data at later point



        // WAIT to add notes; otherwise notes will disrupt unintendedly
        if (recordCopy.endQuantizeOffset > 0)
        {
            float delay = recordCopy.endQuantizeOffset * LoopData.timePerSixteenth;

            isWaitingToWrite = true;

            yield return new WaitForSeconds(delay);
        }
        isWaitingToWrite = false;



        // #3 Get bridges notes that are NOT being played
        curPos = (float)recordCopy.sequencer.GetSequencerPosition();
        var unplayedBridgeNotes = AudioHelmHelper.UnplayedBridgeNotes(CurSequencer, curPos);

        // cur douplicate notes, at the time of the start
        var curNotes_quantize = AudioHelmHelper.GetCurrentNotes(recordCopy.sequencer, recordCopy.start);
        var curDoubleNotes_quantize = AudioHelmHelper.DoubleNotes(recordCopy.notes, curNotes_quantize);


        // 5. Add remaining usual notes (#1; 3/3)
        foreach (NoteContainer note in usualNotes)
        {
            AudioHelmHelper.RemoveIdenticalStartNotes(note, curDoubleNotes_quantize, recordCopy.sequencer);

            recordCopy.sequencer.AddNote(note.note, note.start, note.end, velocity);
        }


        // 4. Write CURRENTLY RECORDED notes
        foreach (int noteNote in recordCopy.notes)
        {
            var note = new Note { note = noteNote, start = recordCopy.start, end = recordCopy.end, velocity = velocity, parent = null };
            //note.parent = null;

            if (note.IsUnplayedBridgeNote(curPos))
            {
                unplayedBridgeNotes.Add(note);
            }
            else
            {
                AudioHelmHelper.RemoveIdenticalStartNotes(note, curDoubleNotes_quantize, recordCopy.sequencer);

                // add to sequencer
                recordCopy.sequencer.AddNote(noteNote, recordCopy.start, recordCopy.end, velocity);
            }
        }



        // 5. Add bridge notes again
        foreach (NoteContainer note in remainingNotes) // #2: remaining notes of case #2
        {
            if (note.IsUnplayedBridgeNote(curPos))
            {
                var helmNote = new Note { note = note.note, start = note.start, end = note.end };
                unplayedBridgeNotes.Add(helmNote);
            }
            else
            {
                AudioHelmHelper.RemoveIdenticalStartNotes(note, curDoubleNotes_quantize, recordCopy.sequencer);

                recordCopy.sequencer.AddNote(note.note, note.start, note.end, velocity);
            }
        }
        foreach (Note note in unplayedBridgeNotes)  // #3: unplayed bridge notes
        {
            AudioHelmHelper.RemoveIdenticalStartNotes(note, curDoubleNotes_quantize, recordCopy.sequencer);

            recordCopy.sequencer.AddNote(note.note, note.start, note.end, velocity);
        }

        yield return null;
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

        MeshRef.inst.preRecordCounter.enabled = true;
        MeshRef.inst.preRecordCounter.text = "";
        #endregion

        // Reset1sBeats()                   // TO DO maybe
    }

    /// <summary>
    /// Get the percentage of the current position of a given sequencer, with end point == sequencer.Length.
    /// </summary>
    private float SequencerPositionPercentage(Sequencer sequencer, float sequencerPos)
    {
        //if (sequencerPos < sequencerLoopStart)
        //    sequencerPos += sequencer.length;

        //float percentage = (sequencerPos - sequencerLoopStart) / sequencer.length;

        float percentage = sequencerPos / sequencer.length;

        return percentage;  // 0-1
    }

    

/// <summary>
/// Return the Vector3 position of a to-be-douplicated recordObject (with the given sequencer data). Calc complicated, because otherwise would become inprecise after time.
/// </summary>
/// <param name="recordObject"></param>
/// <param name="sequencer">The relevant sequencer.</param>
/// <param name="recordObj_startPos">The start position of the midi note in the sequencer.</param>
/// <param name="recordObj_loopStart">The loop start position in the sequencer</param>
/// <returns></returns>
    public Vector3 NextLoopPosition(Sequencer sequencer, float recordObj_startPos)
    {
        //var playerPos = Player.inst.transform.position;
        //float curSeqencerPos = (float)sequencer.GetSequencerPosition();

        //var curSequencerPosPercentage = SequencerPositionPercentage(sequencer, curSeqencerPos, recordObj_loopStart);
        //var chordStartPos = recordObj_startPos;
        //var chordStartPosPercentage = SequencerPositionPercentage(sequencer, chordStartPos, recordObj_loopStart);

        //var position = playerPos + 
        //    (1 - curSequencerPosPercentage) * LoopData.distancePerRecLoop * Vector3.forward +
        //    chordStartPosPercentage * LoopData.distancePerRecLoop * Vector3.forward;

        var playerPos = Player.inst.transform.position;
        float curSequencerPos = (float)sequencer.GetSequencerPosition();

        var sequencer_curPosPercentage =  SequencerPositionPercentage(sequencer, curSequencerPos);
        var chordObj_startPosPercentage = SequencerPositionPercentage(sequencer, recordObj_startPos);

        var position = playerPos +
            (1 - sequencer_curPosPercentage) * LoopData.distancePerRecLoop * Vector3.forward +
            chordObj_startPosPercentage * LoopData.distancePerRecLoop * Vector3.forward;

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
            return 0;
        }

        return closestStep;
    }




    // Visuals

    /// <summary>
    /// Instantiate 2 lane surfaces (current chord, looped chord) and keep scaling it by a coroutine upwards from zero until stopped. Writes into RECORDING variable (!).
    /// </summary>
    private void StartSpawnChordObject()
    {
        RecordVisuals.inst.CreateChordObjectTwice(recording, chordObjects, CurLayer);
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

    

    private void DestroyLoopObjects()
    {
        print("call destroy LoopObj");
        foreach(LoopObject loopObject in loopObjects)
        {
            print("Destroy");
            Destroy(loopObject.obj);
        }
        loopObjects = new List<LoopObject>();

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
    public ChordObject obj;
    public ChordObject loopObj;
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



