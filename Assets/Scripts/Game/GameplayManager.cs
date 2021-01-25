﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{

    private void OnEnable()
    {
        
    }

    void Start()
    {
        // 1. Create meshes & Instantiate all fields (invisible)
        MeshCreation.CreatePlayerMeshes();
        MeshCreation.InitFields();

        // 2. Fill with content
        LoopData.Init();


        // EVENTS
        GameEvents.inst.onFirstBeat += OnFirstBeat;
    }


    void Update()
    {
        
    }



    private void OnFirstBeat()
    {
        MeshUpdate.UpdateFieldsPositions();

        //ObjectSpawner.inst.InstantiateFirstObjects_beat();

        StartCoroutine(Player.inst.DampedScale(Player.inst.scaleMin, 0.0f));

        // 3. Move one after another to front & activate
        StartCoroutine(ObjectSpawner.inst.SpawnMusicFields(TunnelData.fields, 3, 1));
    }
}
