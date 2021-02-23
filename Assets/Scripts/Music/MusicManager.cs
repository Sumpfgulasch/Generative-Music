﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Linq;

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

    // Private variables

    private float minPitch, maxPitch;
    private float curPitch = 0;
    [HideInInspector] public AudioHelm.HelmController curInstrument;
    

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
        curInstrument = Instrument.inner;

        
        //curInstrument.SetParameterValue(AudioHelm.Param.arp, 8);

        //controllers[0].SetPitchWheel(0);

        // EVENT SUBSCRIPTION
        MusicRef.inst.beatSequencer.beatEvent.AddListener(OnFirstBeats);
        MusicRef.inst.beatSequencer.beatEvent.AddListener(OnBeat);
        GameEvents.inst.onFieldStart += OnFieldStart;
        GameEvents.inst.onFieldChange += OnFieldChange;
        GameEvents.inst.onFieldLeave += OnFieldLeave;
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
        lastChord = curChord;
        curChord = GetChord();

        int ID = Player.curField.ID;
        var fieldType = Player.curFieldSet[ID].type;

        // nur wenn sich feld nicht aufbaut
        if (!Player.inst.curFieldSet[ID].isSpawning)
        {
            switch (fieldType)
            {
                case MusicField.Type.Chord:
                    PlayChord(curChord, curInstrument, velocity);
                    //print("Chord: " + curChord.notes.NoteNames());
                    break;

                case MusicField.Type.Modulation:
                    //MusicFieldSet.SwitchEdgeParts();
                    break;

                case MusicField.Type.Pitch:
                    break;
            }
        }
    }

    
    

    private Chord GetChord()
    {
        // = Get chord from currently touched edgePart
        int playerID = Player.curField.ID;
        Chord chord = Player.curFieldSet[playerID].chord;

        return chord;
    }




    // ------------------------------ Events ------------------------------



    // Fields
    private void OnFieldStart(Player.Side side)
    {
        if (side == Player.Side.inner)
            curInstrument = Instrument.inner;
        else
            curInstrument = Instrument.outer;


        PlayField();

        // HACK
        //if (side == Player.Side.inner)
        //{
        //    lastSide = curSide;
        //    curInstrument = Instrument.inner;
        //    curSide = side;
        //}
        //else
        //{
        //    lastSide = curSide;
        //    curInstrument = Instrument.outer;
        //    curSide = side;
        //}
        //if (curSide != lastSide)
        //    PlayField();

        #region pitch
        // calc pitch
        SetFirstPitchRange(ref minPitch, ref maxPitch);
        #endregion
    }

    private void OnFieldChange(PlayerField data)
    {
        if (Player.actionState == Player.ActionState.Play)
        {
            StopChord(curChord, curInstrument);
            PlayField();
        }
    }

    private void OnFieldLeave()
    {
        StopChord(curChord, curInstrument);
    }



    // Input

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



    // Beats

    private void OnFirstBeats(int beat)
    {
        if (beat == 0)
        {
            GameEvents.inst.onFirstBeat?.Invoke();
            print("on first beat");
        }
        else if (beat / LoopData.beatsPerBar == 1)
        {
            GameEvents.inst.onSecondBeat?.Invoke();
            MusicRef.inst.beatSequencer.beatEvent.RemoveListener(this.OnFirstBeats);
            print("on second beat");
        }
    }

    private void OnBeat(int beat)
    {
        if (beat % LoopData.beatsPerBar == 0)
        {
            GameEvents.inst.onBeat?.Invoke(beat / LoopData.beatsPerBar);
        }
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


    


}
