using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MusicUtil
{
    public const int allMidiNotes = 128;

    public static void GetChordDegree(int[] chord, int keyNote, int[] scaleNotes)
    {

    }

    public  static void RandomChordInScale(Chord curChord, Scale curScale)
    {
        int newBaseNote = RandomNoteInScale(curChord, curScale); 
        int[] newChord = BasicChordInScale(newBaseNote, curScale);
        int[] invertedChord = ClosestInversionToChord(newChord, curChord.notes);
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
    public Type name;           // Name, z.b. Major
    public int keyNote;         // Grundton der Skala (Wert zwischen 0-127)
    public int[] notes;         // Alle verfügbaren Midi-Noten der Skala aus 0-127
    public int keyNoteIndex     // Index des Skala-Grundtons in notes
    {
        get { return System.Array.IndexOf(notes, keyNote); }
    }
    public int notesPerOctave;  // Anzahl der Noten innerhalb einer Oktave; meist 7



    // Auxilliary / fields
    public enum Type { Major, Minor, HexatonicBluesMinor };
    private Dictionary<Type, int[]> scaleTypes;
    //private int keyNoteIndex;


    // Constructor: Create all scales
    public Scale()
    {
        scaleTypes = new Dictionary<Type, int[]>()
        {
            { Type.Major, new int[7] { 0, 2, 4, 5, 7, 9, 11 } },
            { Type.Minor, new int[7] { 0, 2, 3, 5, 7, 8, 10 } },
            { Type.HexatonicBluesMinor, new int[6] { 0, 3, 5, 6, 7, 10} }
        };
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



