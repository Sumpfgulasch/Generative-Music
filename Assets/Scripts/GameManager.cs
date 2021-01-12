using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;
    //[HideInInspector] public PlayerControls playerControls;

    [HideInInspector] public int FPS;

    private void Awake()
    {
        inst = this;

        FPS = Screen.currentResolution.refreshRate;
    }

    private void OnEnable()
    {
        //playerControls = new PlayerControls();
        //playerControls.Enable();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
