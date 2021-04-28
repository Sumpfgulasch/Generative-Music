using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopObject : MonoBehaviour
{
    public GameObject obj;
    public MeshRenderer meshRenderer;
    public Color startColor;

    //public LoopObject douplicate;

    public float loopStart;
    public float loopEnd_extended;

    public bool isActive = false;
    public bool hasRespawned = false;
    protected bool hasEnteredField = false;
    protected bool hasLeftField = false;
    protected bool hasLeftScreen = false;


    // Properties
    public float StartZPos { get { return transform.position.z; } }
    public float EndZPos { get { return StartZPos + transform.localScale.z; } }


    

    public void Update()
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
                isActive = true;
            }
        }

        // Exit
        if (EndZPos <= Player.inst.transform.position.z)
        {
            if (!hasLeftField)
            {
                GameEvents.inst.onRecObjFieldExit?.Invoke(this);
                hasLeftField = true;
                isActive = false;
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


    // public

    /// <summary>
    /// Instantiate a loopObject, add data and add to list.
    /// </summary>
    /// <param name="loopStart">Position in a sequencer.</param>
    /// <param name="loopEnd_extended">Position in a sequencer.</param>
    public static LoopObject Create (GameObject obj, Transform parent, Vector3 position, float loopStart, float loopEnd_extended, List<LoopObject> addToList)
    {
        // 1. GameObject
        var newObj = Instantiate(obj, parent);
        newObj.transform.position = position;

        // 2. Data
        var script = newObj.GetComponent<LoopObject>();
        script.obj = newObj;
        script.loopStart = loopStart;
        script.loopEnd_extended = loopEnd_extended;
        script.startColor = script.meshRenderer.material.color;

        // 3. Add to list
        addToList.Add(script);

        return script;
    }


}
