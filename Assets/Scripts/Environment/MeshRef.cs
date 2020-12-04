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
    public Transform edgeParts_parent;
    public Material envEdgePart_mat;
    public LineRenderer envEdges_lr; // to do: in mat umwandeln, player on runtime
    public LineRenderer curEdgePart_lr;
    public List<LineRenderer> curEdgePart2nd_lr;
    
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
