using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager inst;


    // Public properties
    [Header("References")]
    public List<AudioHelm.HelmController> controllers;


    [Header("Contraints")]
    [Tooltip("Number of chord degrees that make up the edgeParts in the beginning.")]
    public int startDegreesCount = 3;
    public float shortNotes_minPlayTime = 0.3f;
    public int maxEdgePitchIntervalRange = 14;
    public int highestNote = 72;
    public int lowestNote = 48;
    public float maxVelocity = 0.3f;
    public float minVelocity = 0.1f;

    [Header("Stages")]
    public StageData[] stageData;


    // Private variables
    private Chord curChord;
    private Key curKey;
    private List<int> curCadence;
    private List<int> curSequence;
    private int stage = 0;
    private List<int> availableDegrees;

    private float velocity;
    private float minPitch, maxPitch;
    private float curPitch = 0;
    private int chordDirection = 1;

    private int curStage = 0;

    // Calc variables


    // get set
    Player player { get { return Player.inst; } }

    
    void Start()
    {
        // Init
        if (inst == null)
            inst = this;
        else if (inst != null && inst != this)
            Destroy(inst);
        InitEdgeParts();

        // key
        curKey = new Key(7, ScaleTypes.Name.Minor);
        int degree = MusicLogic.RandomChordDegree(curKey);
        curChord = MusicUtil.ChordInKey_stayInTonality(curKey, degree, Chords.f2Major);

        controllers[0].SetPitchWheel(0);
    }
    
    void Update()
    {
        ManageStages();
        ManageChordGeneration();
    }

    
    private void ManageStages()
    {
        switch (stage)
        {
            case 0:
                break;

            case 1:
                break;

            default:
                break;
        }
    }

    private void ManageChordGeneration()
    {
        // EDGE CHANGE
        if (player.curEdge.changed)
        {
            // New key
            int newKeyNote = Random.Range(1, 8);
            ScaleTypes.Name newScale;
            if (curKey.Scale == ScaleTypes.Name.Major)
                newScale = ScaleTypes.Name.Minor;
            else
                newScale = ScaleTypes.Name.Major;
            curKey = MusicUtil.ChangeKey(newKeyNote, newScale);


            // Pitch
            SetNextPitchRange(ref minPitch, ref maxPitch);
        }

        // FIRST EDGE TOUCH
        if (player.curEdge.firstTouch)
        {
            GetVelocity();

            PlayChord(curChord, Instrument.inner, velocity);

            #region pitch
            // calc pitch
            SetFirstPitchRange(ref minPitch, ref maxPitch);
            #endregion
        }

        // EDGE PART CHANGE
        else if (player.curEdgePart.changed)
        {
            if (!Input.GetKey(KeyCode.Space)) // für eventuellen pitch
            {
                SetChordDirection();


                StopChord(curChord, Instrument.inner);

                int newDegree = MusicLogic.RandomChordDegree(curKey, curChord.degree);
                curChord = MusicUtil.ChordInKey_stayInTonality(curKey, newDegree, Chords.c3Major);

                PlayChord(curChord, Instrument.inner, velocity);
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

    private void GetVelocity()
    {
        velocity = Player.inst.GetVelocityFromDistance();
    }

    public void SetEdgeParts(List<int> availableDegrees)
    {
        // Set chords, chord patterns & modulation fields

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



    // ------------------------- Edge parts: types & colors -------------------------------

    private void InitEdgeParts()
    {

        // 1. get 1-5-8 chord
        // 2. get 3 different inversions of 1-5-8 (within current tonality range, if possible)
        int[] unisonNotes = stageData[curStage].unison.chordStructure;
        Chord unisonChord = MusicUtil.Triad(curKey, 1, unisonNotes[0], unisonNotes[1], unisonNotes[2]);

        // first additional degree
        // 1. get random degree
        // 2. get 1-3-5 chord on degree
        // 3. get (5x3 - 3) / 2 different inversions of 1-3-5 (within current tonality range, if possible)

        // second additional degree
        // 1. get new random degree
        // 2. get 1-3-5 chord on degree
        // 3. get (5x3 - 3) / 2 different inversions of 1-3-5 (within current tonality range, if possible)

        for (int i = 0; i < EnvironmentData.edgeParts.Length; i++)
        {
            if (EnvironmentData.edgeParts[i].isCorner)
            {
                // Degree = I.

            }

            bool probability50 = Random.Range(0,1f) > 0.5f;

        }
    }



}

[System.Serializable]
public class StageData
{
    [Header("General")]
    public string name;
    public int toneRangeMin;
    public int toneRangeMax;
    [Space]
    public ChordData unison;
    [Space]
    public ChordData[] additionalDegrees;

  

    [System.Serializable]
    public class ChordData
    {
        public string name;
        public int[] chordStructure = new int[3];
    }

    // Constructor
    //StageData()
    //{
    //    additionalDegrees = new ChordData[degrees];
    //}
}
