using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager inst;

    [HideInInspector] public List<MusicLayerButton> layerButtons;
    public MusicLayerButton activeLayerButton;
    public float musicLayerButton_waitBeforDelete = 0.2f;
    public float musicLayerButton_duration = 1f;

    public AnimationCurve deleteLerp;

    void Start()
    {
        inst = this;

        layerButtons = MeshRef.inst.layerButtons;
    }

    
    void Update()
    {
        
    }
}
