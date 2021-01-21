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
    public float edgePartThickness = 0.01f;
    [Range(0.001f, 0.05f)]
    public float playerEdgePartThickness = 0.03f;

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
        

        // EVENT subscription
        GameEvents.inst.onTunnelEnter += OnTunnelStart;
    }


    void Update()
    {
        MeshUpdate.UpdatePlayer();
    }



    // ----------------------------- Events ----------------------------



    private void OnTunnelStart()
    {
        
    }
}
