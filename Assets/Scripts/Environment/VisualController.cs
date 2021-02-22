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
    public bool showCursor = true;
    public bool showMilkSurface = false;
    [Range(0.1f, 1)]
    public float mouseColliderSize_play = 1;
    [Header("Fields line renderer")]
    public bool showPlayerLinerend = false;
    [Range(0.001f, 0.05f)]
    public float fieldThickness = 0.01f;
    [Range(0.001f, 0.05f)]
    public float playerFieldPlayThickness = 0.03f;
    [Range(0.001f, 0.05f)]
    public float playerFieldFocusThickness = 0.02f;
    [Range(0.001f, 0.05f)]
    public float playerSecFieldThickness = 0.01f;
    public float playerFieldBeforeSurface = 0.002f;
    public float fieldsBeforeSurface = 0.01f;
    [Header("Fields surfaces")]
    [Range(0f, 1f)]
    public float ms_focus_inside_fieldSurfaceOpacity = 0;
    [Range(0f, 1f)]
    public float ms_focus_outside_fieldSurfaceOpacity = 0.058f;
    [Range(0f, 1f)]
    public float ms_play_inside_fieldSurfaceOpacity = 1;
    [Range(0f, 1f)]
    public float ms_play_outside_fieldSurfaceOpacity = 1;
    [Range(0, 0.5f)] public float minFieldSurfaceHeight = 0.1f;
    [Range(0, 2.0f)] public float maxFieldSurfaceHeight = 0.4f;
    [Range(0.1f, 1)] public float fieldSurfaceAlpha = 0.2f;

    [Header("Colors")]
    public int colorCount = 4;
    [Space]
    [Range(0, 1f)] public float lineRendHue_CornerStep = 1 / 40f;
    [Range(0, 1f)] public float lineRendHue_NoCornerStep = 1/12f;
    [Range(0, 1f)] public float lineRendHue_Corner2NoCornerDistance = 0;
    [Range(0, 1f)] public float fieldsSaturation = 0.8f;
    [Range(0, 1f)] public float fieldsValue = 0.8f;
    [Range(0, 20f)] public float surfaceIntensity = 5f;
    [Range(0, 20f)] public float lineRendCornerIntensity = 5f;
    [Range(0, 20f)] public float lineRendNoCornerIntensity = 3.2f;

    [Header("Other")]
    public GUIStyle curChordTextStyle;
    public GUIStyle lastChordTextStyle;

    private Vector3 playerMid;

    private Player Player { get { return Player.inst; } }

    

    void Start()
    {
        inst = this;
        if (!showCursor)
            Cursor.visible = false;

        playerMid = Player.transform.position;

        // Event subscription
        GameEvents.inst.onFieldChange += OnFieldChange;
        GameEvents.inst.onMouseInside += OnMouseInside;
        GameEvents.inst.onMouseOutside += OnMouseOutside;
    }



    // ----------------------------- Events ----------------------------


    /// <summary>
    /// Change player visibility
    /// </summary>
    private void OnFieldChange(PlayerField data)
    {
        if (data.IsNotSpawning && data.IsSelectable)
        {
            MeshUpdate.UpdatePlayerFieldVisibility();
        }
    }

    /// <summary>
    /// Change player visibility.
    /// </summary>
    public void OnPlayStart(Player.Side side)
    {
        Player.inst.curField.SetToPlay();
        foreach (PlayerField secField in Player.inst.curSecondaryFields)
            secField.SetVisible(true);
    }


    public void OnPlayEnd()
    {
        Player.inst.curField.SetToFocus();
        foreach (PlayerField secField in Player.inst.curSecondaryFields)
            secField.SetVisible(false);
    }


    public void OnMove(InputAction.CallbackContext context)
    {

    }


    /// <summary>
    /// Change the played outer field.
    /// </summary>
    private void OnMouseInside()
    {
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            Player.inst.curField.SetOpacity(ms_play_inside_fieldSurfaceOpacity);
        }
        else
        {
            Player.inst.curField.SetOpacity(ms_focus_inside_fieldSurfaceOpacity);
        }
    }

    private void OnMouseOutside()
    {
        if (Player.inst.actionState == Player.ActionState.Play)
        {
            Player.inst.curField.SetOpacity(ms_play_outside_fieldSurfaceOpacity);
        }
        else
        {
            Player.inst.curField.SetOpacity(ms_focus_outside_fieldSurfaceOpacity);
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
