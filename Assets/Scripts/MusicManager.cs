using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public List<AudioHelm.HelmController> controllers;
    enum Scale {Ionisch, Blues };
    Scale scale;

    public float shortNotes_minPlayTime = 0.3f;
    public int maxEdgeIntervalRange = 14;

    // private
    float minPitch, maxPitch;

    // get set
    Player player { get { return Player.instance; } }

    
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != null && instance != this)
            Destroy(instance);
        
    }
    
    void Update()
    {
        
    }

    public void PlaySingleNote(AudioHelm.HelmController instrument, int note, float velocity)
    {
        if (!instrument.IsNoteOn(note))
        {
            instrument.NoteOn(note, velocity);
            instrument.NoteOn(note+4, velocity);
            instrument.NoteOn(note+7, velocity);
        }
    }

    public void StopSingleNote(AudioHelm.HelmController instrument, int note)
    {
        if (instrument.IsNoteOn(note))
        {
            float timeToPlay = shortNotes_minPlayTime - instrument.pressedNotesDurations[note].duration;
            if (timeToPlay > 0)
            {
                instrument.WaitNoteOff(note, timeToPlay);
                instrument.WaitNoteOff(note+4, timeToPlay);
                instrument.WaitNoteOff(note+7, timeToPlay);
            }
            else
            {
                instrument.NoteOff(note);
                instrument.NoteOff(note + 4);
                instrument.NoteOff(note + 7);
            }
            
        }
        
    }

    void GetRandomNoteFromScale(int rangeFrom, int rangeTo, Scale scale)
    {
        
    }

    void GenerateMusicalRange(int note)
    {
        
        minPitch = Random.Range(note - 7, note);
        maxPitch = Random.Range(note, note + 7);
    }

    public void SetPitchOnEdge(int note, AudioHelm.HelmController controller)
    {
        float percentage;
        percentage = (player.outerVertices[0] - player.curEnvEdge.Item1).magnitude / (player.curEnvEdge.Item2 - player.curEnvEdge.Item1).magnitude;

        if (player.firstEdgeTouch)
        {
            minPitch = Random.Range(-maxEdgeIntervalRange, 0) * percentage;
            float maxPitchVertex2playerVertex_dist = (player.curEnvEdge.Item2 - player.outerVertices[0]).magnitude;
            float minPitchVertex2playerVertex_dist = (player.curEnvEdge.Item1 - player.outerVertices[0]).magnitude;
            maxPitch = Mathf.Abs(minPitch) * (maxPitchVertex2playerVertex_dist / minPitchVertex2playerVertex_dist);
            print("first Edge touch");
        }
        else if (player.edgeChange)
        {
            print("edgeChange");
            if (player.curRotSpeed < 0)
            {
                // im Uhrzeigersinn
                minPitch = maxPitch;
                maxPitch = maxPitch + Random.Range(0, maxEdgeIntervalRange);
            }
            else
            {
                // gegen Uhrzeigersinn
                maxPitch = minPitch;
                minPitch = minPitch + Random.Range(0, -maxEdgeIntervalRange);
            }
        }
        float pitch = percentage.Remap(0, 1, minPitch, maxPitch);
        controller.SetPitchWheel(pitch);
    }
}
