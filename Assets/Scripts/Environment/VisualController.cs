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
    public int envVertices = 3;
    public bool showCursor = true;
    [Range(0.001f, 0.05f)]
    public float edgePartThickness = 0.01f;
    [Range(0.001f, 0.05f)]
    public float playerEdgePartThickness = 0.03f;

    // Properties
    public int EdgePartCount
    {
        get { return envVertices * envGridLoops; }
    }    

    void Start()
    {
        inst = this;
        if (!showCursor)
            Cursor.visible = false;

        MeshCreation.CreateMeshes();
    }


    void Update()
    {
        EnvironmentData.HandleData();
        MeshUpdate.UpdateMeshes();
    }
}
