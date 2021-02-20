﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRef : MonoBehaviour
{
    public static MeshRef inst;

    [Header("Masks & meshes")]
    public MeshFilter innerPlayerMesh_mf;
    public MeshFilter innerPlayerMask_mf;
    public MeshFilter innerSurface_mf;
    public MeshFilter innerMask_mf;
    public MeshFilter outerPlayerMesh_mf;
    public MeshFilter outerPlayerMask_mf;
    [Space]
    public Transform milkSurface_parent;
    public LineRenderer tunnelEdges_lr;
    [Space]
    [Header("Fields & lane surfaces")]
    public Transform playerField_parent;
    public Material playerField_mat;
    public Material playerFieldSec_mat;
    public Transform musicFields_parent;
    public Material musicFields_mat;
    public Transform highlightSurfaces_parent;
    public Material highlightSurfaces_mat;
    [Header("Diverse")]
    public PolygonCollider2D mouseColllider;


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
