using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{

    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        // infinite lengths
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        if (Mathf.Approximately(planarFactor, 0f) &&
            !Mathf.Approximately(crossVec1and2.sqrMagnitude, 0f))
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }


    public static bool LineSegmentsIntersection(out Vector2 intersection, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        // finite lengths
        intersection = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }


    public static Vector2 FindNearestPointOnLine(Vector2 start, Vector2 end, Vector2 point)
    {
        //Get heading
        Vector2 heading = (end - start);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - start;
        float dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return start + heading * dotP;
    }

    public static Vector3 ClampVector3_2D(Vector3 value, float min, float max)
    {
        // only x & y!
        Vector3 clampedVector3;
        clampedVector3 = new Vector3(
            Mathf.Clamp(value.x, min, max),
            Mathf.Clamp(value.y, min, max),
            value.z);
        return clampedVector3;
    }

    public static Vector3[] ConvertArrayFromWorldToLocal(Vector3[] array, Transform localSpace)
    {
        // für vertices konvertieren
        // macht nur Sinn, wenn beim Mesh der zu konvertierenden Vertices scale = 1 und rotation = 0 sind
        Vector3[] convertedVertices = new Vector3[array.Length];
        for (int i = 0; i < array.Length; i++)
            convertedVertices[i] = localSpace.InverseTransformPoint(array[i]);

        return convertedVertices;
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    // Make first element to last element (chords: invert up)
    public static int[] ShiftBackward(this int[] myArray)
    {
        int[] tArray = new int[myArray.Length];
        for (int i = 0; i < myArray.Length; i++)
        {
            if (i < myArray.Length - 1)
                tArray[i] = myArray[i + 1];
            else
                tArray[i] = myArray[0];
        }
        return tArray;
    }

    // Make last element to first element (chords: invert down)
    public static int[] ShiftForward(this int[] myArray)
    {
        int[] tArray = new int[myArray.Length];
        for (int i = 0; i < myArray.Length; i++)
        {
            if (i > 0)
                tArray[i] = myArray[i - 1];
            else
                tArray[i] = myArray[myArray.Length - 1];
        }
        return tArray;
    }

    public static int Modulo(int value, int modulo)
    {
        return ((value % modulo) + modulo) % modulo;
    }

    public static List<int> IntToList(int number, bool startFromZero = false)
    {
        List<int> newList = new List<int>();
        if (!startFromZero)
        {
            for (int i = 1; i <= number; i++)
                newList.Add(i);
        }
        else
        {
            for (int i = 0; i < number; i++)
                newList.Add(i);
        }

        return newList;
    }

    public static bool Probability(float probability)
    {
        // probability has to be 0 - 1!

        float value = Random.value;
        if (value <= probability)
            return true;
        return false;
    }

    public static int GetRandomWeightedIndex (Weight[] weights)
    {
        // Sum of weights has to be 1

        float addedWeights = 0;
        float randomNumber = Random.value;

        for (int i = 0; i < weights.Length; i++)
        {
            addedWeights += weights[i].value;

            if (addedWeights <= randomNumber)
                return i;
        }
        Debug.LogError("Summe der weights ist nicht 1");
        return -1;
    }

    public static T RandomEnumValue<T>()
    {
        var values = System.Enum.GetValues(typeof(T));
        int random = Random.Range(0, values.Length);
        return (T)values.GetValue(random);
    }

    public static void PrintList(string startText, List<int> myArray)
    {
        Debug.Log(startText + System.String.Join(", ", new List<int>(myArray).ConvertAll(k => k.ToString()).ToArray()));
    }

    public static void PrintArray(string startText, int[] array)
    {
        string text = "";
        foreach(int number in array)
        {
            text = text + number.ToString() + ", ";
        }

        text.Remove(text.Length - 2, 2);

        Debug.Log(startText + text);
    }

    public static string ArrayToString(this int[] myArray)
    {
        string myString = "";
        for (int i=0; i<myArray.Length; i++)
        {
            myString = myString + myArray[i].ToString() + ", ";
        }
        myString = myString.Substring(0, myString.Length - 2);

        return myString;
    }
}
