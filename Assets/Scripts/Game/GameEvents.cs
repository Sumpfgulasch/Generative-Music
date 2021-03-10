using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static GameEvents inst;

    public Action onTunnelEnter;
    // Input
    public Action <Player.Side> onFieldStart;
    public Action<PlayerField> onFieldChange;
    public Action onFieldLeave;
    // Play fields
    public Action onPlayField;
    public Action onStopField;
    // Beats
    public Action onVeryFirstBeat;
    public Action onVerySecondBeat;
    public Action<int> onSixteenth;
    public Action onQuarter;
    public Action onFirstBeat;
    public Action onMouseInside;
    public Action onMouseOutside;

    public Action onScreenResize;

   

    void Start()
    {
        inst = this;
    }

    public void TunnelStart()
    {
        onTunnelEnter?.Invoke();
    }

    public void FieldStart(Player.Side side)
    {
        onFieldStart?.Invoke(side);
    }

    public void FieldChange(PlayerField fieldData)
    {
        onFieldChange?.Invoke(fieldData);
    }

    public void FieldLeave()
    {
        onFieldLeave?.Invoke();
    }

    public void MouseOutside()
    {
        onMouseOutside?.Invoke();
    }

    public void MouseInside()
    {
        onMouseInside?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action">Event.</param>
    /// <param name="beatsToWait">In Sixteenth.</param>
    /// <returns></returns>
    public IEnumerator OnQuarter_subscribeDelayed(Action function, int beatsToWait)
    {
        beatsToWait -= 1; // sonst kriegt er den ersten call nicht

        float targetBeat = MusicManager.inst.curBeat + beatsToWait;
        while (MusicManager.inst.curBeat < targetBeat)
        {
            yield return null;
        }
        onQuarter += function;
    }

    //public void Test(Action action, Action actionSubscribing)
    //{
    //    action += actionSubscribing;
    //}

    //public void TestTest()
    //{
    //    Test(onQuarter, MusicManager.inst.QuantizeSequence);
    //}

    public void PrintSth()
    {
        print("print sth");
    }


}
