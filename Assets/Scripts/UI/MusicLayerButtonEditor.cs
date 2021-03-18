using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(MusicLayerButton))]
public class MusicLayerButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //MusicLayerButton targetButton = (MusicLayerButton)target;

        //DrawDefaultInspector();
    }

}
#endif
