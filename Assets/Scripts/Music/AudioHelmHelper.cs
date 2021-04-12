using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;
using System.Linq;

public static class AudioHelmHelper // : MonoBehaviour
{
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
            if (pos > note.start && pos < note.end)
            {
                notes.Add(note);
            }
            // Notes that extend over the sequencer end
            else if (note.start > note.end)
            {
                if (pos > note.start || pos < note.end)
                    notes.Add(note);
            }
        }

        return notes;



        //var allNotes = sequencer.GetAllNotes();
        //var notes = new List<Note>();

        //foreach (Note note in allNotes)
        //{
        //    if (curPos > note.start && curPos < note.end)
        //    {
        //        notes.Add(note);
        //    }
        //}

        //return notes;
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
    public static bool TryRemoveIdenticalStartNotes(this Note note, List<Note> doubleNotes, Sequencer sequencer)
    {
        bool remove = false;
        foreach(Note doubleNote in doubleNotes)
        {
            if (note.start == doubleNote.start)
            {
                sequencer.RemoveNote(doubleNote);
                remove = true;
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
    public static bool TryRemoveIdenticalStartNotes(this NoteContainer note, List<Note> doubleNotes, Sequencer sequencer)
    {
        bool remove = false;
        foreach (Note doubleNote in doubleNotes)
        {
            if (note.start == doubleNote.start)
            {
                sequencer.RemoveNote(doubleNote);
                remove = true;
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
