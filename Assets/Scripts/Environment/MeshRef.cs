using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRef : MonoBehaviour
{
    public static MeshRef inst;

    [Header("References")]
    public MeshFilter innerPlayerMesh_mf;
    public MeshFilter innerPlayerMask_mf;
    public MeshFilter innerSurface_mf;
    public MeshFilter innerMask_mf;
    public MeshFilter outerPlayerMesh_mf;
    public MeshFilter outerPlayerMask_mf;
    [Space]
    public LineRenderer envEdges_lr;
    [Space]
    public Transform curEdgeParts_parent;
    public Material curEdgePart_mat;
    public Material curSecEdgePart_mat;
    [Space]
    public Transform envEdgeParts_parent;
    public Material envEdgePart_mat;

    
    // Start is called before the first frame update
    void Start()
    {
        inst = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
