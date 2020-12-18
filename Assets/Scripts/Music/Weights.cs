using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Weights
{



    public static Dictionary<string, Weight[]> Create (string[] areas, string[][] weightNames, int [][] weightValues)
    {
        // = Create a dictionary with the given weight-areas, weight-names and weight-values
        // (Iterate over each given weight area and create dictionary, that contains the area as key and a Weight-Array (with the actual variables and values) as value)
        Dictionary<string, Weight[]> weights = new Dictionary<string, Weight[]>();
        int i = 0;

        //foreach (Area area in System.Enum.GetValues(typeof(Area)))
        foreach (string area in areas)
        {
            Debug.Log("ENUM DEBUG! i: " + i + ", value: " + area);                              // TO DO: checken ob der Enum in der Reihenfolge wie deklariert durchlaufen wird

            string[] curNames = weightNames[i];
            int[] curValues = weightValues[i];
            
            Weight[] weightObjects = new Weight[curNames.Length];
            for (int j = 0; j < curNames.Length; j++)
                weightObjects[j] = new Weight(curNames[j], curValues[j]);

            weights.Add(area, weightObjects);
            i++;
        }

        return weights;
    }


    public static string RandomWeightedAction(Weight[] weights)
    {
        int randIndex = ExtensionMethods.GetRandomWeightedIndex(weights);
        string musicalAction = weights[randIndex].name;

        return musicalAction;
    }

}


public class Weight
{
    public string name;
    public float value;

    public Weight(string name, float value)
    {
        this.name = name;
        this.value = Mathf.Clamp01(value);
    }
}