using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static GameEvents inst;

    public Action onTunnelEnter;
    public Action onFieldStart;
    public Action<PlayerField> onFieldChange;
    public Action onFieldLeave;

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

    public void FieldStart()
    {
        onFieldStart?.Invoke();
    }

    public void FieldChange(PlayerField fieldData)
    {
        onFieldChange?.Invoke(fieldData);
    }

    public void FieldLeave()
    {
        onFieldLeave?.Invoke();
    }

    





}
