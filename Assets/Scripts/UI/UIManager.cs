using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager inst;

    public Image activeTrack;

    void Start()
    {
        inst = this;
    }

    
    void Update()
    {
        
    }
}
