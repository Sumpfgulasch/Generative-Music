using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordObject : MonoBehaviour
{
    public GameObject obj;
    public RecordObject douplicate;

    public float start; // All the positions are values in a sequencer.
    public float end;
    public float loopStart;
    public float loopEnd_extended;

    public bool isPlaying;
    public bool hasRespawned = false;
    protected bool hasEnteredField = false;
    protected bool hasLeftField = false;
    protected bool hasLeftScreen = false;


    // Properties
    public float StartZPos { get { return obj.transform.position.z; } }
    public float EndZPos { get { return StartZPos + obj.transform.localScale.z; } }


    void Start()
    {
        
    }

    private void Update()
    {
        InvokeFieldEvents();
    }



    /// <summary>
    /// Invoke Enter- and Exit-events for fields.
    /// </summary>
    private void InvokeFieldEvents()
    {
        // Enter
        if (StartZPos <= Player.inst.transform.position.z)
        {
            if (!hasEnteredField)
            {
                GameEvents.inst.onRecObjFieldEnter?.Invoke(this);
                hasEnteredField = true;
                isPlaying = true;
            }
        }

        // Exit
        if (EndZPos <= Player.inst.transform.position.z)
        {
            if (!hasLeftField)
            {
                GameEvents.inst.onRecObjFieldExit?.Invoke(this);
                hasLeftField = true;
                isPlaying = false;
            }
        }

        // Exit screen
        if (EndZPos <= -2f)
        {
            if (!hasLeftScreen)
            {
                GameEvents.inst.onRecObjScreenExit?.Invoke(this);
                hasLeftScreen = true;
            }
        }
    }


}
