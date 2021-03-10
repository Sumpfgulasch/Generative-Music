using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains ONE chord always.
/// </summary>
public class RecordObject : MonoBehaviour
{
    //[HideInInspector] 
    public bool isRecording = true;
    //[HideInInspector] 
    public bool hasRespawned = false;
    //[HideInInspector] 
    public RecordData data;
    
    public float StartZPos { get { return this.transform.position.z; } }
    public float EndZPos { get { return StartZPos + this.transform.localScale.z; } }
    private float DeltaTime { get { return Time.deltaTime * FPS; } }

    private float FPS;



    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;
    }


    
    void Update()
    {
        Move();
    }



    public RecordObject()
    {

    }




    // ------------------------------ Public functions ------------------------------


    public static void Create()
    {
        
    }




    // ------------------------------ Private functions ------------------------------



    private void Move()
    {
        this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    }

}
