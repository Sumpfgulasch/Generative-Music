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
    public static ChordData[] chordTypes;
    public static EdgePart.Type[] modulationTypes;

    public static int toneRangeMin;
    public static int toneRangeMax;

    public static Weights weights;


    // CONSTRUCTOR
    static LoopData()
    {
        // INIT LOOP
    }

    public class Weights
    {
        public TonalRange tonalRange;
        public Fields fields;
        public ChordIntervals chordIntervals;

        public class TonalRange
        {
            public float stay = 1;
            public float extend;
            public float reduce;
            public float up;
            public float down;

            public bool[] weights;
        }

        public class Fields
        {
            public float stay = 1;
            public float add;
            public float remove;

            public Dictionary<string, float> weights = new Dictionary<string, float> { { "test", 1f } };
        }

        public class ChordIntervals
        {
            public float stay = 1;
            public float change;
        }
    }




    public static void Generate(int stage)
    {
        GetTonalRange(stage);
        ManageFields(stage);
        GetChordIntervals(stage);
        GetChords(stage);

        

        // =  based on weights
        //StageData test = new StageData();
        //return test;

        // = Update chords & colors of the edgeParts; dependant on generated stageData
        // CORNERS

        // 1. get 1-5-8 chord
        int[] unisonChordStructure = stageData[curStage].unison.chordStructure;
        Chord unisonChord = MusicUtil.Triad(curKey, 1, unisonChordStructure);

        // 2. get 3 different inversions of 1-5-8 (within current tonality range, if possible)
        List<Chord> unisonChords = MusicUtil.ChordInversions(unisonChord, VisualController.inst.envVertices, Chords.c4Major, stageData[0].toneRangeMin, stageData[0].toneRangeMax);

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

            bool probability50 = Random.Range(0, 1f) > 0.5f;

            if (probability50)
            {
                // 
            }

            if (edgePart.isEdgeMid)
            {

            }



        }
    }

    // Alle haben weights. Bisher unabhängig von bisherigen Werten, sind nur feste Wahrscheinlichkeiten

    private static void GetTonalRange(int stage)
    {
        float weight_stay = weights.tonalRange.stay;

        if (!ExtensionMethods.Probability(weight_stay))
        {
            toneRangeMin = 20;
            toneRangeMax = 100;
        }
    }

    private static void ManageFields(int stage)
    {

    }

    private static void GetChordIntervals(int stage)
    {

    }

    private static void GetChords(int stage)
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




