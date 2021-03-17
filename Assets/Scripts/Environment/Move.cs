using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    private float FPS;

    // Properties
    private float DeltaTime { get { return Time.deltaTime * FPS; } }
    public float StartZPos { get { return transform.position.z; } }
    public float EndZPos { get { return StartZPos + transform.localScale.z; } }


    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;
    }

    



    void Update()
    {
        MoveForward();


    }




    private void MoveForward()
    {
        this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    }
}
