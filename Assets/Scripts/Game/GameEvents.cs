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
    public Action<PlayerField> onFieldChange;
    public Action onLeave;

    public Action onFirstBeat;
    public Action onSecondBeat;
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

    public void FieldChange(PlayerField fieldData)
    {
        onFieldChange?.Invoke(fieldData);
    }

    public void Leave()
    {
        onLeave?.Invoke();
    }

    





}
