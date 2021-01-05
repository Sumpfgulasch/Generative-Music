using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// ----------------------------- Edge Part ----------------------------

public class EdgePart
{
    public enum Type { Chord, ChordModulation, Pitch};

    // Properties
    public int ID;
    public Type type;
    public Vector3 start;
    public Vector3 end;
    public bool isCorner;
    public bool isEdgeMid;
    public LineRenderer lineRend;
    public Chord chord;

    // Private variables
    private Color color;
    public Color Color
    {
        get { return color; }
        set { color = value; lineRend.material.color = color; }
    }





    
    // Contructors
    public EdgePart()
    {
        
    }

    public EdgePart(Type type)
    {
        this.type = type;
    }

    public EdgePart(int ID, LineRenderer lineRend, bool isCorner, bool isEdgeMid)
    {
        this.ID = ID;
        this.lineRend = lineRend;
        this.isCorner = isCorner;
        this.isEdgeMid = isEdgeMid;
    }









    // Functions

    public void Set(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;

        this.start.z = Player.inst.transform.position.z - 0.001f;
        this.end.z = Player.inst.transform.position.z - 0.001f;

        //lineRend.SetPosition(0, this.start);
        //lineRend.SetPosition(1, this.end);
    }



    public void UpdateLineRenderer(Vector3 start, Vector3 end)
    {
        this.lineRend.positionCount = 2;
        this.lineRend.SetPositions(new Vector3[] { start, end });
    }



    public void Set(int ID, Vector3 start, Vector3 end, bool isCorner)
    {
        this.ID = ID;
        this.start = start;
        this.end = end;
        this.isCorner = isCorner;

        this.start.z = Player.inst.transform.position.z - 0.001f;
        this.end.z = Player.inst.transform.position.z - 0.001f;

        //lineRend.SetPosition(0, this.start);
        //lineRend.SetPosition(1, this.end);
    }

    public static bool IsCorner(int ID)
    {
        if ((ID + 1) % VisualController.inst.envGridLoops == 0 || ID % VisualController.inst.envGridLoops == 0)
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
        int testID = ID + (VisualController.inst.envGridLoops / 2 + 1);

        if (testID % VisualController.inst.envGridLoops == 0)
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
        if (ID % VisualController.inst.envGridLoops == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    //public static int[][] GetRandomSubdivision(ChordData[] chordTypes)
    //{
    //    int[][] subdivisions = new int[subdivisionCount][];

    //    var indicies = ExtensionMethods.IntToList(VisualController.inst.EdgePartCount);

    //    // 1. types
    //    for (int i=0; i< chordTypes.Length; i++)
    //    {
    //        for (int h=0; h<chordTypes[i].individualCount; h++)
    //        {
    //            int randNumber = Random.Range(0, indicies.Count);
    //        }

    //        if (i==0)
    //        {
    //            subdivisions[i] = new int[VisualController.inst.envVertices * 2];

    //            // 2. field-indicies
    //            for (int j=0; j<VisualController.inst.envVertices; j++)
    //            {
    //                int ID1 = ExtensionMethods.NegativeModulo(VisualController.inst.envGridLoops * j - 1, VisualController.inst.EdgePartCount);
    //                int ID2 = VisualController.inst.envGridLoops * j;

    //                subdivisions[i][j * 2] = ID1;
    //                subdivisions[i][j * 2 + 1] = ID2;
    //            }
    //        }

    //    }
    //}
}



// -------------------------- Player Edge Part -------------------------



public class PlayerEdgePart : EdgePart
{
    public Vector3[] positions;

    // Contructor
    public PlayerEdgePart(Type type, LineRenderer lineRend)
    {
        this.type = type;
        base.lineRend = lineRend;
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

        //lineRend.SetPosition(0, base.start);
        //lineRend.SetPosition(1, base.end);
    }

    public void SetVisible(bool value)
    {
        this.lineRend.enabled = value;
    }

    public void UpdateLineRenderer()
    {
        this.lineRend.positionCount = this.positions.Length;
        this.lineRend.SetPositions(this.positions);
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



// -------------------------------- EDGE PARTS -------------------------------



public static class EdgeParts
{
    public static Color[] RandomColors(int types)
    {
        Color[] colors = new Color[types];
        for (int i=0; i < types; i++)
        {
            Color randColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            colors[i] = randColor;
        }
        return colors;
    }



    public static void SetFields(Chord[][] chords, Color[] colors)
    {
        
        // = assign every calculated chord to the fields; assign color

        var edgePartIDs = ExtensionMethods.IntToList(VisualController.inst.EdgePartCount, true);

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
                    // get corner fields
                    int ID1 = ExtensionMethods.Modulo(j * VisualController.inst.envGridLoops - 1, VisualController.inst.EdgePartCount);
                    int ID2 = j * VisualController.inst.envGridLoops;
                    //Debug.Log("chordType i: " + i + ", chord j: " + j + "(chords.length: " + chords[i].Length + ")");
                    edgePartIDs.Remove(ID1);
                    edgePartIDs.Remove(ID2);
                    // set field
                    EnvironmentData.edgeParts[ID1].chord = chord;
                    EnvironmentData.edgeParts[ID2].chord = chord;
                    EnvironmentData.edgeParts[ID1].Color = colors[i];
                    EnvironmentData.edgeParts[ID2].Color = colors[i];
                }
                else
                {
                    // get random field
                    int randID_index = Random.Range(0, edgePartIDs.Count);
                    int randID = edgePartIDs[randID_index];

                    //Debug.Log("chord: " + j + "/" + chords[i].Length + ", edgePartIDs.length: " + edgePartIDs.Count + ", randID: " + randID + ", randID_index: " + randID_index);
                    //ExtensionMethods.PrintArray("Remaining IDs: ", edgePartIDs);
                    edgePartIDs.Remove(randID);

                    // set field
                    EnvironmentData.edgeParts[randID].chord = chord;
                    EnvironmentData.edgeParts[randID].Color = colors[i];
                }
            }
        }
    }

    
}
