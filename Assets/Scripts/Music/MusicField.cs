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
    public Type type; //
    public Vector3 start;
    public Vector3 end;
    public Vector3 mid;
    public Vector3[] positions;
    public float height;
    public bool isCorner;
    public bool isEdgeMid;
    public LineRenderer lineRend;
    public Chord chord; //
    public bool isSelectable;
    public bool isNotSpawning;
    public MeshRenderer fieldSurface;
    public MeshRenderer highlightSurface;
    public Color fieldSurfaceColor;
    public Color highlightSurfaceColor;
    protected float surfaceOpacity;

    private int lastActiveRecords;
    private int activeRecords;
    public int ActiveRecords
    {
        get { return activeRecords; }
        set
        {
            lastActiveRecords = activeRecords;

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


    // Contructors
    public MusicField()
    {
        
    }

    public MusicField(LineRenderer lineRend, int ID)
    {
        this.lineRend = lineRend;
        this.ID = ID;
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
        this.isNotSpawning = !isBuildingUp;
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
        this.fieldSurface.material.SetColor("_BaseColor", fieldSurfaceColor);
        this.highlightSurface.material.SetColor("_BaseColor", highlightSurfaceColor);
        this.highlightSurface.material.SetColor("_EmissionColor", highlightSurfaceColor * surfaceIntensity);
    }

    public void SetFieldColor(Color color)
    {
        fieldSurface.material.SetColor("_BaseColor", color);
    }


    /// <summary>
    /// Set opacity of the highlight surface meshRenderer. Store value in a variable.
    /// </summary>
    /// <param name="opacity"></param>
    public void SetHighlightOpacity(float opacity)
    {
        surfaceOpacity = opacity;
        var color = highlightSurface.material.color;
        color.a = opacity;
        highlightSurface.material.color = color;
    }

    /// <summary>
    /// Set the opacity of the inner surface and an emissive intensifier.
    /// </summary>
    /// <param name="opacity"></param>
    public void SetFieldVisibility(float opacity, float emissiveIntensifier, string altColor = null)
    {
        Color color = fieldSurfaceColor;

        if (altColor == "red")
        {
            color = MeshRef.inst.recordFieldColor;
        }
        fieldSurface.material.SetFloat("Fill", opacity);
        fieldSurface.material.SetColor("_BaseColor", color * emissiveIntensifier);
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




    /// <summary>
    /// Change highlight surface opacity (+unused stuff: Change line renderer width and z-position).
    /// </summary>
    public void SetHighlightSurface_toActive()
    {
        // 1. Unimportant lineRend stuff
        this.lineRend.startWidth = VisualController.inst.playerFieldPlayThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldPlayThickness;

        float zPos = Player.inst.transform.position.z - (VisualController.inst.fieldsBeforeSurface + 0.001f);
        SetLineRendZPos(zPos);

        // 2. Set highlight surface
        SetHighlightOpacity(VisualController.inst.playOutsideHighlightOpacity);
    }


    /// <summary>
    /// Change line renderer width, outerField-opacity (and z-position of line rend).
    /// </summary>
    public void SetHighlightSurface_toFocus()
    {
        // 1. Unimportant lineRend stuff
        this.lineRend.startWidth = VisualController.inst.playerFieldFocusThickness;
        this.lineRend.endWidth = VisualController.inst.playerFieldFocusThickness;

        float zPos = Player.inst.transform.position.z - VisualController.inst.playerFieldBeforeSurface;
        SetLineRendZPos(zPos);

        // 2. Set opacity
        SetHighlightOpacity(VisualController.inst.focusOutsideHighlightOpacity);
    }



    /// <summary>
    /// Disable old Highlight- and FieldSurface, enable new. Set opacity.
    /// </summary>
    public void UpdatePlayingSurfaces()
    {
        var lastField = Player.inst.lastField;

        var playFieldOpacity = VisualController.inst.playFieldOpacity;
        var playFieldIntensifier = VisualController.inst.playFieldIntensifier;
        var playHighlightOpacity = VisualController.inst.playOutsideHighlightOpacity;
        var focusHighlightOpacity = VisualController.inst.focusOutsideHighlightOpacity;
        var recordOpacity = VisualController.inst.recordFieldOpacity;
        var recordIntensifier = VisualController.inst.recordFieldIntensifier;

        // LAST FIELD
        // 1.1. Highlight surface
        lastField.highlightSurface.enabled = false;
        // 1.2. FieldSurfaces: Keep enabled, set opacity
        if (lastField.activeRecords > 0)
        {
            lastField.SetFieldVisibility(recordOpacity, recordIntensifier);
        }
        else
        {
            lastField.SetFieldVisibility(0, 1);                                                      // opacity bräuchte eig variable
        }

        // CURRENT FIELD
        highlightSurface.enabled = true;

        if (Player.inst.actionState == Player.ActionState.Play)
        {
            SetHighlightOpacity(playHighlightOpacity);

            if (Recorder.inst.isRecording)
            {
                SetFieldVisibility(playFieldOpacity, playFieldIntensifier, "red");
            }
            else
            {
                SetFieldVisibility(playFieldOpacity, playFieldIntensifier);
            }
        }
        else
        {
            SetHighlightOpacity(focusHighlightOpacity);
            if (ActiveRecords == 0)
            {
                SetFieldVisibility(0, 1);                                                                 // opacity bräuchte eig variable
            }
        }

        //var fieldOpacity = ms_play_outside_fieldSurfaceOpacity;
        //var intensifier = innerField_liveColorIntensifier;

    }
}




#region trash
// -------------------------- Player Edge Part -------------------------





//public class PlayerField : MusicField
//{
//public bool IsSelectable { get { return Player.inst.curFieldSet[ID].isSelectable; } }
//public bool IsNotSpawning { get { return !Player.inst.curFieldSet[ID].isNotSpawning; } }
//private MeshRenderer lastHighlightSurface;
//private MeshRenderer lastFieldSurface;



// Contructors
//public PlayerField(LineRenderer lineRend, int ID)
//{
//    this.lineRend = lineRend;
//    this.ID = ID;
//}



// Functions


///// <summary>
///// Assign a surface. Disable MeshRenderer.
///// </summary>
//public void InitSurface()
//{
//    highlightSurface = TunnelData.fields[0].highlightSurface;
//    highlightSurface.enabled = false;
//    lastHighlightSurface = highlightSurface;
//}


///// <summary>
///// Set variables. Set z-position.
///// </summary>
//public void Set(int ID, Vector3[] positions, Vector3 mid, bool isCorner)
//{
//    this.ID = ID;
//    this.positions = positions;
//    this.mid = mid;
//    this.isCorner = isCorner;

//    for (int i=0; i< positions.Length; i++)
//        this.positions[i].z = Player.inst.transform.position.z - VisualController.inst.playerFieldBeforeSurface;
//}

//public void SetVisible(bool value)
//{
//    this.lineRend.enabled = value;
//}







///// <summary>
///// Set z pos only.
///// </summary>
//public void SetZPos(float zPos)
//{
//    for (int i = 0; i < this.lineRend.positionCount; i++)
//    {
//        Vector3 newPos = this.lineRend.GetPosition(i);
//        newPos.z = zPos;
//        this.lineRend.SetPosition(i, newPos);
//    }
//}








/// <summary>
/// LineRend currently disabled.
/// </summary>
//public void RefreshLineRend()
//{
//    var curField = Player.inst.curField;

//    // [currently disabled:] Line renderer positions
//    if (curField.isCorner)
//    {
//        var positions = MeshUpdate.PreventLineRendFromBending(curField.positions);
//        var positionCount = positions.Length;
//        curField.lineRend.positionCount = positionCount;
//        curField.lineRend.SetPositions(positions);
//    }
//    else
//    {
//        curField.lineRend.positionCount = curField.positions.Length;
//        curField.lineRend.SetPositions(curField.positions);
//    }
//}



//}
#endregion


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
