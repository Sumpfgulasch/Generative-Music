using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TunnelData
{
    // == calc stuff and store data here and in Player
    

    // Public attributes
    [HideInInspector] public static Vector3[] vertices = new Vector3[3];
    [HideInInspector] public static MusicField[] fields;
    [HideInInspector] public static float fieldLength;      // TO DO: update funktion schreiben und mit ... verknüpfen?

    // Properties

    /// <summary>
    /// All musical fields. Corners are counted as one field.
    /// </summary>
    public static int FieldsCount                           // TO DO: update funktion schreiben und mit ... verknüpfen?
    {
        get { return vertices.Length * VisualController.inst.fieldsPerEdge - vertices.Length; }
    }



    // Private variables
    private static Vector3 playerMid;
    private static Player player;



    // Constructor
    static TunnelData()
    {
        playerMid = Player.inst.transform.position;
        player = Player.inst;
    } 

    // ----------------------------- Public methods ----------------------------

    




    // ----------------------------- Private methods ----------------------------

   


    
}
