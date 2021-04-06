using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager inst;

    [Header("Spawning")]
    public int timeToSpawnTunnel_inQuarters = 0;
    public float tunnelSpawnDistance_inQuarters = 4;
    public int maxTunnelsAtOnce = 2;
    [Space]
    public float timeToSpawnFields_inQuarters = 0;
    public float fieldsSpawnDistance_inQuarters = 2f;
    public float fieldsSpawnDuration_inQuarters = 1f;

    

    IEnumerator Start()
    {
        inst = this;

        var cursorOffset = new Vector2(MeshRef.inst.mouse.width / 2, MeshRef.inst.mouse.height / 2);
        Cursor.SetCursor(MeshRef.inst.mouse, cursorOffset, CursorMode.Auto);

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
        MusicRef.inst.beatSequencer.beatEvent.AddListener(MusicManager.inst.OnBeat);
        MusicRef.inst.beatSequencer.beatEvent.AddListener(MusicManager.inst.OnVeryFirstBeats);
        GameEvents.inst.onVeryFirstBeat += OnVeryFirstBeat;
        GameEvents.inst.onVerySecondBeat += OnVerySecondBeat;
        GameEvents.inst.onPlayPerformed += VisualController.inst.OnPlayStart;
        GameEvents.inst.onPlayCanceled += VisualController.inst.OnPlayEnd;
        GameEvents.inst.onChangeField += VisualController.inst.OnFieldChange;
        GameEvents.inst.onMouseInside += VisualController.inst.OnMouseInside;
        GameEvents.inst.onMouseOutside += VisualController.inst.OnMouseOutside;
        GameEvents.inst.onScreenResize += CameraOps.PanCamera;
        
        // Music-related
        GameEvents.inst.onPlayPerformed += MusicManager.inst.OnFieldStart;
        GameEvents.inst.onChangeField += MusicManager.inst.OnFieldChange;
        GameEvents.inst.onPlayCanceled += MusicManager.inst.OnFieldLeave;

        GameEvents.inst.onScreenResize?.Invoke(); // Hack

        //GameEvents.inst.onStopField += Crap;

        yield return null;
    }


    void Update()
    {
        
    }

    //void Crap()
    //{
    //    print("onStopField event");
    //}



    private void OnVeryFirstBeat()
    {
        // Player shrink animation
        StartCoroutine(Player.inst.DampedScale(Player.inst.scaleMin, 0.0f));


        // HIER WEITER

        // Tests to control audiohelm parameters

        ////print("arp value before: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpOn));
        //MusicManager.inst.curInstrument.SetParameterValue(AudioHelm.Param.kArpOn, 1);
        ////print("arp value after: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpOn));
        //print("arp sync: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpSync));
        //MusicManager.inst.curInstrument.SetParameterValue(AudioHelm.Param.kArpSync, 0);
        //MusicManager.inst.curInstrument.SetParameterValue(AudioHelm.Param.kArpFrequency, 2);
        //print("arp freq: " + MusicManager.inst.curInstrument.GetParameterValue(AudioHelm.Param.kArpFrequency));

    }

    // start on second beat spawning stuff because first beat is not synced yet
    private void OnVerySecondBeat()
    {
        // Spawn Tunnels
        StartCoroutine(ObjectManager.inst.SpawnFirstTunnels(maxTunnelsAtOnce, timeToSpawnTunnel_inQuarters, tunnelSpawnDistance_inQuarters));     // initial
        // Regular event subscription in ObjectSpawner-coroutine (!!)

        // Spawn fields
        StartCoroutine(ObjectManager.inst.SpawnMusicFields(TunnelData.fields, timeToSpawnFields_inQuarters, fieldsSpawnDistance_inQuarters, fieldsSpawnDuration_inQuarters));
        
        // show beat event
        int beatsToWait = (timeToSpawnTunnel_inQuarters + (int) tunnelSpawnDistance_inQuarters) * 4 -4;
        
        StartCoroutine(GameEvents.inst.OnQuarter_subscribeDelayed(MeshUpdateMono.inst.ShowBeat, beatsToWait));
    }
}
