using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Linq;
using AudioHelm;

public class MusicManager : MonoBehaviour
{
    public static MusicManager inst;

    // Public properties
    [Header("Contraints")]
    [Tooltip("Number of chord degrees that make up the edgeParts in the beginning.")]
    public int[] intervals = new int[] { 1, 3, 5 };
    public int chordDegrees = 3;
    public int toneRange_startNote = 41;
    public int toneRange = 24;
    public int maxLayers = 5;
    public float shortNotes_minPlayTime = 0.3f;
    public int maxEdgePitchIntervalRange = 14;
    [Range(0, 1f)]
    public float velocity = 0.1f;

    [Header("Record")]
    public bool firstRecordDelay;
    public bool quantize = true;
    public int[] quantizationChoices = new int[] { 16, 8, 4 };
    public enum Precision { fine, middle, rough};
    public Precision curPrecision = Precision.fine;

    [HideInInspector] public List<float> quantizeSteps;
    [HideInInspector] public float quantizeStep;

    [HideInInspector] public Chord curChord;
    [HideInInspector] public Chord lastChord;
    [HideInInspector] public int curBeat;
    [HideInInspector] public int curLoop = -1;
    [HideInInspector] public int recLoops = 2;

    // Private variables

    private float minPitch, maxPitch;
    private float curPitch = 0;
    [HideInInspector] public HelmController controller;
    [HideInInspector] public Sequencer curSequencer;
    private int curWaitStartBeat;
    private int curBeatsToWait;
    

    // Properties
    Player Player { get { return Player.inst; } }
    VisualController VisualController { get { return VisualController.inst; } }

    Player.Side curSide, lastSide;

    // Quantization
    private float quantization;
    public float Quantization
    {
        get
        {
            return quantization;
        }
        set
        {
            quantization = value;

            quantizeSteps = new List<float>();
            quantizeStep = 16 / value;
            float length = curSequencer.length / quantizeStep;
            for (int i = 0; i < length; i++)
            {
                quantizeSteps.Add(i * quantizeStep);
            }
            quantizeStep = quantizeSteps[1];
        }
    }



    private void Awake()
    {
        
    }


    private void OnEnable()
    {
        
    }

    

    void Start()
    {
        // Init
        inst = this;
        curChord = Chords.c4Major;          // stupid inits
        lastChord = curChord;
        controller = MusicRef.inst.helmController;
        curSequencer = MusicRef.inst.sequencers[0];

        Quantization = quantizationChoices[(int)curPrecision];
        //UIOps.inst.SetPrecisionText(curPrecision);

        //curInstrument.SetParameterValue(AudioHelm.Param.arp, 8);

        //controllers[0].SetPitchWheel(0);


    }




    // ------------------------------ Public functions ------------------------------




    /// <summary>
    /// Change controller channel and sequencer reference.
    /// </summary>
    /// <param name="layer"></param>
    public void ChangeLayer(int layer)
    {
        bool isPlaying = Player.inst.actionState == Player.ActionState.Play;
        var sequencerPos = (float) curSequencer.GetSequencerPosition();
        var sequencerNotes = AudioHelmHelper.GetCurrentNotes(curSequencer, sequencerPos);

        // 1. Stop curChord (if not being played in the sequencer)
        if (isPlaying)
        {
            foreach(int curNote in curChord.notes)
            {
                bool isPlayedInSequencer = false;
                foreach(Note seqNote in sequencerNotes)
                {
                    if (curNote == seqNote.note)
                    {
                        isPlayedInSequencer = true;
                        break;
                    }
                }

                if (!isPlayedInSequencer)
                {
                    controller.NoteOff(curNote);
                }
            }
            //controller.AllNotesOff();
        }
        
        // 2. Change sequencer & controller reference
        curSequencer = Recorder.inst.sequencers[layer];
        controller.channel = layer;
    }




    // ------------------------------ Private functions ------------------------------




    //private void ManageChordPlaying()
    //{
    //    if (Player.actionState == Player.ActionState.Play)
    //    {
    //        // FIRST EDGE TOUCH
    //        if (Player.curEdge.firstTouch)
    //        {
    //            #region pitch
    //            // calc pitch
    //            SetFirstPitchRange(ref minPitch, ref maxPitch);
    //            #endregion
    //        }
            
    //    }

    //    #region Pitch
    //    //if (Input.GetKey(KeyCode.Space))
    //    //{
    //    //    curPitch = player.curEdge.percentage.Remap(0, 1, minPitch, maxPitch);
    //    //    #region Quantize Pitch
    //    //    //float quantizeSize = 0.5f;
    //    //    //float quantize = curPitch % quantizeSize;
    //    //    //if (quantize > 0.05f || quantize < -0.05f)
    //    //    //{
    //    //    //    if (quantize > quantizeSize / 2f)
    //    //    //        curPitch += (quantizeSize - quantize);
    //    //    //    else
    //    //    //        curPitch -= quantize;
    //    //    //}
    //    //    Instrument.inner.SetPitchWheel(curPitch);
    //    //}
    //    #endregion
    //}

    private void PlayField()
    {
        // 1. Get field type
        lastChord = curChord;
        curChord = GetChord();

        int ID = Player.curField.ID;
        var fieldType = Player.curFieldSet[ID].type;
        
        if (!Player.inst.curFieldSet[ID].isSpawning)
        {
            // nur wenn sich feld nicht aufbaut
            switch (fieldType)
            {
                case MusicField.Type.Chord:
                    AudioHelmHelper.PlayChord(curChord, controller, velocity);
                    break;

                case MusicField.Type.Modulation:
                    //MusicFieldSet.SwitchEdgeParts();
                    break;

                case MusicField.Type.Pitch:
                    break;
            }
        }
        // 2. Event
        GameEvents.inst.onPlayField?.Invoke();
    }


