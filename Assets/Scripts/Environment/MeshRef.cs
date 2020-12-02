using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRef : MonoBehaviour
{
    public static MeshRef instance;

    [Header("References")]
    public MeshFilter innerPlayerMesh_mf;
    public MeshFilter innerPlayerMask_mf;
    public MeshFilter innerSurface_mf;
    public MeshFilter innerMask_mf;
    public MeshFilter outerPlayerMesh_mf;
    public MeshFilter outerPlayerMask_mf;
    public LineRenderer envEdges_lr;
    public LineRenderer curEdgePart_lr;
    public List<LineRenderer> curEdgePart2nd_lr;
    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
