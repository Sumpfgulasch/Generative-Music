﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MusicGenerationLogic
{
    // Alle Logik zur Generierung von Musik???

    // CONSTRUCTOR
    static MusicGenerationLogic()
    {

    }



    // PUBLIC FUNCTIONS


    /// <summary>
    /// Generate a random chord degree. [Returns 1-7 in pentatonic scales]
    /// </summary>
    /// <param name="key">The wanted scale.</param>
    /// <param name="preventDegree">A degree you wanna exclude [1-7 in pentatonic scales]. Usually the one of the last chord. Will prevent nothing if ignored.</param>
    public static int RandomChordDegree(Key key, int preventDegree = -1)
    {
        // 1. Get available degrees of scale
        List<int> availableDegrees = ExtensionMethods.IntToList(key.notesPerOctave);

        // 2. Prevent certain degrees?
        if (preventDegree != -1)
        {
            availableDegrees.Remove(preventDegree);
        }

        // 3. Generate random degree
        int randDegreeIndex = Random.Range(0, availableDegrees.Count);
        int randDegree = availableDegrees[randDegreeIndex];

        return randDegree;
    }



    /// <summary>
    /// Generate different random chord degrees within the key. First is always first degree. Possible to prevent tritonus degrees.
    /// </summary>
    /// <param name="key">The current key.</param>
    /// <param name="count">The wanted degree count.</param>
    /// <param name="preventTritonus">Will work with 1-3-5 chords.</param>
    public static int[] RandomChordDegrees(Key key, int count, bool preventTritonus = true)
    {
        // = return x different degrees within key; first is always I.

        // 1. Get available degrees of scale
        List<int> degrees = new List<int>();
        List<int> availableDegrees = ExtensionMethods.IntToList(key.notesPerOctave);

        degrees.Add(1);
        availableDegrees.Remove(1);

        // 2. Exclude tritonus
        if (preventTritonus)
        {
            if (key.Scale == Scale.Name.Major)
            {
                availableDegrees.Remove(7);
            }
            else if (key.Scale == Scale.Name.Minor)
            {
                availableDegrees.Remove(2);
            }
        }

        // 3. Get new degrees
        for (int i = 0; i < count - 1; i++)
        {
            int randDegreeIndex = Random.Range(0, availableDegrees.Count);
            int newDegree = availableDegrees[randDegreeIndex];
            degrees.Add(newDegree);
            availableDegrees.Remove(newDegree);
        }

        return degrees.ToArray();
    }

    public static void ChordDegrees_Popular(Key key, int count)
    {

    }

    public static Key RandomKey()
    {
        int randKeyNote = Random.Range(0, MusicUtil.notesPerOctave);
        Scale.Name randScale = ExtensionMethods.RandomEnumValue<Scale.Name>();

        Key key = new Key(randKeyNote, randScale);

        return key;
    }

    public static int[] RandomChordTypeCounts(int allChordTypes)
    {
        // = returne einen int[] mit den gewünschten individuellen anzahlen von Feldern der jeweiligen chord degrees
        int remainingFields = VisualController.inst.EdgePartCount;
        int[] chordTypeCounts = new int[allChordTypes];
        int curCount;

        for (int i = 0; i<allChordTypes; i++)
        {
            if (i==0)
                curCount = VisualController.inst.envVertices * 2;
            else if (i==allChordTypes)
                curCount = remainingFields;
            else
                curCount = remainingFields / (allChordTypes - i) + Random.Range(-1, 2);

            chordTypeCounts[i] = curCount;
            remainingFields -= curCount;
        }

        return chordTypeCounts;
    }



    public static Chord[][] RandomChordsFromData(ChordData[] chordData)
    {
        // 1. Get all possible chords from data, within contraints (toneRange, chordData)
        AllChordsFromData(chordData);

        // 2. Select random chords
        SelectRandomChords();
    }




    private static void AllChordsFromData(ChordData[] chordData)
    {
        
    }

    private static void SelectRandomChords()
    {




        // = Update chords & colors of the edgeParts; dependant on generated stageData
        // CORNERS

        // 1. get 1-5-8 chord
        int[] chordIntervals = stageData[curStage].unison.chordStructure;
        Chord chord = MusicUtil.Triad(curKey, 1, chordIntervals);

        // 2. get 3 different inversions of 1-5-8 (within current tonality range, if possible)
        List<Chord> unisonChords = MusicUtil.ChordInversions(chord, VisualController.inst.envVertices, Chords.c4Major, stageData[0].toneRangeMin, stageData[0].toneRangeMax);

        for (int i = 0; i < visualController.envVertices; i++)
        {
            // chords & colors
            int ID1 = ExtensionMethods.NegativeModulo(i * visualController.envGridLoops - 1, visualController.EdgePartCount);
            int ID2 = i * visualController.envGridLoops;

            EnvironmentData.edgeParts[ID1].chord = unisonChords[i];
            EnvironmentData.edgeParts[ID1].lineRend.material.color = MeshRef.inst.envEdgePart_corner;
            EnvironmentData.edgeParts[ID2].chord = unisonChords[i];
            EnvironmentData.edgeParts[ID2].lineRend.material.color = MeshRef.inst.envEdgePart_corner;
        }


        // REST

        // first additional degree
        // 1. get random degree
        // 2. get 1-3-5 chord on degree
        // 3. get (5x3 - 3) / 2 different inversions of 1-3-5 (within current tonality range, if possible)

        // second additional degree
        // 1. get new random degree
        // 2. get 1-3-5 chord on degree
        // 3. get (5x3 - 3) / 2 different inversions of 1-3-5 (within current tonality range, if possible)
        for (int ID = 0; ID < EnvironmentData.edgeParts.Length; ID++)
        {
            EdgePart edgePart = EnvironmentData.edgeParts[ID];

            if (edgePart.isCorner)
                continue;


            if (edgePart.isEdgeMid)
            {

            }



        }
    }

    // Weitere Funktion: Akkordstruktur- und Stellung abhängig von Tonlage (unten weite Intervalle)


}
