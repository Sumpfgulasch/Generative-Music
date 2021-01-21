using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    [HideInInspector] public int FPS;

    private void Awake()
    {
        inst = this;

        FPS = Screen.currentResolution.refreshRate;
    }

    private void OnEnable()
    {
        
    }

    void Start()
    {
        
    }

    


    void Update()
    {
        
    }

    
}
