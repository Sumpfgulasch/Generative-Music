using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoopData
{
    // = Alle gebündelten generierten musikalischen Daten zur nächsten Stage
    // CONSTRAINTS werden HIER generiert: Anzahl Akkord-Stufen, Akkord-Stufen, Akkord-Strukturen, Tonumfang, Modulations-Felder (+Anzahl)
    // Was in MusicGeneration generiert wird: Konkrete Akkorde
    // Gemanaged wird alles hier; fertige Akkorde werden in EdgeParts hinein gespeichert

    public static int basicChordTypes_count;
    public static ChordData[] chordData;
    public static EdgePart.Type[] modulationTypes;

    public static int toneRangeMin;
    public static int toneRangeMax;


    public static Dictionary<string, Weight[]> weights;
    public static string[] weightAreas;

 

    // CONSTRUCTOR
    static LoopData()
    {
        
    }

    private static Dictionary<string, Weight[]> InitWeights()
    {
        // = Create a dicitonary with data structures and values (name, value) for the global weights

        // 1. Data structures and values

        // 1.1. Areas
        weightAreas = new string[] { Area.tonalRange, Area.fields, Area.chordIntervals };       // WICHTIG #1: Order is important! If changed, change the names, too!
        // 1.2. Variable-names
        string[][] weightNames = new string[][]                                                 // WICHTIG #2: Variablen-Namen stimmen mit den Animator-Parametern überein!
        {
            // 1. Tonal Range
            new string [] { Action.stay, Action.increase, Action.decrease, Action.up, Action.down},
            // 2. Fields
            new string [] { Action.stay, Action.add, Action.remove},                              
            // 3. Chord Intervals
            new string [] { Action.stay, Action.change}
        };
        // 1.3. Values                                                                           // TO DO: berechnen
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

        // 2. Create dictionary
        var newWeights = Weights.Create(weightAreas, weightNames, weightValues);

        return newWeights;
    }
    
    public static void Init()
    {
        // 0. Weights
        weights = InitWeights();

        // 1. key
        MusicManager.inst.curKey = MusicGenerationLogic.RandomKey();

        // 2. tone range

        // 3. chord types
            // count
            // degrees
            // intervals

        // 4. chords
            // generate

        // 5. fields
            // assign chords to edgeParts
            // colors
    }


    // ------------------------



    public static void Generate(int stage)
    {
        SetTonalRange(stage);
        SetFields(stage);
        SetChordIntervals(stage);
        SetChords(stage);

        ChangeWeights(stage);


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



    private static void SetTonalRange(int stage)
    {
        // 1. Decide if to change the constraining data
        // (= keep, increase, decrease, shift)
        Weight[] curWeights = weights[Area.tonalRange];
        string nextAction = Weights.RandomWeightedAction(curWeights);
        SetAnimatorVariables(nextAction);

        Debug.Log("nextOperation: " + nextAction);
    }

    private static void SetFields(int stage)
    {
        // 1. Decide if to change the constraining data
        // (= keep fields, add new field (chord/mod), remove field)
        Weight[] curWeights = weights[Area.fields];
        string nextAction = Weights.RandomWeightedAction(curWeights);
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