    private void StopField()
    {
        // 1. Get field type
        int ID = Player.curField.ID;
        var fieldType = Player.curFieldSet[ID].type;            // TO DO: ID und fieldType sind glaube ich das aktuell anvisierte feld und nicht das letzte (noch spielende) feld
        
        switch (fieldType)
        {
            case MusicField.Type.Chord:
                AudioHelmHelper.StopChord(curChord, controller, curSequencer);
                break;

            case MusicField.Type.Modulation:
                break;

            case MusicField.Type.Pitch:
                break;
        }

        // 2. Event
        GameEvents.inst.onStopField?.Invoke();

    }



    /// <summary>
    /// Get chord from currently touched field
    /// </summary>
    private Chord GetChord()
    {
        int playerID = Player.curField.ID;
        Chord chord = Player.curFieldSet[playerID].chord;

        return chord;
    }




    // ------------------------------------ Events ------------------------------------



    // Fields
    public void OnFieldStart()
    {
        //print("OnFieldStart (abonniert onPlayPerformed, PlayField->invoke OnPlayField");
        PlayField();

        #region pitch
        // calc pitch
        SetFirstPitchRange(ref minPitch, ref maxPitch);
        #endregion
    }

    public void OnFieldChange(PlayerField data)
    {
        if (Player.actionState == Player.ActionState.Play)
        {
            StopField();
            PlayField();
            //print("OnFieldChange (abonniert OnFieldChange), PlayField -> invoke OnPlayField ");
        }
    }

    public void OnFieldLeave()
    {
        StopField();
    }



    // Input

    public void OnRecord(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // no UI
            var pointerPos = Pointer.current.position.ReadValue();
            if (!UIOps.inst.PointerHitsUI(pointerPos) || !Mouse.current.leftButton.isPressed) // unschön, hack
            {
                if (!Recorder.inst.isRecording && !Recorder.inst.isPreRecording)
                {
                    if (Recorder.inst.Has1stRecord)
                    {
                        Recorder.inst.StartRecord();
                    }
                    else
                    {
                        // Wird aktuell nicht mehr verwendet
                        if (firstRecordDelay)
                            Recorder.inst.StartRecordDelayed(LoopData.quartersPerBar);
                        else
                            Recorder.inst.StartRecord();
                    }
                }
                else
                {
                    Recorder.inst.StopRecord();
                }
            }
            
        }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controller.AllNotesOff();
            LoopData.Init();
        }
    }


    public void OnController1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            curSequencer = Recorder.inst.sequencers[0];
        }
    }

    public void OnController2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            curSequencer = Recorder.inst.sequencers[1];
        }
    }



    // Beats

    public void OnVeryFirstBeats(int beat)
    {
        if (beat == 0)
        {
            GameEvents.inst.onVeryFirstBeat?.Invoke();
            print("on first quarter");
        }
        else if (beat == 4)
        {
            GameEvents.inst.onVerySecondBeat?.Invoke();
            print("on second quarter");

            MusicRef.inst.beatSequencer.beatEvent.RemoveListener(this.OnVeryFirstBeats);
        }
    }

    public void OnBeat(int beat)
    {
        if (beat == 0)
        {
            curLoop++;
            GameEvents.inst.onFirstBeat?.Invoke();
        }

        if (beat % 4 == 0)
        {
            GameEvents.inst.onQuarter?.Invoke();
        }
        
        curBeat = curLoop * LoopData.beatsPerBar + beat;

        GameEvents.inst.onSixteenth?.Invoke(beat);
    }



    // Divers
    public void EnableQuantize(bool value)
    {
        quantize = value;
    }

    /// <summary>
    /// Increase the current precision level by 1. [0 == rough, 1 == middle, 2 == fine]
    /// </summary>
    public Precision IncreasePrecision()
    {
        curPrecision += 1;
        if ((int) curPrecision == quantizationChoices.Length)
            curPrecision = 0;

        Quantization = quantizationChoices[(int)curPrecision];

        return curPrecision;
    }



    // ------------------------- Pitch -------------------------------

    private void SetFirstPitchRange(ref float min, ref float max)
    {
        float randRange = Random.Range(maxEdgePitchIntervalRange, 0);
        min = curPitch - randRange * Player.curEdge.percentage;
        max = curPitch + randRange * (1 - Player.curEdge.percentage);
    }

    private void SetNextPitchRange(ref float min, ref float max)
    {
        //if (Player.curRotSpeed < 0)
        //{
        //    // Clockwise
        //    min = max;
        //    if (Random.Range(0, 2) == 0)
        //        max = max + Random.Range(1, maxEdgePitchIntervalRange);
        //    else
        //        max = min + Random.Range(-1, -maxEdgePitchIntervalRange);
        //}
        //else
        //{
        //    // Counter-clockwise
        //    max = min;
        //    if (Random.Range(0, 2) == 0)
        //        minPitch = minPitch + Random.Range(-1, -maxEdgePitchIntervalRange);
        //    else
        //        minPitch = maxPitch + Random.Range(1, maxEdgePitchIntervalRange);
        //}
    }





    // -------------------------Audio Helm helper -------------------------------



    //public IEnumerator WaitBeats(float beatsToWait)
    //{
    //    float targetBeat = curBeat + beatsToWait;
    //    print("COROUTINE; curBeat: " + curBeat + ", targetBeat: " + targetBeat);
    //    while (curBeat < targetBeat)
    //    {
    //        //print("curBeat: " + curBeat + ", targetBeat: " + targetBeat);
    //        yield return null;
    //    }
    //    print("FINISHED WAIT (targetBeat: " + targetBeat);
    //}





}
