using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public static List<int> RandomChordDegrees(Key key, int count, bool preventTritonus = true)
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
            if (key.Scale == ScaleTypes.Name.Major)
            {
                availableDegrees.Remove(7);
            }
            else if (key.Scale == ScaleTypes.Name.Minor)
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

        return degrees;
    }



    // PRIVATE FUNCTIONS



}
