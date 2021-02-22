﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject start;
    public GameObject end;
    float FPS;

    private float deltaTime
    {
        get { return Time.deltaTime * FPS; }
    }


    private void Awake()
    {
        
    }


    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;
        var visualTiling = new Vector2(VisualController.inst.shapesPerBar, VisualController.inst.fieldsPerEdge);
        this.GetComponentInChildren<MeshRenderer>().material.SetVector("Tiling", visualTiling);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position -= new Vector3(0, 0, ObjectSpawner.inst.moveSpeed * deltaTime);
    }
}
