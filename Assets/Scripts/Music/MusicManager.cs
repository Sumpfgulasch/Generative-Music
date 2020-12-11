using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager inst;

    // Public properties
    [Header("References")]
    public List<AudioHelm.HelmController> controllers;

    [Header("Properties")]
    public float shortNotes_minPlayTime = 0.3f;
    public int maxEdgePitchIntervalRange = 14;
    public int highestNote = 72;
    public int lowestNote = 48;

    // Public variables
    [HideInInspector] public Chord curChord;
    [HideInInspector] public Key curKey;
    [HideInInspector] public List<int> curCadence;
    [HideInInspector] public List<int> curSequence;

    // private
    private float minPitch, maxPitch;
    private float curPitch = 0;
    private int chordDirection = 1;


    // get set
    Player player { get { return Player.inst; } }

    
    void Start()
    {
        // Init
        if (inst == null)
            inst = this;
        else if (inst != null && inst != this)
            Destroy(inst);

        curKey = new Key(53, ScaleTypes.Name.Minor);
        curChord = MusicUtil.RandomChordInKey_stay(curKey);

        controllers[0].SetPitchWheel(0);
    }
    
    void Update()
    {
        ManageChordGeneration();
    }

    

    public void ManageChordGeneration()
    {
        //print("firstEdgeTouch: " + player.curEdge.firstTouch + ", edgePartChange: " + player.curEdgePart.changed + ", leaveEdge: " + player.curEdge.leave + ", edgeChange: " + player.curEdge.changed);


        // EDGE CHANGE
        if (player.curEdge.changed)
        {
            int newKeyNote = curKey.KeyNote + Random.Range(1, 7);
            if (newKeyNote > highestNote || newKeyNote < lowestNote)
                newKeyNote = 60;
            ScaleTypes.Name newScale;
            if (curKey.Scale == ScaleTypes.Name.Major)
                newScale = ScaleTypes.Name.Minor;
            else
                newScale = ScaleTypes.Name.Major;
            curKey.Set(newKeyNote, ScaleTypes.Name.Minor);


            // Pitch
            SetNextPitchRange(ref minPitch, ref maxPitch);
        }

        // FIRST EDGE TOUCH
        if (player.curEdge.firstTouch)
        {
            PlayChord(curChord, Instrument.inner, 0.3f);

            // calc pitch
            SetFirstPitchRange(ref minPitch, ref maxPitch);
        }

        // EDGE PART CHANGE
        else if (player.curEdgePart.changed)
        {
            if (!Input.GetKey(KeyCode.Space)) // für eventuellen pitch
            {
                StopChord(curChord, Instrument.inner);

                SetChordDirection();
                curChord = MusicUtil.RandomChordInKey_move(curKey, curChord, chordDirection);

                PlayChord(curChord, Instrument.inner, 0.3f);
            }
        }

        // LEAVE EDGE
        if (player.curEdge.leave)
        {
            StopChord(curChord, Instrument.inner);
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


    private void SetChordDirection()
    {
        if (curChord.notes[0] < lowestNote)
            chordDirection = 1;
        else if (curChord.notes[curChord.notes.Length - 1] > highestNote)
            chordDirection = -1;
    }




    // ------------------------- Pitch -------------------------------

    private void SetFirstPitchRange(ref float min, ref float max)
    {
        float randRange = Random.Range(maxEdgePitchIntervalRange, 0);
        min = curPitch - randRange * player.curEdge.percentage;
        max = curPitch + randRange * (1 - player.curEdge.percentage);
    }

    private void SetNextPitchRange(ref float min, ref float max)
    {
        if (player.curRotSpeed < 0)
        {
            // Clockwise
            min = max;
            if (Random.Range(0, 2) == 0)
                max = max + Random.Range(1, maxEdgePitchIntervalRange);
            else
                max = min + Random.Range(-1, -maxEdgePitchIntervalRange);
        }
        else
        {
            // Counter-clockwise
            max = min;
            if (Random.Range(0, 2) == 0)
                minPitch = minPitch + Random.Range(-1, -maxEdgePitchIntervalRange);
            else
                minPitch = maxPitch + Random.Range(1, maxEdgePitchIntervalRange);
        }
    }





    // -------------------------Audio Helm helper -------------------------------



    public static class Instrument
    {
        public static AudioHelm.HelmController inner;
        public static AudioHelm.HelmController outer;

        static Instrument()
        {
            inner = MusicManager.inst.controllers[0];
            outer = MusicManager.inst.controllers[1];
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
