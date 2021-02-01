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
        int remainingFields = TunnelData.FieldsCount - VisualController.inst.tunnelVertices;      // Hack
        int[] chordTypeCounts = new int[allChordTypes];
        int fields;

        for (int i = 0; i<allChordTypes; i++)
        {
            if (i==0)
                fields = VisualController.inst.tunnelVertices;
            else if (i==allChordTypes-1)
                fields = remainingFields;
            else
                fields = remainingFields / (allChordTypes - i) + Random.Range(-1, 2);

            chordTypeCounts[i] = fields;
            remainingFields -= fields;
        }

        return chordTypeCounts;
    }



    public static Chord[][] RandomChordsFromData(Key key, ChordData[] chordTypes, int minNote, int maxNote)
    {
        // 1. Get all possible chords from data, within contraints (toneRange, chordData)
        Chord[][][] allChords = AllChordsFromData(key, chordTypes, minNote, maxNote);

        // 2. Select random chords
        Chord[][] randChords = SelectRandomChords(allChords, chordTypes);

        return randChords;
    }




    private static Chord[][][] AllChordsFromData(Key key, ChordData[] chordTypes, int minNote, int maxNote)
    {
        Chord[][] basicChords = new Chord[chordTypes.Length][];
        Chord[][] bigChords = new Chord[chordTypes.Length][];

        // 1. Gehe jeden CHORD TYPE durch
        for (int i=0; i<chordTypes.Length; i++)
        {
            int degree = chordTypes[i].degree;
            int[] intervals = chordTypes[i].intervals;
            //Chord basicChord = MusicUtil.Triad(key, degree, intervals);
            basicChords[i] = MusicUtil.AllChordInversions(key, degree, intervals, minNote, maxNote);
            bigChords[i] = MusicUtil.AllBigTriads(key, degree, intervals, minNote, maxNote);
        }

        Chord[][][] chords = new Chord[][][]
        {
            basicChords,
            bigChords
        };

        return chords;
    }

    private static Chord[][] SelectRandomChords(Chord[][][] chords, ChordData[] chordTypes)
    {
        Chord[][] finalChords = new Chord[chordTypes.Length][];

        // 1. Gehe jeden CHORD TYPE durch
        for (int i=0; i< chordTypes.Length; i++)
        {
            Chord[] relevantChords;
            // random: basic chord or big chord?
            if (ExtensionMethods.Probability(0f))
            {
                relevantChords = chords[0][i];
            }
            else
            {
                relevantChords = chords[1][i];
            }

            // 2. jede individuelle chordType-count
            finalChords[i] = new Chord[chordTypes[i].individualCount];
            var relevantChords_list = relevantChords.ToList();
            for (int j=0; j<chordTypes[i].individualCount; j++)
            {
                int randIndex = Random.Range(0, relevantChords_list.Count);
                //Debug.Log("indivTypeCount[" + i + "]: " + chordTypes[i].individualCount + ", relevantChords-array[" + i + "].length: " + relevantChords.Length + ", relevantChords_list.count: " + relevantChords_list.Count + ", randIndex: " + randIndex);
                finalChords[i][j] = relevantChords_list[randIndex];
                if (relevantChords_list.Count > 1)
                    relevantChords_list.RemoveAt(randIndex);            // hack
            }

        }

        //Debug.Log("final lengths. type1:" + finalChords[0].Length + "; type2: " + finalChords[1].Length + ", type3: " + finalChords[2].Length);
        //for (int i=0; i<finalChords.Length; i++)
        //{
        //    for (int j=0; j<finalChords[i].Length; j++)
        //    {
        //        Debug.Log("i: " + i + ", j: " + j + ", chord: " + finalChords[i][j].notes.AsNames());
        //    }
        //}



        return finalChords;
    }


    

    // Weitere Funktion: Akkordstruktur- und Stellung abhängig von Tonlage (unten weite Intervalle)


}
