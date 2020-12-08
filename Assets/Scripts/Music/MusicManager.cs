﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager inst;

    // Public properties
    public List<AudioHelm.HelmController> controllers;
    public float shortNotes_minPlayTime = 0.3f;
    public int maxEdgeIntervalRange = 14;
    public int highestNote = 72;
    public int lowestNote = 48;
    public bool closeChordInversions = true;

    // Public variables
    //[HideInInspector] public int[] curChord = new int[3] { 60, 64, 67 };
    [HideInInspector] public Chord curChord;
    [HideInInspector] public Scale curScale;
    [HideInInspector] public List<int> curCadence;
    [HideInInspector] public List<int> curSequence;

    // private
    private float minPitch, maxPitch;
    private float curPitch = 0;
    private int nextIntervalDirection = 1;


    // get set
    Player player { get { return Player.inst; } }

    
    void Start()
    {
        if (inst == null)
            inst = this;
        else if (inst != null && inst != this)
            Destroy(inst);

        controllers[0].SetPitchWheel(0);

    }
    
    void Update()
    {
        
    }

    public void PlayChord(AudioHelm.HelmController instrument, float velocity)
    {
        // 3-Klang
        for (int i=0; i<3; i++)
        {
            if (!instrument.IsNoteOn(curChord[i]))
                instrument.NoteOn(curChord[i], velocity);
        }
    }

    public void StopChord(AudioHelm.HelmController instrument)
    {
        // 3-Klang
        float timeToPlay = shortNotes_minPlayTime - instrument.pressedNotesDurations[curChord[0]].duration;
        for (int i = 0; i < 3; i++)
        {
            if (instrument.IsNoteOn(curChord[i]))
            {
                if (timeToPlay > 0)
                    instrument.WaitNoteOff(curChord[i], timeToPlay);
                else
                    instrument.NoteOff(curChord[i]);
            }
        }
    }

    int[] ChordInCMajor()
    {
        int[] chord = new int[3];
        int[] cMajorRange = new int[8] { 60, 62, 64, 65, 67, 69, 71, 72 };

        // V2 - weniger veränderung
        int chosenNote = Random.Range(0, 3);
        int newNote = cMajorRange[Random.Range(0, 8)];
        while (newNote == curChord[0] || newNote == curChord[1] || newNote == curChord[2])
            newNote = cMajorRange[Random.Range(0, 8)];
        chord[chosenNote] = newNote;
        chord[(chosenNote + 1) % 3] = curChord[(chosenNote + 1) % 3];
        chord[(chosenNote + 2) % 3] = curChord[(chosenNote + 2) % 3];
        return chord;
    }


    public void SetPitchOnEdge(int note, AudioHelm.HelmController controller)
    {
        // First edge touch
        if (player.curEdge.firstTouch)
        {
            // calc pitch
            float randRange = Random.Range(maxEdgeIntervalRange, 0);
            minPitch = curPitch - randRange * player.curEdge.percentage;
            maxPitch = curPitch + randRange * (1 - player.curEdge.percentage);
        }

        // Edge part change
        else if (player.curEdgePart.changed && !Input.GetKey(KeyCode.Space))
        {
            // Akkordwechsel
            int[] newChord = ChordInCMajor();
            for (int i=0; i< curChord.Length; i++)
            {
                if (newChord[i] != curChord[i])
                {
                    controller.NoteOff(curChord[i]);
                    controller.NoteOn(newChord[i], 0.5f);
                }
            }
            curChord = newChord;
        }

        if (player.curEdge.changed)
        {
            if (player.curRotSpeed < 0)
            {
                //im Uhrzeigersinn
                minPitch = maxPitch;
                if (Random.Range(0, 2) == 0)
                    maxPitch = maxPitch + Random.Range(1, maxEdgeIntervalRange);
                else
                    maxPitch = minPitch + Random.Range(-1, -maxEdgeIntervalRange);
            }
            else
            {
                //gegen Uhrzeigersinn
                maxPitch = minPitch;
                if (Random.Range(0, 2) == 0)
                    minPitch = minPitch + Random.Range(-1, -maxEdgeIntervalRange);
                else
                    minPitch = maxPitch + Random.Range(1, maxEdgeIntervalRange);
            }
        }

        // Pitch
        curPitch = player.curEdge.percentage.Remap(0, 1, minPitch, maxPitch);

        #region Quantize Pitch
        //float quantizeSize = 0.5f;
        //float quantize = curPitch % quantizeSize;
        //if (quantize > 0.05f || quantize < -0.05f)
        //{
        //    if (quantize > quantizeSize / 2f)
        //        curPitch += (quantizeSize - quantize);
        //    else
        //        curPitch -= quantize;
        //}
        #endregion

        if (Input.GetKey(KeyCode.Space))
            controller.SetPitchWheel(curPitch);
    }
}
