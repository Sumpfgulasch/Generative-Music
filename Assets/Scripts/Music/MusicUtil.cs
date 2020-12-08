using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MusicUtil
{
    public const int allMidiNotes = 128;
    public const int notesPerOctave = 12;

    public static void GetChordDegree(int[] chord, int keyNote, int[] scaleNotes)
    {

    }

    public  static void RandomChordInScale(Chord curChord, Scale curScale)
    {
        int newBaseNote = RandomNoteInScale(curChord, curScale); 
        int[] newChord = BasicChordInScale(newBaseNote, curScale);
        int[] newChord_correctInversion = ClosestInversionToChord(newChord, curChord.notes);
    }



    public static int RandomNoteInScale(Chord curChord, Scale curScale, bool preventSameNote = true)
    {
        int rangeMin = curScale.keyNoteIndex;
        int rangeMax = rangeMin + curScale.notesPerOctave;
        int randomIndex = Random.Range(rangeMin, rangeMax); // Index für curScale.notes (immer deutlich kleiner als 127)

        if (preventSameNote)
        {
            while (curScale.notes[randomIndex] == curChord.baseNote)
                randomIndex = Random.Range(rangeMin, rangeMax);
        }

        int randomNote = curScale.notes[randomIndex];

        return randomNote; // Wert zwischen 0-127
    }

    public static int[] BasicChordInScale(int baseNote, Scale scale)
    {
        // Generate triad, no inversion
        int[] newchord = new int[2]; // Todo: weiter hier
        return newchord;
    }

    public static int[] ClosestInversionToChord (int[] newChord, int[] lastChord)
    {
        int[] invertedChord = new int[2]; // ToDo: Hier weiter
        // Todo: Direction-Variable prüfen (die auch von Grenzen bestimmt wird)
        return invertedChord;
    }

}


// ----------------------------- Further classes ----------------------------


public class Chord
{
    public int[] notes;     // Noten des Akkords; erstmal immer 3; Werte zwischen 0-127
    public int degree;      // Akkord-Stufe (I-VII)
    public int inversion;   // Akkord-Umkehrung; 0-2, 0 = keine Umkehrung
    public int baseNote;    // Grundton des Akkords ohne Umkehrung; Wert zwischen 0-127
}




public class Scale
{
    // Public attributes
    public Type name;               // Name, z.b. Major
    public int keyNote;             // Grundton der Skala (Wert zwischen 0-127)
    public int[] notes;             // Alle verfügbaren Midi-Noten der Skala aus 0-127 (length immer kleiner als 128!)
    public int keyNoteIndex;        // Index des Skala-Grundtons in notes
    public int notesPerOctave;      // Anzahl der Skala-Noten innerhalb einer Oktave; meist 7


    // Auxilliary / fields
    private int[] stepsPerOctave;   // Alle Halbton-Schritte innerhalb einer Oktave vom Grundton aus
    public enum Type { Major, Minor, HexatonicBluesMinor };
    private Dictionary<Type, int[]> allScales;
    
    

    // Constructor: Create all scales
    public Scale()
    {
        allScales = new Dictionary<Type, int[]>()
        {
            { Type.Major, new int[7] { 0, 2, 4, 5, 7, 9, 11 } },
            { Type.Minor, new int[7] { 0, 2, 3, 5, 7, 8, 10 } },
            { Type.HexatonicBluesMinor, new int[6] { 0, 3, 5, 6, 7, 10} }
        };
    }

    // Public functions
    public void Set(Type name, int keyNote)
    {
        this.name = name;
        this.keyNote = keyNote;
        this.stepsPerOctave = allScales[name];
        this.notesPerOctave = allScales[name].Length;
        

        // Generate remaining data
        // 1. NOTES
        List<int> newScaleNotes = new List<int>();
        int testNote = 0;
        int lowestBaseNote = keyNote / MusicUtil.notesPerOctave;                // Wert ist immer 0 bis 11

        // 1.1. Get lowest notes (below lowest key note)
        if (lowestBaseNote != 0)
        {
            int negativeKeyNote = lowestBaseNote - MusicUtil.notesPerOctave;    // Wert zwischen -11 und -1
            for (int i = 0; i < this.notesPerOctave; i++)
            {
                testNote = negativeKeyNote + stepsPerOctave[i];
                if (testNote >= 0)
                    newScaleNotes.Add(testNote);
            }
        }
        // 1.2. Add all notes to highest key note                               // Wert zwischen 0 und 127, fast immer niedriger als 127
        while (testNote + MusicUtil.notesPerOctave < MusicUtil.allMidiNotes)
        {
            for (int i=0; i < notesPerOctave; i++)
            {
                newScaleNotes.Add(testNote + stepsPerOctave[i]);
            }
            testNote += MusicUtil.notesPerOctave;
        }
        // 1.3. Add highest notes above highest key note                        // Werte zwischen 116 und 127
        if (testNote < MusicUtil.allMidiNotes - 1)
        {

        }

        

        // 2. keynoteIndex
        this.keyNoteIndex = System.Array.IndexOf(this.notes, this.keyNote);

    }
}











//public class Music
//{
//    // Constructor
//    public Music()
//    {
//        chord = new Chord();
//        scale = new Scale();
//    }

//    // Public attributes
//    public Chord chord;
//    public Scale scale;
//    public List<int> cadence;
//    public List<int> sequence;
//}



