using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class LoopData
{
    // = Alle gebündelten generierten musikalischen Daten zur nächsten Stage
    // CONSTRAINTS werden HIER generiert: Anzahl Akkord-Stufen, Akkord-Stufen, Akkord-Strukturen, Tonumfang, Modulations-Felder (+Anzahl)
    // Was in MusicGeneration generiert wird: Konkrete Akkorde
    // Gemanaged wird alles hier; fertige Akkorde werden in EdgeParts hinein gespeichert

    // Constraints
    public static int chordTypeCount;
    public static ChordData[] chordTypes;
    public static MusicField.Type[] modulationTypes;
    public static int toneRangeMin;
    public static int toneRangeMax;
    public static float minVelocity;
    public static float maxVelocity;

    // Current play variables
    public static Key curKey;
    public static Dictionary<string, Weight[]> weights;
    public static string[] weightAreas;
    public static int curCycle;
    public static float timer;

    // time & distance values
    public static float timePerBar;
    public static float timePerBeat;
    public static int beatsPerBar;

    // Properties
    private static Player Player { get { return Player.inst; } }
    private static int FieldsCount { get { return TunnelData.FieldsCount; } }


    // CONSTRUCTOR
    static LoopData()
    {
        // EVENT subscription
        MusicRef.inst.beatSequencer.beatEvent.AddListener(ManageSpawning);

        GetBeatData();
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
            new string [] { MusicAction.stay, MusicAction.increase, MusicAction.decrease, MusicAction.up, MusicAction.down},
            // 2. Fields
            new string [] { MusicAction.stay, MusicAction.add, MusicAction.remove},                              
            // 3. Chord Intervals
            new string [] { MusicAction.stay, MusicAction.change}
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
    
    /// <summary>
    /// Generate all musical data and store it in TunnelData.fields.
    /// </summary>
    public static void Init()
    {
        // 0. Weights
        //weights = InitWeights();

        // 1. key
        curKey = MusicGenerationLogic.RandomKey();

        // 2. tone range
        toneRangeMin = curKey.KeyNote + MusicUtil.notesPerOctave * 3;
        toneRangeMax = curKey.KeyNote + MusicUtil.notesPerOctave * 5;

        // 3. velocity
        minVelocity = 0.08f;
        maxVelocity = 0.2f;

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
        var colors = MeshUpdate.RandomColors(chordTypeCount);
        // assign!
        chordTypes = new ChordData[chordTypeCount];
        for (int i = 0; i < chordTypeCount; i++)
        {
            chordTypes[i] = new ChordData(degrees[i], intervals, individualCounts[i], colors[i]);
        }

        int[] testArray = new int[] { curKey.KeyNote };
        Debug.Log("curKey: " + testArray.NoteNames() + "-" + curKey.Scale + ", baseNote: " + (curKey.KeyNote + 4 * MusicUtil.notesPerOctave));

        // 4. chords
        Chord[][] chords = MusicGenerationLogic.RandomChordsFromData(curKey, chordTypes, toneRangeMin, toneRangeMax);

        // 5. fields
        var fieldTypes = new MusicField.Type[FieldsCount];
        var selectables = new bool[FieldsCount];
        var buildUps = new bool[FieldsCount];
        for (int i=0; i < FieldsCount; i++)
        {
            fieldTypes[i] = MusicField.Type.Chord;
            selectables[i] = true;
            buildUps[i] = true;
        }
        TunnelData.fields = MusicFieldSet.StoreDataInFields(TunnelData.fields, fieldTypes, chords, colors, selectables, buildUps);

        Player.curFields = TunnelData.fields;

        // 6. Beat data
        GetBeatData();



    }


    // ------------------------



    private static void ManageSpawning(int beat)
    {
        if (beat == 0)
        {
            // Increase cycle
            curCycle++;


        }
    }

    private static bool AllowForSpecialField()
    {
        // = Gibt es aktuell ein special field? ist eines aktiv?

        return true;
    }



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


    /// <summary>
    /// Time-related data: BeatsPerBar, timePerBar, timePerBeat.
    /// </summary>
    public static void GetBeatData()
    {
        // Info: 4 16tel sind 1 Beat
        // Wichtig: length muss immer vielfaches sein von 4; z.B. length=16 == 4/4-Takt, length=20 == 5/4-Takt
        beatsPerBar = MusicRef.inst.beatSequencer.length / 4;
        timePerBar = (beatsPerBar / MusicRef.inst.clock.bpm) *60;
        timePerBeat = timePerBar / beatsPerBar;
    }

}




public static class Area
{
    public static string tonalRange = "tonalRange";
    public static string fields = "fields";
    public static string chordIntervals = "chordIntervals";
    public static string chords = "chords";
}

public static class MusicAction
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