using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    private float FPS;
    private float DeltaTime { get { return Time.deltaTime * FPS; } }




    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;
    }

    



    void Update()
    {
        this.transform.position -= new Vector3(0, 0, ObjectManager.inst.moveSpeed * DeltaTime);
    }
}
