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


    /// <summary>
    /// Generate a chord in a key and stay in a certain tonality.
    /// </summary>
    /// <param name="key">The current key.</param>
    /// <param name="degree">The wanted degree.</param>
    ///     /// <param name="tonality">The tonality you wanna stay in as close as possible.</param>
    public static Chord ChordInKey_stayInTonality(Key key, int degree, Chord tonality)
    {
        // 1. Basic chord
        Chord chord = BasicTriad(key, degree);

        // 2. Correct inversion
        chord = InvertChord_stayInTonality(chord, tonality);

        return chord;
    }


    /// <summary>
    /// Generate a chord in a key and move up or down. Tempo is defined by direction.
    /// </summary>
    /// <param name="key">The key you wanna have.</param>
    /// <param name="degree">The degree of the wanted chord.</param>
    /// <param name="direction">The direction to move to. Slowly up/down: [1 or -1], fast up/down: [2 or -2].</param>
    /// <param name="relationChord">The chord from where you wanna move up or down. Usually the last chord.</param>
    public static Chord ChordInKey_move(Key key, int degree, int direction, Chord relationChord)
    {
        // 1. Basic chord
        Chord chord = BasicTriad(key, degree);

        // 2. Correct inversion
        chord = InvertChord_moveInDirection(chord, direction, relationChord);

        return chord;
    }

    /// <summary>
    /// Returns a new key.
    /// </summary>
    /// <param name="keyNote">The note defining the key [0-11]. (0=C, 7=G, 11=H).</param>
    /// <param name="scale">The wanted scale.</param>
    public static Key ChangeKey(int keyNote, ScaleTypes.Name scale)
    {
        keyNote = ExtensionMethods.NegativeModulo(keyNote, ScaleTypes.list[scale].Length);
        Key key = new Key(keyNote, scale);

        return key;
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
    /// Returns any triad in a given key and in the given degree*. The tonality is 48-59. No inversion. (* Chords not containing the perfect unison are not supported yet.)
    /// </summary>
    /// <param name="key">The key in which you wanna have the chord.</param>
    /// <param name="degree">The wanted degree within the key.</param>
    /// <param name="intervals">Intervals. [1 = perfect unison, 8 = octave]. intervals[0] has to be 1.</param>
    public static Chord Triad(Key key, int degree, int[] intervals)
    {
        int baseNoteIndex = key.notesPerOctave * 4 + key.keyNoteIndex + degree;
        int note1 = key.notes[baseNoteIndex + (intervals[0] - 1)];
        int note2 = key.notes[baseNoteIndex + (intervals[1] - 1)];
        int note3 = key.notes[baseNoteIndex + (intervals[2] - 1)];
        int[] chordNotes = new int[3] { note1, note2, note3 };

        Chord newChord = new Chord(chordNotes, degree, 0, note1);

        if (intervals[0] != 1)
            Debug.LogError("Chords different than 1-3-5 are not supported yet.");

        return newChord;
    }

    /// <summary>
    /// Returns a triad of thirds in a given key and in the given degree. The tonality is 48-59. No inversion.
    /// </summary>
    /// <param name="key">The key in which you wanna have the chord.</param>
    /// <param name="degree">The wanted degree within the key.</param>
    private static Chord BasicTriad(Key key, int degree)
    {
        int[] intervals = new int[] { 1, 3, 5 };
        Chord newChord = Triad(key, degree, intervals);

        return newChord;
    }

    

    /// <summary>
    /// Get inversion that is the closest to the relation chord, in order to stay in its tonality.
    /// </summary>
    /// <param name="chord">The chord to be inverted.</param>
    /// <param name="relationChord">The chord that the inversion shall be the closest to.</param>
    private static Chord InvertChord_stayInTonality(Chord chord, Chord relationChord)
    {
        // = Compare distances between lowest and highest notes of the current inversion and the relation chord (in semi-tones)
        Chord invertedChord = chord;           
        int distance, lastDistance;                                         // distance in semi-tones

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
            if (distance > lastDistance)                                    // ToDo: hier Sonderregel mit weiterem if für niemals-die-gleiche-umkehrung
            {                                                 
                invertedChord = invertedChord.InvertChord_up();
            }
        }

        // Nähere von unten an
        else
        {
            // 1. Invert upwards until lowestNote > relationChord.lowestNote
            do
            {
                lastDistance = ChordDistance(invertedChord, relationChord);
                invertedChord = invertedChord.InvertChord_up();
            }
            while (invertedChord.notes[0] < relationChord.notes[0]);

            // InvertedChord ist jetzt höher als relationChord
            distance = ChordDistance(invertedChord, relationChord);

            // 2. Get final cloest chord: Maybe revert last inversion
            if (distance > lastDistance)                                    // ToDo: hier Sonderregel mit weiterem if für niemals-die-gleiche-umkehrung
            {                                                  
                invertedChord = invertedChord.InvertChord_down();
            }
        }

        return invertedChord;
    }



    /// <summary>
    /// Inverts a chord so that it shifts its tonality slowly or fast up or down, compared to another chord.
    /// </summary>
    /// <param name="chord">The chord to be inverted.</param>
    /// <param name="direction">The direction to move to. Slowly up/down: [1 or -1], fast up/down: [2 or -2].</param>
    /// <param name="relationChord">The chord that the inversion shall be the closest to. Usually the last chord.</param>
    private static Chord InvertChord_moveInDirection(Chord chord, int direction, Chord relationChord)
    {
        Chord invertedChord = chord;
        
        // 1. Get the closest inversion to the relationChord as start point
        invertedChord = InvertChord_stayInTonality(chord, relationChord);

        if (direction > 0)
        {
            // 2. Check if chord is already higher; skip one iteration then
            bool chordIsHigher = ChordIsHigher(invertedChord, relationChord);

            for (int i=0; i < Mathf.Abs(direction); i++)
            {
                if (i == 0 && chordIsHigher)
                    continue;

                invertedChord = InvertChord_up(invertedChord);
            }
        }
        else if (direction < 0)
        {
            // 2. Check if chord is already lower; skip one iteration then
            bool chordIsLower = ChordIsLower(invertedChord, relationChord);

            for (int i = 0; i < Mathf.Abs(direction); i++)
            {
                if (i == 0 && chordIsLower)
                    continue;

                invertedChord = InvertChord_down(invertedChord);
            }
        }
        
        return invertedChord;
    }


    /// <summary>
    /// Inverts a chord x times in always different ways. Orients to a relationChord. Forced to stay in a certain tonality. Ordered in distance to relationChord.
    /// </summary>
    /// <param name="chord">The chord to be inverted.</param>
    /// <param name="count">The amount of inverted chords you wanna have.</param>
    /// <param name="relationChord">The chord that the inversion shall be the closest to. Usually the last chord.</param>
    /// <param name="minNote">Lowest possible note.</param>
    /// <param name="maxNote">Highest possible note.</param>
    public static List<Chord> ChordInversions(Chord chord, int count, Chord relationChord, int minNote, int maxNote)
    {
        Chord[] inversions = new Chord[count];
        inversions[0] = InvertChord_stayInTonality(chord, relationChord);
        Chord startChord = inversions[0];
        Chord nextInversion;
        bool alternativelyUpAndDown = true;
        int invertDirection = 1;

        //Debug.Log("RelationChord: " + relationChord.notes[0] + ", " + relationChord.notes[1] + ", " + relationChord.notes[2]);
        //Debug.Log("StartChord: " + startChord.notes[0] + ", " + startChord.notes[1] + ", " + startChord.notes[2]);

        for (int i = 1; i < count; i++)
        {
            nextInversion = InvertChord_moveInDirection(startChord, invertDirection, startChord);
            //Debug.Log("i : " + i + ", direction: " + invertDirection + ", nextInversion: " + nextInversion.notes[0] + ", " + nextInversion.notes[1] + ", " + nextInversion.notes[2]);
            bool chordIsWithinRange = ChordIsWithinRange(nextInversion, minNote, maxNote);

            // Phase 1: Go alternatively up and down
            if (alternativelyUpAndDown)
            {
                invertDirection *= -1;
                if (i % 2 == 0)
                    invertDirection++;

                if (!chordIsWithinRange)
                {
                    // Reverse last inversion, assign & go to phase 2
                    alternativelyUpAndDown = false;

                    nextInversion = InvertChord_moveInDirection(startChord, invertDirection, startChord);
                    invertDirection += 1 * (int) Mathf.Sign(invertDirection);

                    chordIsWithinRange = ChordIsWithinRange(nextInversion, minNote, maxNote);
                    #region Debug message for "missing" functionality
                    if (!chordIsWithinRange)
                        Debug.Log("ERROR! Chord inversions [" + (i+1) + " / " + count + "]: Phase 2 is not within range anymore");
                    #endregion
                }
            }
            // Phase 2: Search only downwards or upwards
            else
            {
                invertDirection += 1 * (int)Mathf.Sign(invertDirection);
                #region Debug message for "missing" functionality
                if (!chordIsWithinRange)
                    Debug.Log("ERROR! Chord inversions [" + (i+1) + " / " + count + "]: Phase 2 is not within range anymore");
                #endregion
            }

            inversions[i] = nextInversion;
        }

        return inversions.ToList();
    }



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
        int newInversion = ExtensionMethods.NegativeModulo(chord.inversion - 1, 3);
        Chord invertedChord = new Chord(newNotes, chord.degree, newInversion, chord.baseNote);

        return invertedChord;
    }


    private static int ChordDistance(Chord chord1, Chord chord2)
    {
        // = Returns the distance between two chords. Always a positive value.
        int lowestNote1 = chord1.notes[0];
        int highestNote1 = chord1.notes[chord1.notes.Length - 1];
        int lowestNote2 = chord2.notes[0];
        int highestNote2 = chord2.notes[chord2.notes.Length - 1];

        int lowestNotesDistance = Mathf.Abs(lowestNote1 - lowestNote2);
        int highestNotesDistance = Mathf.Abs(highestNote1 - highestNote2);

        int distance = lowestNotesDistance + highestNotesDistance;

        return distance;
    }


    private static bool ChordIsHigher(Chord chord, Chord relationChord)
    {
        int lowestNotesDistance = 0;
        int highestNotesDistance = 0;
        GetDistanceData(chord, relationChord, ref lowestNotesDistance, ref highestNotesDistance);

        if (lowestNotesDistance > 0)                                    // TO DO: vergleicht nicht mittlere Töne, können auch entscheidend sein
        {
            if (highestNotesDistance >= 0)
                return true;
            else
                return false;
        }
        else if (lowestNotesDistance == 0)
        {
            if (highestNotesDistance > 0)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    private static bool ChordIsLower(Chord chord, Chord relationChord)
    {
        int lowestNotesDistance = 0;
        int highestNotesDistance = 0;
        GetDistanceData(chord, relationChord, ref lowestNotesDistance, ref highestNotesDistance);

        if (lowestNotesDistance < 0)                                    // TO DO: vergleicht nicht mittlere Töne, können auch entscheidend sein
        {
            if (highestNotesDistance <= 0)
                return true;
            else
                return false;
        }
        else if (lowestNotesDistance == 0)
        {
            if (highestNotesDistance < 0)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    private static void GetDistanceData(Chord chord, Chord relationChord, ref int lowestNotesDistance, ref int highestNotesDistance)
    {
        int lowestNote1 = chord.notes[0];
        int highestNote1 = chord.notes[chord.notes.Length - 1];
        int lowestNote2 = relationChord.notes[0];
        int highestNote2 = relationChord.notes[relationChord.notes.Length - 1];

        lowestNotesDistance = lowestNote1 - lowestNote2;
        highestNotesDistance = highestNote1 - highestNote2;
    }


    private static bool ChordIsWithinRange(Chord chord, int minNote, int maxNote)
    {
        int highestNote = chord.notes[chord.notes.Length - 1];
        int lowestNote = chord.notes[0];

        if (highestNote <= maxNote && lowestNote >= minNote)
            return true;
        else
            return false;

        //if (highestNote > maxNote)
        //{
        //    highIsHIgher = true;
        //    return false;
        //}
        //else if( lowestNote < minNote)
        //{
        //    highIsHIgher = false;
        //    return false;
        //}
        //else
        //{
        //    return true;
        //}
    }

}





// -------------------------------------------------------------------------

// Existieren immer nur innerhalb einer Skala
public class Chord
{
    // Public attributes
    public int[] notes;     // Noten des Akkords; erstmal immer 3; Werte zwischen 0-127
    public int degree;      // Akkord-Stufe (I-VII) innerhalb seiner Skala
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




//public class ChordProgression
//{
//    public List<int> degrees;
//}



// -------------------------------------------------------------------------



public class Key
{
    // Public attributes
    private ScaleTypes.Name scale;
    public ScaleTypes.Name Scale        // Name, z.b. Major
    {
        get { return scale; }
        private set { scale = value; }
    }
    private int keyNote;
    public int KeyNote                  // Tiefster Grundton der Skala (Wert zwischen 0-11)
    {
        get { return keyNote; }
        private set { keyNote = value; }
    }
    public int keyNoteIndex;            // Index des tiefsten Skala-Grundtons in notes
    public int[] notes;                 // Alle verfügbaren Midi-Noten der Tonart aus 0-127 (length immer kleiner als 128!)
    public int notesPerOctave;          // Anzahl der Skala-Noten innerhalb einer Oktave; meist 7
    
    // Auxilliary / fields
    private int[] stepsInOctave;        // Alle Intervalle von der Prim der Skala aus, die die Skala innerhalb einer Oktave ausmachen






    // CONSTRUCTOR

    /// <param name="keyNote">The note defining the key [0-11]. (0=C, 7=G, 11=H).</param>
    /// <param name="scale">The wanted scale.</param>
    public Key(int keyNote, ScaleTypes.Name scale)
    {
        Set(keyNote, scale);
    }
    




    // PUBLIC METHODS

    public void Set(int keyNote, ScaleTypes.Name scale)
    {
        this.Scale = scale;
        this.KeyNote = keyNote;
        this.stepsInOctave = ScaleTypes.list[scale];
        this.notesPerOctave = ScaleTypes.list[scale].Length;
        this.notes = GetScaleNotes(keyNote, stepsInOctave, notesPerOctave);
        this.keyNoteIndex = System.Array.IndexOf(this.notes, this.KeyNote);
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
    public static Chord f2Major;
    public static Chord c3Major;
    public static Chord c4Major;

    static Chords()
    {
        f2Major = new Chord(new int[3] { 41, 45, 48 }, 1, 0, 41);
        c3Major = new Chord(new int[3] { 48, 52, 55 }, 1, 0, 48);
        c4Major = new Chord(new int[3] { 60, 64, 67 }, 1, 0, 60);
    }
}