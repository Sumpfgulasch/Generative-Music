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
    /// <param name="end"></param>
    /// <returns></returns>
    public static List<Note> GetAllNotesInRange(Sequencer sequencer, float end)
    {
        var allNotes = sequencer.GetAllNotes();
        var notes = new List<Note>();

        foreach (Note note in allNotes)
        {
            // Regular notes (start < end)
            if (end > note.start && end < note.end)
            {
                notes.Add(note);
            }
            // Notes that extend over the sequencer end
            else if (note.start > note.end)
            {
                if (end > note.start || end < note.end)
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

    /// <summary>
    /// Return the sequencer notes that have the some notes as the current recorded chord.
    /// </summary>
    /// <param name="requordNotes"></param>
    /// <param name="seqNotes"></param>
    /// <returns></returns>
    public static List<Note> DoubleNotes(int[] requordNotes, List<Note> seqNotes)
    {
        var doubleNotes = new List<Note>();
        foreach (Note note in seqNotes)
        {
            if (requordNotes.Contains(note.note))
                doubleNotes.Add(note);
        }

        //foreach (Note note in doubleNotes)
        //{
        //    Debug.Log("double note: " + note.note);
        //}

        return doubleNotes;
    }





}


public class NoteContainer
{
    public int note;
    public float start;
    public float end;
    public float velocity;
    public NoteContainer(int note, float start, float end, float velocity)
    {
        this.note = note;
        this.start = start;
        this.end = end;
        this.velocity = velocity;
    }
}
