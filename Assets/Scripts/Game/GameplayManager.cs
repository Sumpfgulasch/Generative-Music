using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{

    private void OnEnable()
    {
        
    }

    IEnumerator Start()
    {
        // Wait (cause raycasts wouldnt hit any collider)
        yield return new WaitForFixedUpdate(); yield return new WaitForEndOfFrame();
        
        // 1. Create all meshes
        MeshCreation.CreateAll();
        
        // 2. Fill fields with last content (chords & colors)
        LoopData.Init();
        
        // EVENTS
        GameEvents.inst.onFirstBeat += OnFirstBeat;
        GameEvents.inst.onSecondBeat += OnSecondBeat;

        yield return null;
    }


    void Update()
    {
        
    }



    private void OnFirstBeat()
    {
        // Player shrink animation
        StartCoroutine(Player.inst.DampedScale(Player.inst.scaleMin, 0.0f));
    }

    private void OnSecondBeat()
    {
        // Spawn Tunnels
        StartCoroutine(ObjectSpawner.inst.InstantiateFirstTunnels(0,2,2));      // initial
        GameEvents.inst.onBeat += ObjectSpawner.inst.OnBeat;                    // regular

        // Spawn fields
        StartCoroutine(ObjectSpawner.inst.SpawnMusicFields(TunnelData.fields, 3, 3, 1));
    }
}
