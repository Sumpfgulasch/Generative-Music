using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TunnelData
{
    // == calc stuff and store data here and in Player
    

    // Public attributes
    [HideInInspector] public static Vector3[] vertices = new Vector3[3];
    [HideInInspector] public static MusicField[] fields;



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
