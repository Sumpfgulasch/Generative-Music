using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager inst;

    [Header("Spawning")]
    public int timeToSpawnTunnel_inBeats = 8;
    public float tunnelSpawnDistance_InBeats = 2;
    public int maxTunnelsAtOnce = 2;
    [Space]
    public float timeToSpawnFields_inBeats = 3;
    public float fieldsSpawnDistance_inBeats = 3f;
    public float fieldsSpawnDuration_inBeats = 1f;

    private void OnEnable()
    {
        
    }

    IEnumerator Start()
    {
        inst = this;

        // Wait (cause raycasts wouldnt hit any collider)
        yield return new WaitForFixedUpdate(); yield return new WaitForEndOfFrame();
        
        // 1. Create all meshes
        MeshCreation.CreateAll();
        
        // 2. Fill fields with last content (chords & colors)
        LoopData.Init();
        #region hack: isSpawning = true
        for (int i=0; i< TunnelData.fields.Length; i++)
        {
            Player.inst.curFieldSet[i].isSpawning = true;
        }
        #endregion

        // EVENTS
        GameEvents.inst.onFirstBeat += OnFirstBeat;
        GameEvents.inst.onSecondBeat += OnSecondBeat;
        GameEvents.inst.onFieldStart += VisualController.inst.OnPlayStart;
        GameEvents.inst.onFieldLeave += VisualController.inst.OnPlayEnd;
        GameEvents.inst.onScreenResize += CameraOps.PanCamera;

        yield return null;
    }


    void Update()
    {
        
    }



    private void OnFirstBeat()
    {
        // Player shrink animation
        StartCoroutine(Player.inst.DampedScale(Player.inst.scaleMin, 0.0f));


        // HIER WEITER


        ////print("arp value before: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpOn));
        //MusicManager.inst.curInstrument.SetParameterValue(AudioHelm.Param.kArpOn, 1);
        ////print("arp value after: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpOn));
        //print("arp sync: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpSync));
        //MusicManager.inst.curInstrument.SetParameterValue(AudioHelm.Param.kArpSync, 0);
        //MusicManager.inst.curInstrument.SetParameterValue(AudioHelm.Param.kArpFrequency, 2);
        //print("arp freq: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpFrequency));

    }

    // start on second beat spawning stuff because first beat is not synced yet
    private void OnSecondBeat()
    {
        // Spawn Tunnels
        StartCoroutine(ObjectSpawner.inst.InstantiateFirstTunnels(timeToSpawnTunnel_inBeats, tunnelSpawnDistance_InBeats));     // initial
        // Regular event subscription in ObjectSpawner-coroutine (!!)

        // Spawn fields
        StartCoroutine(ObjectSpawner.inst.SpawnMusicFields(TunnelData.fields, timeToSpawnFields_inBeats, fieldsSpawnDistance_inBeats, fieldsSpawnDuration_inBeats));
    }
}
