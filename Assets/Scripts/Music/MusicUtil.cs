using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// Functions around generating chords, inversions and chord-sequences

public static class MusicUtil
{
    // --------------------------- Public attributes --------------------------

    public const int allMidiNotes = 128;
    public const int notesPerOctave = 12; // Alle Halbton-Schritte innerhalb einer Oktave vom Grundton aus


    // PUBLIC METHODS
    
    public static Chord RandomChordInKey(Key curKey)
    {
        // 1. Degree of next chord
        int chordDegree = RandomChordDegree(curKey);

        // 2. Basic chord
        Chord chord = BasicTriad(curKey, chordDegree);

        // 3. Correct inversion
        chord = InvertChord_stayInTonality(chord, Chords.fMajor); // sehr geil; einfach geil

        //Debug.Log("chord before invertDown: degree: " + chord.degree + ", inversion: " + chord.inversion);
        //foreach (int note in chord.notes)
        //    Debug.Log("note: " + note);
        //chord = chord.InvertChord_down();
        //Debug.Log("chord.invertDown: degree: " + chord.degree + ", inversion: " + chord.inversion);
        //foreach (int note in chord.notes)
        //    Debug.Log("note: " + note);

        return chord;
    }



    // PRIVATE METHODS

    #region random note in scale, to rework
    // To rework (random range)
    //private static int RandomNoteInScale(int noteMin, int noteMax, Scale scale, int preventNote = -1)
    //{
    //    // 1. Random index (indices adress scale.notes, not allMidiNotes!)
    //    int rangeMin = System.Array.IndexOf(scale.notes, noteMin);
    //    int rangeMax = System.Array.IndexOf(scale.notes, noteMax);
    //    int randomIndex = Random.Range(rangeMin, rangeMax);

    //    // 2. New note than the last chord-base note?
    //    if (preventNote != -1)
    //    {
    //        while (scale.notes[randomIndex] == preventNote)
    //            randomIndex = Random.Range(rangeMin, rangeMax);
    //    }

    //    // 3. Assign
    //    int randomNote = scale.notes[randomIndex];

    //    return randomNote; // Wert zwischen 0-127
    //}
    #endregion

    /// <summary>
    /// Generate random chord degree. [Returns in pentatonic scales: 1-7]
    /// </summary>
    /// <param name="scale">The wanted scale.</param>
    /// <param name="preventDegrees">Degrees you wanna exclude.</param>
    private static int RandomChordDegree(Key scale, int[] preventDegrees = null)
    {
        // 1. Get available degrees of scale
        int maxDegree = scale.notesPerOctave;
        List<int> degrees = new List<int>();
        for (int i = 1; i <= maxDegree; i++)
            degrees.Add(i);

        // 2. Prevent certain degrees?
        if (preventDegrees != null)
        {
            foreach (int notThisDegree in preventDegrees)
                degrees.Remove(notThisDegree);
        }
        
        // 3. Generate random degree
        int randDegreeIndex = Random.Range(0, degrees.Count);

        return degrees[randDegreeIndex];
    }


    /// <summary>
    /// Returns a triad of thirds in a given scale and in the given degree within the octave of the curScale.keyNote. No inversion.
    /// </summary>
    /// <param name="degree">The wanted chord degree</param>
    /// <param name="chord">The given scale</param>
    private static Chord BasicTriad(Key key, int degree)
    {
        // Generate triad, no inversion
        int baseNoteIndex = key.keyNoteIndex + degree;
        int baseNote = key.notes[baseNoteIndex];
        int third = key.notes[baseNoteIndex + 2];
        int fifth = key.notes[baseNoteIndex + 4];
        int[] chordNotes = new int[3] { baseNote, third, fifth };
        Chord newChord = new Chord(chordNotes, degree, 0, baseNote);

        return newChord;
    }


    // Inversion #1: Stay in tonality
    private static int[] InvertChord_moveInDirection (int[] curChord, Chord relationChord)
    {
        // = Get inversion that is the closest to the start chord, in order to stay in its tonality

        int[] invertedChord = new int[2]; // ToDo: Hier weiter
        // Todo: Direction-Variable prüfen (die auch von Grenzen bestimmt wird)
        return invertedChord;
    }


