using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualController : MonoBehaviour
{
    // public
    public static VisualController inst;

    [Header("Settings")]
    public int fieldsPerEdge = 6;
    public int tunnelVertices = 3;
    public bool showCursor = true;
    [Range(0.001f, 0.05f)]
    public float fieldThickness = 0.01f;
    [Range(0.001f, 0.05f)]
    public float playerFieldThickness = 0.03f;
    [Range(0.001f, 0.05f)]
    public float playerFieldFocusThickness = 0.02f;
    public float playerFieldBeforeSurface = 0.002f;
    public float fieldsBeforeSurface = 0.01f;

    // Properties
    public int FieldsCount
    {
        get { return tunnelVertices * fieldsPerEdge; }
    }    

    void Start()
    {
        inst = this;
        if (!showCursor)
            Cursor.visible = false;
        
    }


    void Update()
    {
        MeshUpdate.UpdatePlayer();
    }



    // ----------------------------- Events ----------------------------


}
