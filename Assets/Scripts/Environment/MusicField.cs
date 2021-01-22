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
    public bool isCorner;
    public bool isEdgeMid;
    public LineRenderer lineRend;
    public Chord chord;
    public bool available;
    public bool isBuildingUp;

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
    /// Sets vertices variables.
    /// </summary>
    /// <param name="start">Start Position.</param>
    /// <param name="end">End position.</param>
    public void UpdateVertices(Vector3 start, Vector3 end)
    {
        start.z = Player.inst.transform.position.z - 0.001f;
        end.z = Player.inst.transform.position.z - 0.001f;

        this.start = start;
        this.end = end;
        
        //this.lineRend.positionCount = 2;
        //this.lineRend.SetPositions(new Vector3[] { start, end });
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
    /// Set the z-position of the line renderer.
    /// </summary>
    public void SetZPos(float zPos)
    {
        Vector3 lineRendPos1 = this.start;
        Vector3 lineRendPos2 = this.end;
        lineRendPos1.z = zPos;
        lineRendPos2.z = zPos;
        this.lineRend.SetPosition(0, lineRendPos1);
        this.lineRend.SetPosition(1, lineRendPos2);
    }

    /// <summary>
    /// Set data and set material color.
    /// </summary>
    public void SetContent(Type fieldType, Chord chord, Color color, bool available, bool isBuildingUp)
    {
        this.type = fieldType;
        this.chord = chord;
        this.color = color;
        this.available = available;
        this.isBuildingUp = isBuildingUp;

        this.lineRend.material.color = color;
    }

    public static bool IsCorner(int ID)
    {
        if ((ID + 1) % VisualController.inst.fieldsPerEdge == 0 || ID % VisualController.inst.fieldsPerEdge == 0)
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
        int testID = ID + (VisualController.inst.fieldsPerEdge / 2 + 1);

        if (testID % VisualController.inst.fieldsPerEdge == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsCorner_RightPart(int ID)
    {
        if (ID % VisualController.inst.fieldsPerEdge == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static Vector3 CornerPosition(int ID)
    {
        if (!IsCorner(ID)) 
        {
            Debug.Log("Error! Wrong ID, no corner.");
            return Vector3.zero;
        }
        else if(IsCorner_RightPart(ID))
        {
            return TunnelData.fields[ID].start;
        }
        else
            return TunnelData.fields[ID].end;
    }

    public static Vector3 NextEdgePartMid(int curID, int direction)
    {
        // = take the mid of corners

        int nextID = (curID + direction).Modulo(VisualController.inst.FieldsCount);
        Vector3 position;

        if (!IsCorner(nextID))
        {
            position = (TunnelData.fields[nextID].start + TunnelData.fields[nextID].end) / 2f;
            return position;
        }
        else if (!IsCorner(curID) && IsCorner(nextID))
        {
            position = CornerPosition(nextID);
            return position;
        }
        else if (IsCorner(curID) && IsCorner(nextID))
        {
            nextID = (nextID + (int) Mathf.Sign(direction)).Modulo(VisualController.inst.FieldsCount);
            position = (TunnelData.fields[nextID].start + TunnelData.fields[nextID].end) / 2f;
            return position;
        }
        else
        {
            Debug.LogError("wrong values");
            return Vector3.zero;
        }

    }
}



// -------------------------- Player Edge Part -------------------------



public class PlayerField : MusicField
{
    public Vector3[] positions;
    

    // Contructor
    public PlayerField(Type type, LineRenderer lineRend)
    {
        this.type = type;
        base.lineRend = lineRend;
        SetToFocus();
    }

    public new enum Type {Main, Second};
    public new Type type;
    public bool changed;


    public void Set(int ID, Vector3[] positions, bool isCorner)
    {
        this.ID = ID;
        this.positions = positions;
        this.isCorner = isCorner;

        for (int i=0; i<positions.Length; i++)
            this.positions[i].z = Player.inst.transform.position.z - 0.002f;
    }

    public void SetVisible(bool value)
    {
        this.lineRend.enabled = value;
    }

    

    public void SetToFocus()
    {
        this.lineRend.startWidth = VisualController.inst.edgePartThickness;
        this.lineRend.endWidth = VisualController.inst.edgePartThickness;
        SetOpacity(0.5f);
    }

    public void SetToPlay()
    {
        this.lineRend.startWidth = VisualController.inst.playerEdgePartThickness;
        this.lineRend.endWidth = VisualController.inst.playerEdgePartThickness;
        SetOpacity(1);
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

    

    public void UpdateLineRenderer()
    {
        if (!isCorner)
        {
            this.lineRend.positionCount = this.positions.Length;
            this.lineRend.SetPositions(this.positions);
        }
        else
        {
            List<Vector3> addedLineRendPositions = this.positions.ToList();
            Vector3 cornerPos = addedLineRendPositions[1];
            addedLineRendPositions.Insert(1, cornerPos);
            addedLineRendPositions.Insert(1, cornerPos);


            this.lineRend.positionCount = addedLineRendPositions.Count;
            this.lineRend.SetPositions(addedLineRendPositions.ToArray());
        }
        
    }

}


// -------------------------------- Edge -------------------------------


public class Edge
{
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


    public static Color[] RandomColors(int types)
    {
        // == Get a random color for each type

        Color[] colors = new Color[types];
        for (int i=0; i < types; i++)
        {
            Color randColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            colors[i] = randColor;
        }
        return colors;
    }


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
        var edgePartIDs = ExtensionMethods.IntToList(VisualController.inst.FieldsCount, true);
        var fieldsPerEdge = VisualController.inst.fieldsPerEdge;
        var fieldsCount = VisualController.inst.FieldsCount;

        // 1. Gehe jeden chordType durch (3)
        for (int i = 0; i < chords.Length; i++)
        {
            var curChords = chords[i].ToList();

            // 2. Gehe jeden chord durch (3-5)
            for (int j = 0; j < chords[i].Length; j++)
            {

                // chord
                var chord = curChords[0];
                curChords.RemoveAt(0);

                if (i == 0)
                {
                    // get CORNER fields
                    int ID1 = ExtensionMethods.Modulo(j * fieldsPerEdge - 1, fieldsCount);
                    int ID2 = j * fieldsPerEdge;
                    edgePartIDs.Remove(ID1);
                    edgePartIDs.Remove(ID2);

                    // assign
                    fieldsToAssign[ID1].SetContent(fieldTypes[ID1], chord, colors[i], availables[ID1], buildUps[ID1]);
                    fieldsToAssign[ID2].SetContent(fieldTypes[ID2], chord, colors[i], availables[ID2], buildUps[ID2]);

                }
                else
                {
                    // get random other field
                    int randID_index = Random.Range(0, edgePartIDs.Count);
                    int randID = edgePartIDs[randID_index];

                    edgePartIDs.Remove(randID);

                    // assign
                    //Debug.Log("chord type: " + i + ", chord: " + j + ", randID: " + randID);
                    //Debug.Log("field type: " + fieldTypes.Length + ", colors.length: " + colors.Length + ", availables.lenght: " + availables.Length + ", buildups.length: " + buildUps.Length);
                    fieldsToAssign[randID].SetContent(fieldTypes[randID], chord, colors[i], availables[randID], buildUps[randID]);
                }
            }
        }

        return fieldsToAssign;
    }

    public static void SwitchEdgeParts()
    {

    }



}
