using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// ----------------------------- MusicField ----------------------------

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
    public bool isSelectable;
    public bool isSpawning;
    public MeshRenderer fieldSurface;
    public MeshRenderer highlightSurface;
    public float SurfaceOpacity { get; protected set; }
    public float height;

    private int lastActiveRecords;
    private int activeRecords;
    public int ActiveRecords
    {
        get { return activeRecords; }
        set
        {
            lastActiveRecords = activeRecords;

            //activeRecords = Mathf.Clamp(value, 0, 100);
            activeRecords = value;
            if (activeRecords == 0)
            {
                GameEvents.inst.onStopFieldByRecord?.Invoke(this);
            }
            else if (lastActiveRecords == 0 && activeRecords > 0)
            {
                GameEvents.inst.onPlayFieldByRecord?.Invoke(this);
            }
            if (activeRecords < 0)
            {
                Debug.LogError("active records wrong value");
            }
            
        }
    }

    // Private variables
    public Color fieldSurfaceColor;
    public Color highlightSurfaceColor;



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

    // Update
    //private void Update()
    //{
        
    //}




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
    /// Set the z-positions of the line renderer.
    /// </summary>
    public void SetLineRendZPos(float zPos)
    {
        for (int i=0; i<lineRend.positionCount; i++)
        {
            var newPos = lineRend.GetPosition(i);
            newPos.z = zPos;
            lineRend.SetPosition(i, newPos);
        }
    }

    /// <summary>
    /// Set data (chord, fieldType, ...).
    /// </summary>
    public void SetData(Type fieldType, Chord chord, bool selectable, bool isBuildingUp)
    {
        this.type = fieldType;
        this.chord = chord;
        this.isSelectable = selectable;
        this.isSpawning = isBuildingUp;
    }

    /// <summary>
    /// Calc different colors from the given color (differing in opacity and value). Set material colors (baseColor and emissiveColor for lineRend, highlightSurface and fieldSurface).
    /// </summary>
    public void SetColors(Color color)
    {
        // 1. Calc colors
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        // 1.1. fieldSurface
        v = VisualController.inst.fieldSurfaceValue;
        Color fieldSurfaceColor = Color.HSVToRGB(h,s,v);
        fieldSurfaceColor.a = VisualController.inst.fieldSurfaceAlpha; // fieldSurface, alpha
        this.fieldSurfaceColor = fieldSurfaceColor;
        
        // 1.2. highlightSurface
        Color.RGBToHSV(color, out h, out s, out v);
        s = VisualController.inst.highlightSurface_emissiveSaturation; // highlightSurface, saturation
        Color highlightSurfaceColor = Color.HSVToRGB(h, s, v);
        this.highlightSurfaceColor = highlightSurfaceColor;

        // Calc intensities
        float lineRendIntensity;
        if (isCorner)
            lineRendIntensity = VisualController.inst.lineRendCornerIntensity;
        else
            lineRendIntensity = VisualController.inst.lineRendNoCornerIntensity;
        float surfaceIntensity = VisualController.inst.highlightSurface_emisiveIntensity;

        // Assign!
        this.lineRend.material.SetColor("_BaseColor", color);
        this.lineRend.material.SetColor("_EmissionColor", color * lineRendIntensity);
        this.fieldSurface.material.SetColor("_BaseColor", fieldSurfaceColor); // no emissive color // "_colorA"    _BaseColor
        //this.fieldSurface.material.SetColor("_colorB", fieldSurfaceColor * 0.3f);
        this.highlightSurface.material.SetColor("_BaseColor", highlightSurfaceColor);
        this.highlightSurface.material.SetColor("_EmissionColor", highlightSurfaceColor * surfaceIntensity);
    }


    public void SetHighlightOpacity(float opacity)
    {
        SurfaceOpacity = opacity;
        var color = highlightSurface.material.color;
        color.a = opacity;
        highlightSurface.material.color = color;
    }

    /// <summary>
    /// Set the opacity of the inner surface and an emissive intensifier.
    /// </summary>
    /// <param name="opacity"></param>
    public void SetInnerFieldVisibility(float opacity, float emissiveIntensifier)
    {
        fieldSurface.material.SetFloat("Fill", opacity);
        fieldSurface.material.SetColor("GridColor", fieldSurfaceColor * emissiveIntensifier);
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





public class PlayerField // : MusicField
{
    public int ID;
    
    #region line renderer zeug
    public LineRenderer lineRend;
    public Vector3 start;
    public Vector3 end;
    public Vector3 mid;
    public Vector3[] positions;
    public bool isCorner;
    public bool isEdgeMid;      
    public Vector3[] secondaryPositions; // Alles nur Zeug für line renderer
    #endregion

    public bool IsSelectable { get { return Player.inst.curFieldSet[ID].isSelectable; } }
    public bool IsNotSpawning { get { return !Player.inst.curFieldSet[ID].isSpawning; } }
    public MeshRenderer OuterSurface { get; private set; }
    public float SurfaceOpacity { get; private set; }

    

    // Contructors
    public PlayerField(LineRenderer lineRend, int ID)
    {
        this.lineRend = lineRend;
        this.ID = ID;
    }



    // Functions


    /// <summary>
    /// Assign a surface. Disable MeshRenderer.
    /// </summary>
    public void InitSurface()
    {
        OuterSurface = TunnelData.fields[0].highlightSurface;
        OuterSurface.enabled = false;
    }


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
    public void SetHighlightSurface_toFocus()
    {
        this.lineRend.startWidth = VisualController.inst.playerFieldFocusThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldFocusThickness;

        float zPos = Player.inst.transform.position.z - VisualController.inst.playerFieldBeforeSurface;
        SetZPos(zPos);

        SetHighlightOpacity(VisualController.inst.ms_focus_outside_fieldSurfaceOpacity); // to do: besser; fragen ob ...
    }

    /// <summary>
    /// Change highlight surface opacity (+unused stuff: Change line renderer width and z-position).
    /// </summary>
    public void SetHighlightSurfaceActive()
    {
        // 1. Unimportant lineRend stuff
        this.lineRend.startWidth = VisualController.inst.playerFieldPlayThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldPlayThickness;

        float zPos = Player.inst.transform.position.z - (VisualController.inst.fieldsBeforeSurface + 0.001f);
        SetZPos(zPos);

        // 2. Set highlight surface
        SetHighlightOpacity(VisualController.inst.ms_play_outside_fieldSurfaceOpacity);
    }


    /// <summary>
    /// Set z pos only.
    /// </summary>
    public void SetZPos(float zPos)
    {
        for (int i = 0; i < this.lineRend.positionCount; i++)
        {
            Vector3 newPos = this.lineRend.GetPosition(i);
            newPos.z = zPos;
            this.lineRend.SetPosition(i, newPos);
        }
    }


    /// <summary>
    /// Set opacity of the highlight surface.
    /// </summary>
    /// <param name="opacity">Opacity. 0 == transparent [0, 1].</param>
    public void SetHighlightOpacity(float opacity)
    {
        SurfaceOpacity = opacity;
        Color newColor = OuterSurface.material.color;
        newColor.a = opacity;
        OuterSurface.material.color = newColor;
    }

    /// <summary>
    /// Disable old HighlightSurface, enable new. Set opacity to current value.
    /// </summary>
    public void RefreshHighlightSurface()
    {
        OuterSurface.enabled = false;
        OuterSurface = Player.inst.curFieldSet[ID].highlightSurface;
        OuterSurface.enabled = true;

        SetHighlightOpacity(SurfaceOpacity);
    }

    /// <summary>
    /// LineRend currently disabled.
    /// </summary>
    public void RefreshLineRend()
    {
        var curField = Player.inst.curField;

        // [currently disabled:] Line renderer positions
        if (curField.isCorner)
        {
            var positions = MeshUpdate.PreventLineRendFromBending(curField.positions);
            var positionCount = positions.Length;
            curField.lineRend.positionCount = positionCount;
            curField.lineRend.SetPositions(positions);
        }
        else
        {
            curField.lineRend.positionCount = curField.positions.Length;
            curField.lineRend.SetPositions(curField.positions);
        }
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
