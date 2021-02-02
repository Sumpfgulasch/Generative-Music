using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// ----------------------------- Edge Part ----------------------------

public class MusicField
{
    public enum Type { Chord, Modulation, Pitch};

    // Properties
    public int ID;
    public Type type;
    public Vector3 start;
    public Vector3 end;
    public Vector3 mid;
    public Vector3[] positions;
    public bool isCorner;
    public bool isEdgeMid;
    public LineRenderer lineRend;
    public Chord chord;
    public bool selectable;
    public bool isBuildingUp; // -> is playable

    // Private variables
    public Color color;




    // Contructors
    public MusicField()
    {
        
    }

    public MusicField(Type type)
    {
        this.type = type;
    }

    public MusicField(int ID, LineRenderer lineRend, bool isCorner, bool isEdgeMid)
    {
        this.ID = ID;
        this.lineRend = lineRend;
        this.isCorner = isCorner;
        this.isEdgeMid = isEdgeMid;
    }




    // ----------------------------- public functions ----------------------------




    /// <summary>
    /// Set line renderer and position-variables. Set z-position to fieldsBeforeSurface-value.
    /// </summary>
    public void UpdateVertices(Vector3 start, Vector3 mid, Vector3 end, Vector3[] positions)
    {
        // 1. Set z-position
        float zPos = Player.inst.transform.position.z - VisualController.inst.fieldsBeforeSurface;
        start.z = zPos;
        mid.z = zPos;
        end.z = zPos;
        for (int i=0; i<positions.Length; i++)
            positions[i].z = zPos;
        
        // 2. Assign
        this.start = start;
        this.mid = mid;
        this.end = end;
        this.positions = positions;

        // 3. Set line renderer
        var linePositions = MeshUpdate.PreventLineRendFromBending(positions);
        lineRend.positionCount = linePositions.Length;
        lineRend.SetPositions(linePositions);
    }


    /// <summary>
    /// Store color and change material color of line renderer.
    /// </summary>
    public void SetColor(Color color)
    {
        this.color = color;
        this.lineRend.material.color = color;
    }

    /// <summary>
    /// Set the z-positions of the line renderer.
    /// </summary>
    public void SetZPos(float zPos)
    {
        for (int i=0; i<lineRend.positionCount; i++)
        {
            var newPos = lineRend.GetPosition(i);
            newPos.z = zPos;
            lineRend.SetPosition(i, newPos);
        }
    }

    /// <summary>
    /// Set data and set material color.
    /// </summary>
    public void SetContent(Type fieldType, Chord chord, Color color, bool selectable, bool isBuildingUp)
    {
        this.type = fieldType;
        this.chord = chord;
        this.color = color;
        this.selectable = selectable;
        this.isBuildingUp = isBuildingUp;

        this.lineRend.material.color = color;
    }

