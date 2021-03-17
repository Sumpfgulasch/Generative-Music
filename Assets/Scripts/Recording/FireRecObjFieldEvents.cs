using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRecObjFieldEvents : MonoBehaviour
{
    // Properties
    public float StartZPos { get { return transform.position.z; } }
    public float EndZPos { get { return StartZPos + transform.localScale.z; } }

    void Start()
    {
        
    }

    

    void Update()
    {
        if (StartZPos <= Player.inst.transform.position.z)
        {
            //GameEvents.inst.onRecObjFieldEnter?.Invoke();
        }
    }
}
