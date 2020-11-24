using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public List<AudioHelm.HelmController> instruments;
    enum Scale {Ionisch, Blues };
    Scale scale;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != null && instance != this)
            Destroy(instance);
        
    }

    // Update is called once per frame
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
            instrument.NoteOff(note);
            instrument.NoteOff(note+4);
            instrument.NoteOff(note+7);
        }
    }

    void GetRandomNoteFromScale(int rangeFrom, int rangeTo, Scale scale)
    {

    }
}
