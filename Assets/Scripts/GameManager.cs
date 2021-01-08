using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    [HideInInspector] public int FPS;

    void Start()
    {
        inst = this;

        FPS = Screen.currentResolution.refreshRate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