    // Inversion #2: Move up/down
    /// <summary>
    /// Get inversion that is the closest to the relation chord, in order to stay in its tonality.
    /// </summary>
    /// <param name="chord">The chord to be inverted.</param>
    /// <param name="relationChord">The chord that the inversion shall be the closest to.</param>
    private static Chord InvertChord_stayInTonality(Chord chord, Chord relationChord)
    {
        // = Compare distances between lowest and highest notes of the current inversion and the relation chord (in semi-tones)
        Chord invertedChord = chord;           
        int distance, lastDistance;                          // distance in semi-tones

        // Nähere von oben an
        if (chord.notes[0] > relationChord.notes[0])
        {
            // 1. invert downwards until lowestNote < relationChord.lowestNote
            do
            {
                lastDistance = ChordDistance(invertedChord, relationChord);
                invertedChord = invertedChord.InvertChord_down();
            }
            while (invertedChord.notes[0] > relationChord.notes[0]);

            // InvertedChord ist jetzt niedriger als relationChord
            distance = ChordDistance(invertedChord, relationChord);

            // 2. Get final cloest chord: Maybe revert last inversion
            if (distance > lastDistance)                                                  // ToDo: hier Sonderregel mit weiterem if für niemals-die-gleiche-umkehrung
                invertedChord.InvertChord_up(); 
        }

        // Nähere von unten an
        else
        {
            // 1. Invert upwards until lowestNote > relationChord.lowestNote
            do
            {
                lastDistance = ChordDistance(invertedChord, relationChord);
                invertedChord.InvertChord_up();
            }
            while (invertedChord.notes[0] < relationChord.notes[0]);

            // InvertedChord ist jetzt höher als relationChord
            distance = ChordDistance(invertedChord, relationChord);

            // 2. Get final cloest chord: Maybe revert last inversion
            if (distance > lastDistance)                                                  // ToDo: hier Sonderregel mit weiterem if für niemals-die-gleiche-umkehrung
                invertedChord.InvertChord_down();
        }

        return invertedChord;
    }


    // Inversion #3: Get next/prior
    private static Chord InvertChord_up(this Chord chord)
    {
        // Move lowest note one octave up
        int[] newNotes = chord.notes.ShiftBackward();
        newNotes[newNotes.Length - 1] += MusicUtil.notesPerOctave;
        int newInversion = (chord.inversion + 1) % 3;
        Chord invertedChord = new Chord(newNotes, chord.degree, newInversion, chord.baseNote);
        
        return invertedChord;
    }


    private static Chord InvertChord_down(this Chord chord)
    {
        // Move highestNote one octave down
        int[] newNotes = chord.notes.ShiftForward();
        newNotes[0] -= MusicUtil.notesPerOctave;
        int newInversion = ExtensionMethods.Modulo(chord.inversion - 1, 3);
        Chord invertedChord = new Chord(newNotes, chord.degree, newInversion, chord.baseNote);

        return invertedChord;
    }


    private static int ChordDistance(Chord chord1, Chord chord2)
    {
        int lowestNote1 = chord1.notes[0];
        int highestNote1 = chord1.notes[chord1.notes.Length - 1];
        int lowestNote2 = chord2.notes[0];
        int highestNote2 = chord2.notes[chord2.notes.Length - 1];

        int lowestNotesDistance = Mathf.Abs(lowestNote1 - lowestNote2);
        int highestNotesDistance = Mathf.Abs(highestNote1 - highestNote2);

        int distance = lowestNotesDistance + highestNotesDistance;

        return distance;
    }
}





// -------------------------------------------------------------------------

// Existieren immer nur innerhalb einer Skala
public class Chord
{
    // Public attributes
    public int[] notes;     // Noten des Akkords; erstmal immer 3; Werte zwischen 0-127
    public int degree;      // Akkord-Stufe (I-VII) innerhalb seiner Skala                          // brauche ich die Stufe wirklich?
    public int inversion;   // Akkord-Umkehrung; 0-2, 0 = keine Umkehrung
    public int baseNote;    // Grundton des Akkords ohne Umkehrung; Wert zwischen 0-127

    // Constructor
    public Chord(int[] notes, int degree, int inversion, int baseNote)
    {
        this.notes = notes;
        this.degree = degree;
        this.inversion = inversion;
        this.baseNote = baseNote;
    }

    // Public methods
    public void Set(int[] notes, int inversion, int baseNote)
    {
        this.notes = notes;
        this.inversion = inversion;
        this.baseNote = baseNote;
    }
}








// -------------------------------------------------------------------------


