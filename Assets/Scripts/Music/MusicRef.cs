using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicRef : MonoBehaviour
{
    public static MusicRef inst;

    [Header("References")]
    public List<AudioHelm.HelmController> helmControllers;
    public AudioHelm.SampleSequencer beatSequencer;
    public AudioHelm.AudioHelmClock clock;
    public AudioMixer mixer;


    void Start()
    {
        inst = this;
    }

    
    void Update()
    {
       
    }
}
