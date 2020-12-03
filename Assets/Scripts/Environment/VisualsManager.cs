using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisualsManager : MonoBehaviour
{
    // public
    public static VisualsManager instance;

    [Header("Settings")]
    public int envGridLoops = 6;
    public bool showCursor = true;
    
    

    void Start()
    {
        instance = this;
        if (!showCursor)
            Cursor.visible = false;


        MeshCreation.instance.CreateMeshes();
    }


    void Update()
    {
        EnvironmentData.HandleData();
        MeshUpdate.instance.UpdateMeshes();
    }
}
