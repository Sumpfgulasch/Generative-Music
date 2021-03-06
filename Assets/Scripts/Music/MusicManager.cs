﻿using System.Collections;
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
    public float shortNotes_minPlayTime = 0.3f;
    public int maxEdgePitchIntervalRange = 14;
    [Range(0, 1f)]
    public float velocity = 0.1f;

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
        controller = Instrument.inner;
        curSequencer = MusicRef.inst.sequencers[0];
        
        //curInstrument.SetParameterValue(AudioHelm.Param.arp, 8);

        //controllers[0].SetPitchWheel(0);

        
    }
    

    

    private void ManageChordPlaying()
    {
        if (Player.actionState == Player.ActionState.Play)
        {
            // FIRST EDGE TOUCH
            if (Player.curEdge.firstTouch)
            {
                #region pitch
                // calc pitch
                SetFirstPitchRange(ref minPitch, ref maxPitch);
                #endregion
            }
            
        }

        #region Pitch
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    curPitch = player.curEdge.percentage.Remap(0, 1, minPitch, maxPitch);
        //    #region Quantize Pitch
        //    //float quantizeSize = 0.5f;
        //    //float quantize = curPitch % quantizeSize;
        //    //if (quantize > 0.05f || quantize < -0.05f)
        //    //{
        //    //    if (quantize > quantizeSize / 2f)
        //    //        curPitch += (quantizeSize - quantize);
        //    //    else
        //    //        curPitch -= quantize;
        //    //}
        //    Instrument.inner.SetPitchWheel(curPitch);
        //}
        #endregion
    }

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
                    PlayChord(curChord, controller, velocity);
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
                StopChord(curChord, controller);
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




    // ------------------------------ Events ------------------------------



    // Fields
    public void OnFieldStart(Player.Side side)
    {
        if (side == Player.Side.inner)
            controller = Instrument.inner;
        else
            controller = Instrument.outer;


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
            if (!Recorder.inst.isRecording && !Recorder.inst.isPreRecording)
            {
                if (Recorder.inst.Has1stRecord)
                {
                    Recorder.inst.StartRecord();
                }
                else
                {
                    Recorder.inst.StartRecordDelayed(LoopData.quartersPerBar);
                }
            }
            else
            {
                Recorder.inst.StopRecord();
            }
            
        }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        Instrument.inner.AllNotesOff();
        Instrument.outer.AllNotesOff();
        if (context.performed)
        {
            LoopData.Init();
        }
    }

    public void OnPlayInside(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //curInstrument = Instrument.inner;
        }
    }

    public void OnPlayOutside(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //curInstrument = Instrument.outer;
        }
    }


    public void OnController1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            curSequencer = MusicRef.inst.sequencers[0];
        }
    }

    public void OnController2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            curSequencer = MusicRef.inst.sequencers[1];
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



    public static class Instrument
    {
        public static AudioHelm.HelmController inner;
        public static AudioHelm.HelmController outer;

        static Instrument()
        {
            inner = MusicRef.inst.helmControllers[0];
            outer = MusicRef.inst.helmControllers[1];
        }
    }



    public void PlayChord(Chord chord, AudioHelm.HelmController instrument, float velocity)
    {
        for (int i = 0; i < chord.notes.Length; i++)
        {
            if (!instrument.IsNoteOn(chord.notes[i]))
                instrument.NoteOn(chord.notes[i], velocity);
        }
    }

    public void StopChord(Chord chord, AudioHelm.HelmController instrument)
    {
        // Mindest-Spielzeit für Noten
        float timeToPlay = shortNotes_minPlayTime - instrument.pressedNotesDurations[chord.notes[0]].duration;
        
        for (int i = 0; i < chord.notes.Length; i++)
        {
            if (instrument.IsNoteOn(chord.notes[i]))
            {
                if (timeToPlay > 0)
                {
                    StartCoroutine(instrument.WaitNoteOff(chord.notes[i], timeToPlay));
                }
                else
                    instrument.NoteOff(chord.notes[i]);
            }
        }
    }

    public void QuantizeSequence() // AudioHelm.Sequencer sequencer
    {

    }

    public IEnumerator WaitBeats(float beatsToWait)
    {
        float targetBeat = curBeat + beatsToWait;
        print("COROUTINE; curBeat: " + curBeat + ", targetBeat: " + targetBeat);
        while (curBeat < targetBeat)
        {
            //print("curBeat: " + curBeat + ", targetBeat: " + targetBeat);
            yield return null;
        }
        print("FINISHED WAIT (targetBeat: " + targetBeat);
    }

    //public void WaitBeats(int beats)
    //{

    //    GameEvents.inst.onBeat += OnWaitBeats;
    //}

    //public void OnWaitBeats(int beats)
    //{

    //}


    


}
