using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VisualController : MonoBehaviour
{
    // public
    public static VisualController inst;

    [Header("General settings")]
    public int fieldsPerEdge = 6;
    public int tunnelVertices = 3;
    public int colorCount = 4;
    public bool showCursor = true;
    public bool showMilkSurface = false;
    public int shapesPerBar = 4;
    [Range(0.1f, 1)]
    public float mouseColliderSize_play = 1;
    public Vector2 cameraOffset;
    public Color[] colorPalette;

    [Header("Player")]
    [Range(0, 1f)] public float playerAlpha = 0.5f;

    [Header("fields lineRend & surface")]
    [Range(0, 1f)] public float fieldsSaturation = 0.8f;
    [Range(0, 1f)] public float fieldsValue = 0.9f;
    [Range(0, 1f)] public float fieldsHue_CornerStep = 0;
    [Range(0, 1f)] public float fieldsHue_NoCornerStep = 0.043f;
    [Range(0, 1f)] public float fieldsHue_Corner2NoCornerDistance = 0;
    [Range(0, 1f)] public float fieldFoldOutTime = 0.3f;
    public AnimationCurve fieldFoldOutCurve;

    [Header("Fields line renderer")]
    public bool showPlayerLinerend = false;
    [Range(0.001f, 0.05f)]
    public float fieldThickness = 0.01f;
    public float fieldsBeforeSurface = 0.01f;
    [Space]

    [Range(0, 20f)] public float lineRendCornerIntensity = 5.6f;
    [Range(0, 20f)] public float lineRendNoCornerIntensity = 2.2f;

    [Header("Field surfaces")]
    [Range(0, 0.5f)] public float minFieldSurfaceHeight = 0.1f;
    [Range(0, 2.0f)] public float maxFieldSurfaceHeight = 0.4f;
    [Range(0.1f, 1)] public float fieldSurfaceAlpha = 1;
    [Range(0.1f, 1)] public float fieldSurfaceValue = 0.5f;
    [Space]
    [Range(0.001f, 1)] public float innerFieldSurface_recordPlayOpacity = 0.001f;
    [Range(0.01f, 30)] public float innerField_recordColorIntensifier = 6.8f;
    [Range(0.001f, 1)] public float innerFieldSurface_livePlayOpacity = 0.5f;
    [Range(0.01f, 30)] public float innerField_liveColorIntensifier = 10f;

    [Header("Highlight surfaces")]
    [Range(0f, 1f)] public float ms_focus_inside_fieldSurfaceOpacity = 0;
    [Range(0f, 1f)] public float ms_focus_outside_fieldSurfaceOpacity = 0.058f;
    [Range(0f, 1f)] public float ms_play_inside_fieldSurfaceOpacity = 1;
    [Range(0f, 1f)] public float ms_play_outside_fieldSurfaceOpacity = 1;
    [Range(0, 20f)] public float highlightSurface_emisiveIntensity = 5f;
    [Range(0, 1f)] public float highlightSurface_emissiveSaturation = 0.9f;

    [Header("Record objects")]
    public float chordObjectsOpacity = 1;

    [Header("Beat triangle")]
    public float highlightBeatTime = 0.2f;
    public Color highlightBeatColor = Color.white;
    public float highlightBeatIntensity = 5f;
    public AnimationCurve highlightBeatCurve;

    [Header("Recording")]
    public Color recordColor;
    public Color nonRecordColor;
    
    [Header("Other")]
    public GUIStyle curChordTextStyle;
    public GUIStyle lastChordTextStyle;

    [Header("Unused")]
    [Range(0.001f, 0.05f)] public float playerFieldPlayThickness = 0.03f;
    [Range(0.001f, 0.05f)] public float playerFieldFocusThickness = 0.02f;
    [Range(0.001f, 0.05f)] public float playerSecFieldThickness = 0.01f;
    public float playerFieldBeforeSurface = 0.002f;



    // Private variables
    private Vector3 playerMid;


    // Properties
    private Player Player { get { return Player.inst; } }

    

    void Start()
    {
        inst = this;
        if (!showCursor)
            Cursor.visible = false;

        playerMid = Player.transform.position;

        colorPalette = new Color[5];
        for (int i=0; i<colorPalette.Length; i++)
        {
            colorPalette[i] = MeshRef.inst.layerButtons[i].colors.normalColor;
        }

        // Events
        GameEvents.inst.onPlayFieldByRecord += OnPlayField_byRecord;
        GameEvents.inst.onStopFieldByRecord += OnStopField_byRecord;
    }



    // ----------------------------- Events ----------------------------


    /// <summary>
    /// Highlight field when played by a loopObject.
    /// </summary>
    /// <param name="field"></param>
    private void OnPlayField_byRecord(MusicField field)
    {
        var opacity = innerFieldSurface_recordPlayOpacity;
        var intensifier = innerField_recordColorIntensifier;

        field.SetInnerFieldVisibility(opacity, intensifier);
    }

    private void OnStopField_byRecord(MusicField field)
    {
        field.SetInnerFieldVisibility(0, 1);
    }


    

    /// <summary>
    /// Change player visibility.
    /// </summary>
    public void OnPlayField_byInput()
    {
        int ID = Player.inst.curField.ID;
        var curField = Player.inst.curFieldSet[ID];

        Player.inst.curField.SetHighlightSurfaceActive();

        //curField.SetInnerFieldVisibility(ms_play_outside_fieldSurfaceOpacity, innerField_liveColorIntensifier);
        //curField.SetHighlightOpacity(ms_play_outside_fieldSurfaceOpacity);
        //curField.ActiveRecords++;
        //print("onPlayField_byInput; ++");
    }



    /// <summary>
    /// Change player visibility
    /// </summary>
    public void OnFieldChange(PlayerField data)
    {
        if (data.IsNotSpawning && data.IsSelectable)
        {
            var curField = Player.inst.curField;
            curField.RefreshHighlightSurface();
        }
    }


    public void OnStopfield_byInput()
    {
        Player.inst.curField.SetHighlightSurface_toFocus();
        //foreach (PlayerField secField in Player.inst.curSecondaryFields)
        //    secField.SetVisible(false);

        int ID = Player.inst.curField.ID;
        var curField = Player.inst.curFieldSet[ID];

        //curField.SetHighlightOpacity(0);
        //curField.ActiveRecords--;
        //print("onSTOPField_byInput; --");
    }

    public void OnBeat(int beat)
    {
        MeshUpdateMono.inst.ShowBeat();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MeshRef.inst.mouseTrail.position = Player.inst.mousePos;
        }
    }


    /// <summary>
    /// Change the played outer field.
    /// </summary>
    public void OnMouseInside()
    {
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            Player.inst.curField.SetHighlightOpacity(ms_play_inside_fieldSurfaceOpacity);
        }
        else
        {
            Player.inst.curField.SetHighlightOpacity(ms_focus_inside_fieldSurfaceOpacity);
        }
    }

    public void OnMouseOutside()
    {
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            Player.inst.curField.SetHighlightOpacity(ms_play_outside_fieldSurfaceOpacity);
        }
        else
        {
            Player.inst.curField.SetHighlightOpacity(ms_focus_outside_fieldSurfaceOpacity);
        }
    }




    private void OnGUI()
    {
        GUI.Label(new Rect(50, 50, 200, 70), 
            "Chord: " + MusicManager.inst.curChord.notes.NoteNames(), 
            curChordTextStyle);

        GUI.Label(new Rect(50, 90, 200, 70),
            "Chord: " + MusicManager.inst.lastChord.notes.NoteNames(),
            lastChordTextStyle);

        


    }



}
