using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MusicUtil
{
    // --------------------------- Public attributes --------------------------

    public const int allMidiNotes = 128;
    public const int notesPerOctave = 12; // Alle Halbton-Schritte innerhalb einer Oktave vom Grundton aus



    // PUBLIC METHODS

    public static void GetChordDegree(int[] chord, int keyNote, int[] scaleNotes)
    {

    }

    

    public  static void RandomChordInScale(Chord curChord, Scale curScale)
    {
        // 1. Degree of next chord (= get base note)
        int minRange = curScale.keyNote;
        int maxRange = curScale.keyNote + MusicUtil.notesPerOctave;
        int newBaseNote = RandomNoteInScale(minRange, maxRange, curScale, curChord.baseNote); 

        // 2. Basic chord
        int[] newChord = BasicTriadInScale(newBaseNote, curScale);

        // 3. Correct inversion
        int[] newChord_correctInversion = Inversion_stayInTonality(newChord, curChord.notes);
    }



    // PRIVATE METHODS

    private static int RandomNoteInScale(int noteMin, int noteMax, Scale scale, int preventNote = -1)
    {
        // 1. Random index (indices adress scale.notes, not allMidiNotes!)
        int rangeMin = System.Array.IndexOf(scale.notes, noteMin);
        int rangeMax = System.Array.IndexOf(scale.notes, noteMax);
        int randomIndex = Random.Range(rangeMin, rangeMax);

        // 2. New note than the last chord-base note?
        if (preventNote != -1)
        {
            while (scale.notes[randomIndex] == preventNote)
                randomIndex = Random.Range(rangeMin, rangeMax);
        }

        // 3. Assign
        int randomNote = scale.notes[randomIndex];

        return randomNote; // Wert zwischen 0-127
    }

    private static int[] BasicTriadInScale(int baseNote, Scale scale)
    {
        // Generate triad, no inversion
        int baseNoteIndex = System.Array.IndexOf(scale.notes, baseNote);
        int third = scale.notes[baseNoteIndex + 2];
        int fifth = scale.notes[baseNoteIndex + 4];
        int[] newchord = new int[3] { baseNote, third, fifth };

        return newchord;
    }

    // Inversion #1: Stay in tonality
    private static int[] Inversion_stayInTonality (int[] newChord, int[] lastChord)
    {
        // = Get inversion that is the closest to the start chord, in order to stay in its tonality

        int[] invertedChord = new int[2]; // ToDo: Hier weiter
        // Todo: Direction-Variable prüfen (die auch von Grenzen bestimmt wird)
        return invertedChord;
    }

    // Inversion #2: Move up/down
    private static int[] Inversion_moveInDirection(int direction)
    {
        if (direction == 1)
            Debug.Log("");
        else if (direction == -1)
            Debug.Log("");

        int[] invertedChord = new int[2]; // ToDo: Hier weiter
        // Todo: Direction-Variable prüfen (die auch von Grenzen bestimmt wird)
        return invertedChord;
    }

    // Inversion #3: Get next/prior
    private static int[] Inversion_getNext()
    {
        int[] invertedChord = new int[2]; // ToDo: Hier weiter
        // Todo: Direction-Variable prüfen (die auch von Grenzen bestimmt wird)
        return invertedChord;
    }
}





// -------------------------------------------------------------------------


public class Chord
{
    public int[] notes;     // Noten des Akkords; erstmal immer 3; Werte zwischen 0-127
    public int degree;      // Akkord-Stufe (I-VII)
    public int inversion;   // Akkord-Umkehrung; 0-2, 0 = keine Umkehrung
    public int baseNote;    // Grundton des Akkords ohne Umkehrung; Wert zwischen 0-127
}








// -------------------------------------------------------------------------


public class Scale
{
    // Public attributes
    public Type name;               // Name, z.b. Major
    public int keyNote;             // Grundton der Skala (Wert zwischen 0-127)
    public int[] notes;             // Alle verfügbaren Midi-Noten der Skala aus 0-127 (length immer kleiner als 128!)
    public int keyNoteIndex;        // Index des Skala-Grundtons in notes
    public int notesPerOctave;      // Anzahl der Skala-Noten innerhalb einer Oktave; meist 7
    
    // Auxilliary / fields
    private int[] stepsInOctave;    // Alle Intervalle von der Prim der Skala aus, die die Skala innerhalb einer Oktave ausmachen
    public enum Type { Major, Minor, HexatonicBluesMinor };
    private Dictionary<Type, int[]> scaleTypes;
    
    


    // CONSTRUCTOR: Create all scales
    public Scale()
    {
        scaleTypes = new Dictionary<Type, int[]>()
        {
            { Type.Major, new int[7] { 0, 2, 4, 5, 7, 9, 11 } },
            { Type.Minor, new int[7] { 0, 2, 3, 5, 7, 8, 10 } },
            { Type.HexatonicBluesMinor, new int[6] { 0, 3, 5, 6, 7, 10} }
        };
    }

    

    // PUBLIC METHODS

    public void Set(Type name, int keyNote)
    {
        this.name = name;
        this.keyNote = keyNote;
        this.stepsInOctave = scaleTypes[name];
        this.notesPerOctave = scaleTypes[name].Length;
        this.notes = GetScaleNotes(keyNote, stepsInOctave, notesPerOctave);
        this.keyNoteIndex = System.Array.IndexOf(this.notes, this.keyNote);
    }

    

    // PRIVATE METHODS

    private int[] GetScaleNotes(int keyNote, int[]stepsInOctave, int notesPerOctave)
    {
        // 1. NOTES
        List<int> newScaleNotes = new List<int>();
        int nextNote = keyNote / MusicUtil.notesPerOctave;                      // Wert ist immer 0 bis 11

        // 1.1. Get lowest notes (below lowest key note)
        if (nextNote != 0)
        {
            int negativeKeyNote = nextNote - MusicUtil.notesPerOctave;          // Wert zwischen -11 und -1
            for (int i = 0; i < notesPerOctave; i++)
            {
                nextNote = negativeKeyNote + stepsInOctave[i];
                if (nextNote >= 0)
                    newScaleNotes.Add(nextNote);
            }
        }
        // 1.2. Add all notes to highest key note                               // Wert zwischen 0 und 127, fast immer niedriger als 127
        while (nextNote + MusicUtil.notesPerOctave < MusicUtil.allMidiNotes)
        {
            for (int i = 0; i < notesPerOctave; i++)
            {
                newScaleNotes.Add(nextNote + stepsInOctave[i]);
            }
            nextNote += MusicUtil.notesPerOctave;
        }
        // 1.3. Add highest notes above highest key note                        // Werte zwischen 116 und 126
        if (nextNote < MusicUtil.allMidiNotes - 1)
        {
            int highestKeyNote = nextNote;
            for (int i = 0; i < notesPerOctave; i++)
            {
                nextNote = highestKeyNote + stepsInOctave[i];
                if (nextNote < MusicUtil.allMidiNotes)
                    newScaleNotes.Add(nextNote);
            }
        }

        return newScaleNotes.ToArray();
    }
}