public class Key
{
    // Public attributes
    public ScaleTypes.Name scale;   // Name, z.b. Major
    public int keyNote;             // Grundton der Skala (Wert zwischen 0-127)                                                             // To do: Ich will keynote zwischen 0-127 nicht festlegen, sondern Name [C-H] oder Wert [0-11]
    public int[] notes;             // Alle verfügbaren Midi-Noten der Skala aus 0-127 (length immer kleiner als 128!)
    public int keyNoteIndex;        // Index des Skala-Grundtons in notes
    public int notesPerOctave;      // Anzahl der Skala-Noten innerhalb einer Oktave; meist 7
    
    // Auxilliary / fields
    private int[] stepsInOctave;    // Alle Intervalle von der Prim der Skala aus, die die Skala innerhalb einer Oktave ausmachen






    // CONSTRUCTOR
    public Key(int keyNote, ScaleTypes.Name scale)
    {
        Set(keyNote, scale);
    }
    




    // PUBLIC METHODS

    public void Set(int keyNote, ScaleTypes.Name scale)
    {
        this.scale = scale;
        this.keyNote = keyNote;
        this.stepsInOctave = ScaleTypes.list[scale];
        this.notesPerOctave = ScaleTypes.list[scale].Length;
        this.notes = GetScaleNotes(keyNote, stepsInOctave, notesPerOctave);
        this.keyNoteIndex = System.Array.IndexOf(this.notes, this.keyNote);
    }

    

    // PRIVATE METHODS

    private int[] GetScaleNotes(int keyNote, int[]stepsInOctave, int notesPerOctave)
    {
        List<int> newScaleNotes = new List<int>();
        int nextNote = keyNote % MusicUtil.notesPerOctave;                      // Wert ist immer 0 bis 11
        //Debug.Log("keyNote % notesPerOctave: " + keyNote + " % " + MusicUtil.notesPerOctave + " = " + nextNote);

        // 1. Get lowest notes (below lowest key note)
        if (nextNote != 0)
        {
            int negativeKeyNote = nextNote - MusicUtil.notesPerOctave;          // Wert zwischen -11 und -1
            //Debug.Log("#1 negativeKeyNote: " + negativeKeyNote);
            for (int i = 0; i < notesPerOctave; i++)
            {
                nextNote = negativeKeyNote + stepsInOctave[i];
                if (nextNote >= 0)
                {
                    newScaleNotes.Add(nextNote);
                    //Debug.Log("#1 nextNote beeing added: " + nextNote);
                }
            }
            nextNote = negativeKeyNote + MusicUtil.notesPerOctave;
        }
        // 2. Add all notes to highest key note                                 // Wert zwischen 0 und 127, fast immer niedriger als 127
        while (nextNote + MusicUtil.notesPerOctave < MusicUtil.allMidiNotes)
        {
            for (int i = 0; i < notesPerOctave; i++)
            {
                newScaleNotes.Add(nextNote + stepsInOctave[i]);
                //Debug.Log("#2 nextNote beeing added: " + (nextNote + stepsInOctave[i]));
            }
            nextNote += MusicUtil.notesPerOctave;
        }
        // 3. Add highest notes above highest key note                          // Werte zwischen 116 und 126
        if (nextNote < MusicUtil.allMidiNotes - 1)
        {
            int highestKeyNote = nextNote;
            for (int i = 0; i < notesPerOctave; i++)
            {
                nextNote = highestKeyNote + stepsInOctave[i];
                if (nextNote < MusicUtil.allMidiNotes)
                {
                    newScaleNotes.Add(nextNote);
                    //Debug.Log("#3 nextNote beeing added: " + nextNote);
                }
            }
        }

        return newScaleNotes.ToArray();
    }
}







// -------------------------------------------------------------------------





public static class ScaleTypes
{
    public enum Name { Major, Minor, HexatonicBluesMinor };
    public static Dictionary<Name, int[]> list;

    




    // CONSTRUCTOR: Create all scales
    static ScaleTypes()
    {
        list = new Dictionary<Name, int[]>()
        {
            { Name.Major, new int[7] { 0, 2, 4, 5, 7, 9, 11 } },
            { Name.Minor, new int[7] { 0, 2, 3, 5, 7, 8, 10 } },
            { Name.HexatonicBluesMinor, new int[6] { 0, 3, 5, 6, 7, 10} }
        };
    }
}






// -------------------------------------------------------------------------







public static class Chords
{
    public static Chord fMajor;
    public static Chord cMajor;

    static Chords()
    {
        fMajor = new Chord(new int[3] { 67, 71, 74 }, 1, 0, 67);
        cMajor = new Chord(new int[3] { 60, 64, 67 }, 1, 0, 60);
    }
}





// -------------------------------------------------------------------------









