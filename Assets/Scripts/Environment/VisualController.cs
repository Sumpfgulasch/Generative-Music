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
    [HideInInspector] public Color[] tracksColorPalette;

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
    [Range(0.001f, 1)] public float recordFieldOpacity = 0.005f;
    [Range(0.01f, 30)] public float recordFieldIntensifier = 22;
    [Range(0.001f, 1)] public float playFieldOpacity = 0.177f;
    [Range(0.01f, 30)] public float playFieldIntensifier = 30f;
    [Space]
    [Range(0, 0.5f)] public float minFieldSurfaceHeight = 0.1f;
    [Range(0, 2.0f)] public float maxFieldSurfaceHeight = 0.4f;
    [Range(0.1f, 1)] public float fieldSurfaceAlpha = 0.9f;
    [Range(0.1f, 1)] public float fieldSurfaceValue = 0.6f;



    [Header("Highlight surfaces")]
    [Range(0f, 1f)] public float focusInsideHighlightOpacity = 0.02f;
    [Range(0f, 1f)] public float focusOutsideHighlightOpacity = 0.04f;
    [Range(0f, 1f)] public float playInsideHighlightOpacity = 1;
    [Range(0f, 1f)] public float playOutsideHighlightOpacity = 1;
    [Space]
    [Range(0, 20f)] public float highlightSurface_emisiveIntensity = 3.94f;
    [Range(0, 1f)] public float highlightSurface_emissiveSaturation = 0.8f;

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

        tracksColorPalette = new Color[5];

        // Unschön
        for (int i=0; i<tracksColorPalette.Length; i++)
        {
            tracksColorPalette[i] = MeshRef.inst.layerButtons[i].colors.normalColor;
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
        bool isPlayingLive = Player.inst.actionState == Player.ActionState.Play;
        if (!(isPlayingLive && Player.inst.curField.ID == field.ID))
        {
            var opacity = recordFieldOpacity;
            var intensifier = recordFieldIntensifier;

            field.SetFieldVisibility(opacity, intensifier);
        }
    }



    private void OnStopField_byRecord(MusicField field)
    {
        bool isPlayingLive = Player.inst.actionState == Player.ActionState.Play;
        if (!(isPlayingLive && Player.inst.curField.ID == field.ID))
        {
            field.SetFieldVisibility(0, 1);
        }
    }




    /// <summary>
    /// Set FieldSurface and HighlightSurface.
    /// </summary>
    public void OnPlayField_byInput()
    {
        var curField = Player.inst.curFieldSet[Player.inst.curField.ID];
        var curPlayerField = Player.inst.curField;

        // 1. HighlightSurface
        curPlayerField.SetHighlightOpacity(playOutsideHighlightOpacity);

        // 2. FieldSurface
        if (Recorder.inst.isRecording)
        {
            curField.SetFieldVisibility(playFieldOpacity, playFieldIntensifier, "red");
        }
        else
        {
            curField.SetFieldVisibility(playFieldOpacity, playFieldIntensifier);
        }
        
    }



    /// <summary>
    /// Set FieldSurface and HighlightSurface.
    /// </summary>
    public void OnStopfield_byInput()
    {
        var curField = Player.inst.curFieldSet[Player.inst.curField.ID];

        var curPlayerField = Player.inst.curField;
        var opacity = focusOutsideHighlightOpacity;



        // 1. HighlightSurface
        curPlayerField.SetHighlightOpacity(opacity);

        // 2. FieldSurface
        if (curField.ActiveRecords > 0)
        {
            var fieldOpacity = recordFieldOpacity;
            var intensifier = recordFieldIntensifier;

            curPlayerField.SetFieldVisibility(fieldOpacity, intensifier);
        }
        else
        {
            curPlayerField.SetFieldVisibility(0, 1);
        }
            


    }


    /// <summary>
    /// Change player visibility
    /// </summary>
    public void OnFieldChange(MusicField curField)
    {
        if (curField.isNotSpawning && curField.isSelectable)
        {
            curField.UpdatePlayingSurfaces();
            
        }
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
            Player.inst.curField.SetHighlightOpacity(playInsideHighlightOpacity);
        }
        else
        {
            Player.inst.curField.SetHighlightOpacity(focusInsideHighlightOpacity);
        }
    }

    public void OnMouseOutside()
    {
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            Player.inst.curField.SetHighlightOpacity(playOutsideHighlightOpacity);
        }
        else
        {
            Player.inst.curField.SetHighlightOpacity(focusOutsideHighlightOpacity);
        }
    }


    public void OnRecord(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Player.inst.actionState == Player.ActionState.Play)
            {
                if (Recorder.inst.isRecording)
                {
                    Player.inst.curField.SetFieldVisibility(playFieldOpacity, playFieldIntensifier, "red");
                }
                else
                {
                    Player.inst.curField.SetFieldVisibility(playFieldOpacity, playFieldIntensifier);
                }
                
            }
        }
    }



    private void OnGUI()
    {
        if (UIManager.inst.debugChords)
        {
            GUI.Label(new Rect(50, 50, 200, 70),
            "Chord: " + MusicManager.inst.curChord.notes.NoteNames(),
            curChordTextStyle);

            GUI.Label(new Rect(50, 90, 200, 70),
                "Chord: " + MusicManager.inst.lastChord.notes.NoteNames(),
                lastChordTextStyle);
        }

    }



}
