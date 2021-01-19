using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static GameEvents inst;

    public Action onTunnelStart;

    void Start()
    {
        inst = this;
    }

    public void TunnelStart()
    {
        onTunnelStart?.Invoke();
    }

    

    

}
