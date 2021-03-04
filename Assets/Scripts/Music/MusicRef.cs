using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AudioHelm;

public class MusicRef : MonoBehaviour
{
    public static MusicRef inst;

    [Header("References")]
    public List<HelmController> helmControllers;
    public List<Sequencer> sequencers;
    public SampleSequencer beatSequencer;
    public AudioHelmClock clock;
    public AudioMixer mixer;

    [Header("Other")]
    public HelmPatch helmPatch1;
    public HelmPatch helmPatch2;


    void Start()
    {
        inst = this;
    }

    
    void Update()
    {
       
    }
}
