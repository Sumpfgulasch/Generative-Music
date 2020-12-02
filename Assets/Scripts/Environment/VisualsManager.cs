using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisualController : MonoBehaviour
{
    // public
    public static VisualController instance;

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
        EnvironmentData.instance.HandleData();
        MeshUpdate.instance.UpdateMeshes();
    }
}
