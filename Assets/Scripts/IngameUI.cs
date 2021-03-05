using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class IngameUI : MonoBehaviour
{
    
    void Start()
    {
        
    }


    public void ClearSequencer(int layer)
    {
        MusicRef.inst.sequencers[layer].Clear();
    }
    
}
