using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicRef : MonoBehaviour
{
    public static MusicRef inst;

    [Header("References")]
    public List<AudioHelm.HelmController> helmControllers;
    public AudioHelm.SampleSequencer beatSequencer;
    public AudioHelm.AudioHelmClock clock;


    void Start()
    {
        inst = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