    public static bool IsCorner(int ID)
    {
        #region old
        //if ((ID + 1) % VisualController.inst.fieldsPerEdge == 0 || ID % VisualController.inst.fieldsPerEdge == 0)
        //{
        //    return true;
        //}
        //else
        //{
        //    return false;
        //}
        #endregion

        if (ID % (VisualController.inst.fieldsPerEdge - 1) == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsEdgeMid(int ID)
    {
        int testID = ID + ((VisualController.inst.fieldsPerEdge - 1) / 2);

        if (testID % (VisualController.inst.fieldsPerEdge - 1) == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //public static bool IsCorner_RightPart(int ID)
    //{
    //    if (ID % VisualController.inst.fieldsPerEdge == 0)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    //public static Vector3 CornerPosition(int ID)
    //{
    //    if (!IsCorner(ID)) 
    //    {
    //        Debug.Log("Error! Wrong ID, no corner.");
    //        return Vector3.zero;
    //    }
    //    else if(IsCorner_RightPart(ID))
    //    {
    //        return TunnelData.fields[ID].start;
    //    }
    //    else
    //        return TunnelData.fields[ID].end;
    //}


    //private static Vector3 RegularFieldMid(int ID)
    //{
    //    Vector3 position = (TunnelData.fields[ID].start + TunnelData.fields[ID].end) / 2f;

    //    return position;
    //}


    /// <summary>
    /// Returns the mid point of any field, given by an ID. Respect corner IDs, too.
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    //public static Vector3 FieldMid(int targetID)
    //{
    //    if (IsCorner(targetID))
    //    {
    //        return CornerPosition(targetID);
    //    }
    //    else
    //    {
    //        return RegularFieldMid(targetID);
    //    }



    //}

    /// <summary>
    /// Get the adjacent field ID to a given ID.
    /// </summary>
    public static int NextFieldID(int curID, int direction)
    {
        //int nextID = (curID + direction).Modulo(VisualController.inst.FieldsCount);
        //if (IsCorner(curID) && IsCorner(nextID))
        //{
        //    nextID = (nextID + (int)Mathf.Sign(direction)).Modulo(VisualController.inst.FieldsCount);
        //}
        //return nextID;

        int nextID = ExtensionMethods.Modulo(curID + direction, TunnelData.FieldsCount);

        return nextID;
    }
}



// -------------------------- Player Edge Part -------------------------



public class PlayerField : MusicField
{
    public new enum Type { Main, Second };
    public new Type type;
    public Vector3[] secondaryPositions;

    // Contructors
    public PlayerField(Type type, LineRenderer lineRend)
    {
        this.type = type;
        base.lineRend = lineRend;
    }

    public PlayerField(int ID, Vector3[] positions, Vector3 mid)
    {
        this.ID = ID;
        this.positions = positions;
        this.mid = mid;
    }


    // Functions

    /// <summary>
    /// Set variables. Set z-position.
    /// </summary>
    public void Set(int ID, Vector3[] positions, Vector3 mid, bool isCorner)
    {
        this.ID = ID;
        this.positions = positions;
        this.mid = mid;
        this.isCorner = isCorner;

        for (int i=0; i< positions.Length; i++)
            this.positions[i].z = Player.inst.transform.position.z - VisualController.inst.playerFieldBeforeSurface;
    }

    public void SetVisible(bool value)
    {
        this.lineRend.enabled = value;
    }

    
    /// <summary>
    /// Set line renderer positions. Change z-position.
    /// </summary>
    public void SetToFocus()
    {
        this.lineRend.startWidth = VisualController.inst.playerFieldFocusThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldFocusThickness;
        SetOpacity(1f);

        float zPos = Player.inst.transform.position.z - VisualController.inst.playerFieldBeforeSurface;
        SetZPos(zPos);
    }

    /// <summary>
    /// Set line renderer positions. Change z-position.
    /// </summary>
    public void SetToPlay()
    {
        this.lineRend.startWidth = VisualController.inst.playerFieldPlayThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldPlayThickness;

        float zPos = Player.inst.transform.position.z - (VisualController.inst.fieldsBeforeSurface + 0.001f);
        SetZPos(zPos);
        
        SetOpacity(1);
    }


    /// <summary>
    /// Set z pos only.
    /// </summary>
    new public void SetZPos(float zPos)
    {
        for (int i = 0; i < this.lineRend.positionCount; i++)
        {
            Vector3 newPos = this.lineRend.GetPosition(i);
            newPos.z = zPos;
            this.lineRend.SetPosition(i, newPos);
        }
    }


    /// <summary>
    /// Set opacity of the line renderer material.
    /// </summary>
    /// <param name="opacity">Opacity. 0 == transparent [0, 1].</param>
    public void SetOpacity(float opacity)
    {
        Color newColor = this.lineRend.material.color;
        newColor.a = opacity;
        this.lineRend.material.color = newColor;
    }



}


// -------------------------------- Edge -------------------------------


public class Edge
{
    public int ID;
    public bool changed;
    public float percentage;
    public Vector3 start, end;
    public bool firstTouch;
    public bool leave;

    public void Set (Vector3 start, Vector3 end, bool changed, float percentage)
    {
        this.start = start;
        this.end = end;
        this.changed = changed;
        this.percentage = percentage;
    }
}



// --------------------------- Music field set --------------------------



public static class MusicFieldSet
{
    // == Operations for complete MusicFieldSets
    

    /// <summary>
    /// Store data (chords, colors, types, available, isBuildingUp) in each field; data only, no material assignments
    /// </summary>
    /// <param name="fieldsToAssign">Length == edges * divisions (att 15).</param>
    /// <param name="chords">First array.length == types.length, second arrays.length == vary.</param>
    /// <param name="colors">Length == types.length.</param>
    /// <param name="availables">Length == edges * divisions (att 15).</param>
    /// <param name="buildUps">Length == edges * divisions (att 15)</param>
    public static MusicField[] StoreDataInFields(MusicField[] fieldsToAssign, MusicField.Type[] fieldTypes, Chord[][] chords, Color[] colors, bool[] availables, bool [] buildUps)
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
                    ID = chordIndex * (fieldsPerEdge-1);
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



}
