using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        //this.type = type;
    }









    // Functions

    public void Set(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;

        this.start.z = Player.inst.transform.position.z - 0.001f;
        this.end.z = Player.inst.transform.position.z - 0.001f;

        lineRend.SetPosition(0, this.start);
        lineRend.SetPosition(1, this.end);
    }

    public void Set(int ID, Vector3 start, Vector3 end, bool isCorner)
    {
        this.ID = ID;
        this.start = start;
        this.end = end;
        this.isCorner = isCorner;

        this.start.z = Player.inst.transform.position.z - 0.001f;
        this.end.z = Player.inst.transform.position.z - 0.001f;

        lineRend.SetPosition(0, this.start);
        lineRend.SetPosition(1, this.end);
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


    public static int[][] GetRandomSubdivision(ChordData[] chordTypes)
    {
        int[][] subdivisions = new int[subdivisionCount][];

        var indicies = ExtensionMethods.IntToList(VisualController.inst.EdgePartCount);

        // 1. types
        for (int i=0; i< chordTypes.Length; i++)
        {
            for (int h=0; h<chordTypes[i].individualCount; h++)
            {
                int randNumber = Random.Range(0, indicies.Count);
            }

            if (i==0)
            {
                subdivisions[i] = new int[VisualController.inst.envVertices * 2];

                // 2. field-indicies
                for (int j=0; j<VisualController.inst.envVertices; j++)
                {
                    int ID1 = ExtensionMethods.NegativeModulo(VisualController.inst.envGridLoops * j - 1, VisualController.inst.EdgePartCount);
                    int ID2 = VisualController.inst.envGridLoops * j;

                    subdivisions[i][j * 2] = ID1;
                    subdivisions[i][j * 2 + 1] = ID2;
                }
            }

        }
    }
}



// -------------------------- Player Edge Part -------------------------



public class PlayerEdgePart : EdgePart
{
    // Contructor
    public PlayerEdgePart(Type type, LineRenderer lineRend)
    {
        this.type = type;
        base.lineRend = lineRend;
    }

    public new enum Type {Main, Second};
    public new Type type;
    public bool changed;


    public new void Set(Vector3 start, Vector3 end)
    {
        base.start = start;
        base.end = end;

        base.start.z = Player.inst.transform.position.z - 0.002f;
        base.end.z = Player.inst.transform.position.z - 0.002f;

        lineRend.SetPosition(0, base.start);
        lineRend.SetPosition(1, base.end);
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
