﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoopData
{
    // = Alle gebündelten generierten musikalischen Daten zur nächsten Stage
    // CONSTRAINTS werden HIER generiert: Anzahl Akkord-Stufen, Akkord-Stufen, Akkord-Strukturen, Tonumfang, Modulations-Felder (+Anzahl)
    // Was in MusicGeneration generiert wird: Konkrete Akkorde
    // Gemanaged wird alles hier; fertige Akkorde werden in EdgeParts hinein gespeichert

    // Constraints
    public static int chordTypeCount;
    public static ChordData[] chordTypes;
    public static EdgePart.Type[] modulationTypes;
    public static int toneRangeMin;
    public static int toneRangeMax;

    // Current play variables
    public static Key curKey;
    public static Dictionary<string, Weight[]> weights;
    public static string[] weightAreas;


    // CONSTRUCTOR
    static LoopData()
    {
        
    }

    private static Dictionary<string, Weight[]> InitWeights()
    {
        // = Create a dicitonary with data structures and values (name, value) for the global weights; also assign the values!

        // 1. Data structures and values

        // 1.1. Areas
        weightAreas = new string[]                                                              // WICHTIG #1: Order is important! If changed, change the names, too!
        { 
            Area.tonalRange, 
            Area.fields, 
            Area.chordIntervals 
        };       

        // 1.2. Names
        string[][] weightNames = new string[][]                                                 // WICHTIG #2: Variablen-Namen stimmen mit den Animator-Parametern überein!
        {
            // 1. Tonal Range
            new string [] { Action.stay, Action.increase, Action.decrease, Action.up, Action.down},
            // 2. Fields
            new string [] { Action.stay, Action.add, Action.remove},                              
            // 3. Chord Intervals
            new string [] { Action.stay, Action.change}
        };

        // 1.3. Values                                                                           // TO DO: berechnen! Bisher ist der stay-value jeder Area immer 1, alle anderen 0
        int[][] weightValues = new int[weightNames.Length][];
        for (int i = 0; i < weightNames.Length; i++)
        {
            weightValues[i] = new int[weightValues[i].Length];

            for (int j = 0; j < weightNames[i].Length; j++)
            {
                if (j == 0)
                    weightValues[i][j] = 1;
                else
                    weightValues[i][j] = 0;
            }
        }

        // 2. Create dictionary & assign
        var newWeights = Weights.Create(weightAreas, weightNames, weightValues);

        return newWeights;
    }
    
    public static void Init()
    {
        // 0. Weights
        //weights = InitWeights();

        // 1. key
        curKey = MusicGenerationLogic.RandomKey();

        // 2. tone range
        toneRangeMin = curKey.KeyNote + MusicUtil.notesPerOctave * 4;
        toneRangeMax = curKey.KeyNote + MusicUtil.notesPerOctave * 7;

        // 3. chord types
        // count
        chordTypeCount = 3;
        // degrees
        int[] degrees = MusicGenerationLogic.RandomChordDegrees(curKey, chordTypeCount);
        // intervals
        int[] intervals = new int[] { 1, 3, 5 };
        // individual count
        int[] individualCounts = MusicGenerationLogic.RandomChordTypeCounts(chordTypeCount);
        // colors
        var colors = EdgeParts.RandomColors(chordTypeCount);                    // to do?: eig nicht nötig hier
        // assign!
        chordTypes = new ChordData[chordTypeCount];
        for (int i = 0; i < chordTypeCount; i++)
        {
            
            chordTypes[i] = new ChordData(degrees[i], intervals, individualCounts[i], colors[i]);
            Debug.Log("chordTypes[i].indivCount: " + chordTypes[i].individualCount);
            //chordTypes[i].Set(degrees[i], intervals, individualCounts[i], colors[i]);
        }

        // 4. chords
        Chord[][] chords = MusicGenerationLogic.RandomChordsFromData(curKey, chordTypes, toneRangeMin, toneRangeMax);

        // 5. fields
        EdgeParts.SetFields(chords, colors);
    }


    // ------------------------



    public static void Generate(int stage)
    {
        SetTonalRange(stage);
        SetFields(stage);
        SetChordIntervals(stage);
        SetChords(stage);

        ChangeWeights(stage);


        
    }



    private static void SetTonalRange(int stage)
    {
        // 1. Decide if to change the constraining data
        // (= keep, increase, decrease, shift)
        Weight[] rangeWeights = weights[Area.tonalRange];
        string nextAction = Weights.RandomWeightedAction(rangeWeights);
        SetAnimatorVariables(nextAction);

        Debug.Log("nextOperation: " + nextAction);
    }

    private static void SetFields(int stage)
    {
        // 1. Decide if to change the constraining data
        // (= keep fields, add new field (chord/mod), remove field)
        Weight[] fieldWeights = weights[Area.fields];
        string nextAction = Weights.RandomWeightedAction(fieldWeights);
        SetAnimatorVariables(nextAction);
    }

    private static void SetChordIntervals(int stage)
    {

    }

    private static void SetChords(int stage)
    {

    }

    private static void ChangeWeights (int stage)
    {

    }

    private static void SetAnimatorVariables(string parameter)
    {

    }



    private static void GetWeights(int loop)
    {

        if (loop == 0)
        {

        }
        else if (loop == 1)
        {

        }
        else if (loop == 2)
        {

        }
        else if (loop == 3)
        {

        }
        else if (loop >= 4 && loop <= 7)
        {

        }
    }

}




public static class Area
{
    public static string tonalRange = "tonalRange";
    public static string fields = "fields";
    public static string chordIntervals = "chordIntervals";
    public static string chords = "chords";
}

public static class Action
{
    public static string stay = "stay";
    public static string increase = "increase";
    public static string decrease = "decrease";
    public static string up = "up";
    public static string down = "down";
    public static string add = "add";
    public static string remove = "remove";
    public static string change = "change";
}