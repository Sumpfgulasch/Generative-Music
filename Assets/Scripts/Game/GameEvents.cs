using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour
{
    public static GameEvents inst;

    public Action onTunnelEnter;
    public Action onScreenResize;
    // Input
    public Action <Player.Side> onStartField_input;
    public Action<PlayerField> onChangeField_input;
    public Action onEndField_input;
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
    // Record
    public Action<RecordObject> onRecObjFieldEnter;
    public Action<RecordObject> onRecObjFieldExit;
    public Action<RecordObject> onRecObjScreenExit;
    public Action<MusicField> onPlayFieldByRecord;
    public Action<MusicField> onStopFieldByRecord;



    void Start()
    {
        inst = this;
    }

    public void TunnelStart()
    {
        onTunnelEnter?.Invoke();
    }

    public void StartField_input(Player.Side side)
    {
        onStartField_input?.Invoke(side);
    }

    public void ChangeField_input(PlayerField fieldData)
    {
        onChangeField_input?.Invoke(fieldData);
    }

    public void EndField_input()
    {
        onEndField_input?.Invoke();
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
