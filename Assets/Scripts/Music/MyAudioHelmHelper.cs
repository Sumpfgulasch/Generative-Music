using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;
using System.Linq;

public static class MyAudioHelmHelper // : MonoBehaviour
{



    public static void PlayChord(Chord chord, HelmController controller, float velocity)
    {
        for (int i = 0; i < chord.notes.Length; i++)
        {
            int note = chord.notes[i];
            if (controller.IsNoteOn(note))
            {
                controller.NoteOff(note);
            }

            controller.NoteOn(note, velocity);
        }
    }

    public static void StopChord(Chord chord, HelmController controller, Sequencer sequencer, bool forceNoteOff = false)
    {
        for (int i = 0; i < chord.notes.Length; i++)
        {
            int note = chord.notes[i];
            if (controller.IsNoteOn(note))
            {
                // 2. Stop only if the notes are NOT being played in the sequencer
                if (!sequencer.IsNoteOn(note) || forceNoteOff)
                {
                    controller.NoteOff(note);
                }

            }
        }
    }



    /// <summary>
    /// Get all the notes that play at the given sequencer position. Also those that extent over the sequencer-end.
    /// </summary>
    /// <param name="sequencer"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static List<Note> GetCurrentNotes(Sequencer sequencer, float pos)
    {
        var allNotes = sequencer.GetAllNotes();

        var notes = new List<Note>();

        foreach (Note note in allNotes)
        {
            if (note == null)
                continue;

            // Regular notes (start < end)
            if (pos >= note.start && pos <= note.end)
            {
                notes.Add(note);
            }
            // Notes that extend over the sequencer end
            else if (note.start > note.end)
            {
                if (pos >= note.start || pos <= note.end)
                    notes.Add(note);
            }
        }

        return notes;
    }

    /// <summary>
    /// Get all the currently unplayed notes, that extend over the sequencer-end.
    /// </summary>
    /// <param name="sequencer"></param>
    /// <param name="curPos"></param>
    /// <returns></returns>
    public static List<Note> UnplayedBridgeNotes(Sequencer sequencer, float curPos)
    {
        var allNotes = sequencer.GetAllNotes();
        var notes = new List<Note>();

        foreach (Note note in allNotes)
        {
            if (note.start > note.end)
            {
                if (curPos < note.start && curPos > note.end)
                    notes.Add(note);
            }
        }

        return notes;
    }

    public static bool IsUnplayedBridgeNote(this Note note, float curPos)
    {
        if (note.start > note.end)
        {
            if (curPos < note.start && curPos > note.end)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsUnplayedBridgeNote(this NoteContainer note, float curPos)
    {
        if (note.start > note.end)
        {
            if (curPos < note.start && curPos > note.end)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Removes those notes from the note-sequencer, that have the same start position.
    /// </summary>
    /// <param name="note"></param>
    /// <param name="doubleNotes"></param>
    /// <returns></returns>
    public static bool RemoveIdenticalStartNotes(Note note, List<Note> doubleNotes, Sequencer sequencer)
    {
        bool remove = false;
        foreach(Note doubleNote in doubleNotes)
        {
            if (doubleNote.note == note.note)
            {
                if (doubleNote.start == note.start || doubleNote.end == note.end)
                {
                    sequencer.RemoveNote(doubleNote);

                    MusicManager.inst.controller.NoteOff(note.note); // force note off, wont happen through the delayed remove of the sequencer note
                    remove = true;

                    //Debug.Log("remove: " + doubleNote.note);
                }
            }
        }

        return remove;
    }

    /// <summary>
    /// Removes those notes from the note-sequencer, that have the same start position.
    /// </summary>
    /// <param name="note"></param>
    /// <param name="doubleNotes"></param>
    /// <returns></returns>
    public static bool RemoveIdenticalStartNotes(this NoteContainer note, List<Note> doubleNotes, Sequencer sequencer)
    {
        bool remove = false;
        foreach (Note doubleNote in doubleNotes)
        {
            if (doubleNote.note == note.note)
            {
                if (doubleNote.start == note.start || doubleNote.end == note.end)
                {
                    sequencer.RemoveNote(doubleNote);

                    MusicManager.inst.controller.NoteOff(doubleNote.note); // force note off, wont happen through the delayed remove of the sequencer note
                    remove = true;
                }
            }
        }
        return remove;
    }






    /// <summary>
    /// Return the sequencer notes that have the some notes as the current recorded chord.
    /// </summary>
    /// <param name="recordNotes"></param>
    /// <param name="seqNotes"></param>
    /// <returns></returns>
    public static List<Note> DoubleNotes(int[] recordNotes, List<Note> seqNotes)
    {
        var doubleNotes = new List<Note>();
        foreach (Note note in seqNotes)
        {
            if (recordNotes.Contains(note.note))
                doubleNotes.Add(note);
        }

        //foreach (Note note in doubleNotes)
        //{
        //    Debug.Log("double note: " + note.note);
        //}

        return doubleNotes;
    }


    public static float NoteLength(Sequencer sequencer, float start, float end)
    {
        if (end < start)
        {
            end += sequencer.length;
        }
        
        return (end - start); // in sixteenth
    }


    /// <summary>
    /// Check if a given note is currently played in the given sequencer.
    /// </summary>
    /// <param name="note"></param>
    /// <param name="sequencer"></param>
    /// <returns></returns>
    public static bool IsNoteOn(this Sequencer sequencer, int note)
    {
        // 1. Check if the current notes are played in the sequencer
        var curPos = (float)sequencer.GetSequencerPosition();
        var curSeqNotes = GetCurrentNotes(sequencer, curPos);

        bool noteIsPlayed = false;

        foreach (Note sequencerNote in curSeqNotes)
        {
            if (sequencerNote.note == note)
            {
                noteIsPlayed = true;
                //Debug.Log("seq note.end: " + sequencerNote.end);
                break;
            }
        }

        return noteIsPlayed;
    }





}


public class NoteContainer
{
    public int note;
    public float start;
    public float end;
    public float velocity;
    //public Sequencer parent;
    public NoteContainer(int note, float start, float end, float velocity)
    {
        this.note = note;
        this.start = start;
        this.end = end;
        this.velocity = velocity;
        //this.parent = sequencer;
    }
}
