using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordObject : MonoBehaviour
{
    [HideInInspector] public bool isRecording = true;
    [HideInInspector] public bool hasRespawned = false;
    
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



    // ------------------------------ Private functions ------------------------------



    private void Move()
    {
        this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    }

}
