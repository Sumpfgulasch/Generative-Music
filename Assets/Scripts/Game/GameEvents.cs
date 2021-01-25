using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static GameEvents inst;

    public Action onTunnelEnter;
    public Action onFirstTouch;
    public Action onEdgePartChange;
    public Action onLeave;

    public Action onFirstBeat;
    public Action<int> onBeat;

    void Start()
    {
        inst = this;
    }

    public void TunnelStart()
    {
        onTunnelEnter?.Invoke();
    }

    public void FirstTouch()
    {
        onFirstTouch?.Invoke();
    }

    public void EdgePartChange()
    {
        onEdgePartChange?.Invoke();
    }

    public void Leave()
    {
        onLeave?.Invoke();
    }

    





}
