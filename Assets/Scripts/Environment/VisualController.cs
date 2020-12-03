using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisualController : MonoBehaviour
{
    // public
    public static VisualController inst;

    [Header("Settings")]
    public int envGridLoops = 6;
    public bool showCursor = true;
    
    

    void Start()
    {
        inst = this;
        if (!showCursor)
            Cursor.visible = false;


        MeshCreation.inst.CreateMeshes();
    }


    void Update()
    {
        EnvironmentData.HandleData();
        MeshUpdate.UpdateMeshes();
    }
}
