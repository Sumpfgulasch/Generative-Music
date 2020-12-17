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

    private int curLoop = 0;

    // Calc variables


    // Properties
    Player player { get { return Player.inst; } }
    VisualController visualController { get { return VisualController.inst; } }


    void Start()
    {
        // Init
        if (inst == null)
            inst = this;
        else if (inst != null && inst != this)
            Destroy(inst);

        // key
        curKey = new Key(7, ScaleTypes.Name.Major);

        int degree = MusicGenerationLogic.RandomChordDegree(curKey);
        curChord = MusicUtil.ChordInKey_stayInTonality(curKey, degree, Chords.f2Major);

        UpdateEdgeParts();

        controllers[0].SetPitchWheel(0);
    }
    
    void Update()
    {
        LoopData.Generate(curLoop);

        ManageChordPlaying();
    }

    

    private void ManageChordPlaying()
    {
        // EDGE CHANGE
        if (player.curEdge.changed)
        {
            #region pitch
            SetNextPitchRange(ref minPitch, ref maxPitch);
            #endregion
        }

        // FIRST EDGE TOUCH
        if (player.curEdge.firstTouch)
        {
            velocity = GetVelocity();
            curChord = GetChordFromEdgePart();

            PlayChord(curChord, Instrument.inner, velocity);
            #region pitch
            // calc pitch
            SetFirstPitchRange(ref minPitch, ref maxPitch);
            #endregion
        }

        // EDGE PART CHANGE
        else if (player.curEdgePart.changed)
        {
                StopChord(curChord, Instrument.inner);

                int newDegree = MusicGenerationLogic.RandomChordDegree(curKey, curChord.degree);
                curChord = GetChordFromEdgePart();

                PlayChord(curChord, Instrument.inner, velocity);
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

    private float GetVelocity()
    {
        return Player.inst.GetVelocityFromDistance();
    }

    private Chord GetChordFromEdgePart()
    {
        int playerID = player.curEdgePart.ID;
        Chord chord = EnvironmentData.edgeParts[playerID].chord;

        return chord;
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

    //private void UpdateEdgeParts(StageData data)
    //{
        
    //}



}

//[System.Serializable]
//public class StageData
//{
    
//}
