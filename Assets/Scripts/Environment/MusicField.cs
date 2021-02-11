using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// ----------------------------- Edge Part ----------------------------

public class MusicField
{
    public enum Type { Chord, Modulation, Pitch };

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
    public MeshRenderer outerSurface;
    public float SurfaceOpacity { get; protected set; }

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
    public void SetVertices(Vector3 start, Vector3 mid, Vector3 end, Vector3[] positions)
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
    /// Set data and set material colors.
    /// </summary>
    public void SetContent(Type fieldType, Chord chord, Color color, bool selectable, bool isBuildingUp)
    {
        this.type = fieldType;
        this.chord = chord;
        this.color = color;
        this.selectable = selectable;
        this.isBuildingUp = isBuildingUp;

        float intensity = VisualController.inst.outerSurfaceIntensity;

        this.lineRend.material.SetColor("_BaseColor", color);
        this.lineRend.material.SetColor("_EmissionColor", color * intensity);
        this.outerSurface.material.SetColor("_BaseColor", color);
        this.outerSurface.material.SetColor("_EmissionColor", color * intensity);
    }

    public static bool IsCorner(int ID)
    {
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

    


    /// <summary>
    /// Get the adjacent field ID to a given ID.
    /// </summary>
    public static int NextFieldID(int curID, int direction)
    {
        int nextID = ExtensionMethods.Modulo(curID + direction, TunnelData.FieldsCount);

        return nextID;
    }
}



// -------------------------- Player Edge Part -------------------------



public class PlayerField : MusicField
{
    //public new enum Type { Main, Second };
    //public new Type type;
    public Vector3[] secondaryPositions;

    // Contructors
    public PlayerField(LineRenderer lineRend, int ID)
    {
        //this.type = type;
        base.lineRend = lineRend;
        this.ID = ID;
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
    /// Change line renderer width, outerField-opacity (and z-position of line rend).
    /// </summary>
    public void SetToFocus()
    {
        this.lineRend.startWidth = VisualController.inst.playerFieldFocusThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldFocusThickness;

        float zPos = Player.inst.transform.position.z - VisualController.inst.playerFieldBeforeSurface;
        SetZPos(zPos);

        SetOpacity(VisualController.inst.ms_focus_outside_fieldSurfaceOpacity); // to do: besser; fragen ob ...
    }

    /// <summary>
    /// Change line renderer width, opacity and z-position.
    /// </summary>
    public void SetToPlay()
    {
        this.lineRend.startWidth = VisualController.inst.playerFieldPlayThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldPlayThickness;

        float zPos = Player.inst.transform.position.z - (VisualController.inst.fieldsBeforeSurface + 0.001f);
        SetZPos(zPos);

        SetOpacity(VisualController.inst.ms_play_outside_fieldSurfaceOpacity);
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
    /// Set opacity of the outer surface (!) - not the line renderer.
    /// </summary>
    /// <param name="opacity">Opacity. 0 == transparent [0, 1].</param>
    public void SetOpacity(float opacity)
    {
        SurfaceOpacity = opacity;
        Color newColor = outerSurface.material.color;
        newColor.a = opacity;
        outerSurface.material.color = newColor;
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
