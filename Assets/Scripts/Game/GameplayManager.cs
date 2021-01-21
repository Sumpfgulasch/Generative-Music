using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{

    void Start()
    {
        // 1. Create meshes
        MeshCreation.CreatePlayerMeshes();

        // EVENTS
        MusicRef.inst.beatSequencer.beatEvent.AddListener(OnFirstBeat);
        GameEvents.inst.onTunnelEnter += OnTunnelStart;
        
    }


    void Update()
    {
        
    }



    private void OnFirstBeat(int beat)
    {
        // gefährlich; könnte nicht klappen, wenn durch lags hier noch kein tunnel-collider ist

        if (beat == 0)
        {
            // 1. Instantiate all fields (invisible)
            MeshCreation.CreateFields();

            // 2. Fill with content
            LoopData.Init();

            // 3. Move one after another to front & activate
        }
        
    }

    private void OnTunnelStart()
    {
        // 2. 
        MeshUpdate.UpdateFieldsPositions(); // brache nicht mehr das, anders
        
    }
}
