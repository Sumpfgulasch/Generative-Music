using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    //public static InputManager inst;
    // Start is called before the first frame update
    //void Start()
    //{
    //    //inst = this;
    //}

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    public static bool FastMoveStart()
    {
        // Mouse
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
            return true;
        else
            return false;
    }


    public static bool FastMoveEnd()
    {
        // Mouse
        if (Input.GetMouseButtonUp(1))
            return true;
        else
            return false;
    }

    public static bool ActionStart()
    {
        if (Input.GetMouseButtonDown(0))
            return true;
        else if (Input.GetKeyDown(KeyCode.Space))
            return true;
        else
            return false;
    }

    public static bool Action()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            return true;
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
            return true;
        else
            return false;
    }


    public static bool Reset()
    {
        if (Input.GetKeyDown(KeyCode.R))
            return true;
        else
            return false;
    }

    public static bool SelectClockWise()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            return true;
        else
            return false;
    }

    public static bool SelectCounterClockWise()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            return true;
        else
            return false;
    }
}
