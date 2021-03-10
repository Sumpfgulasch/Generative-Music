using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTunnel : MonoBehaviour
{
    public Transform startPos;
    public Transform endPos;
    
    private float FPS;

    private float DeltaTime
    {
        get { return Time.deltaTime * FPS; }
    }


    private void Awake()
    {
        
    }


    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;

        // SET SHADER TILING (according to VisualController)
        var visualTiling = new Vector2(VisualController.inst.shapesPerBar, VisualController.inst.fieldsPerEdge);
        this.GetComponentInChildren<MeshRenderer>().material.SetVector("Tiling", visualTiling);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    }
}
