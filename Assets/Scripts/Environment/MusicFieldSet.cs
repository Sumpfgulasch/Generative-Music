using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MusicFieldSet
{
    // == Operations for complete MusicFieldSets


    /// <summary>
    /// Store data (chords, colors, types, available, isBuildingUp) in each field; assign materials;
    /// </summary>
    /// <param name="fieldsToAssign">Length == edges * divisions (atm 15).</param>
    /// <param name="chords">First array.length == types.length, second arrays.length == vary.</param>
    /// <param name="colors">Length == types.length.</param>
    /// <param name="availables">Length == edges * divisions (atm 15).</param>
    /// <param name="buildUps">Length == edges * divisions (atm 15)</param>
    public static MusicField[] SetDataToFields(MusicField[] fieldsToAssign, MusicField.Type[] fieldTypes, Chord[][] chords, Color[] colors, bool[] availables, bool[] buildUps)
    {
        var fieldIDs = ExtensionMethods.IntToList(TunnelData.FieldsCount, true);
        var fieldsPerEdge = VisualController.inst.fieldsPerEdge;
        var fieldsCount = TunnelData.FieldsCount;

        // 1. Gehe jeden chordType durch (3)
        for (int chordTypeIndex = 0; chordTypeIndex < chords.Length; chordTypeIndex++)
        {
            var curChords = chords[chordTypeIndex].ToList();

            // 2. Gehe jeden chord durch (3-5)
            for (int chordIndex = 0; chordIndex < chords[chordTypeIndex].Length; chordIndex++)
            {
                var chord = curChords[0];
                curChords.RemoveAt(0);
                int ID;

                // get CORNER fields
                if (chordTypeIndex == 0)
                {
                    ID = chordIndex * (fieldsPerEdge - 1);
                    fieldIDs.Remove(ID);
                }
                // get random other field
                else
                {
                    int randID_index = Random.Range(0, fieldIDs.Count);
                    ID = fieldIDs[randID_index];
                }

                fieldIDs.Remove(ID);

                fieldsToAssign[ID].SetContent(fieldTypes[ID], chord, colors[chordTypeIndex], availables[ID], buildUps[ID]);
            }
        }

        return fieldsToAssign;
    }


    /// <summary>
    /// Array of similar colors, sorted from 0 to last ID.
    /// </summary>
    public static Color[] ColorSet()
    {


        return null;
    }
}
