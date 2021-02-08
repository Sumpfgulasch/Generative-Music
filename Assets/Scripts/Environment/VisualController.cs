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


    void Update()
    {
        
    }



    // ----------------------------- Events ----------------------------


    /// <summary>
    /// Change player visibility
    /// </summary>
    private void OnFieldChange(PlayerField data)
    {
        MeshUpdate.UpdatePlayerFieldVisibility();

        //foreach (PlayerField secField in Player.inst.curSecondaryFields)          // TO DO
        //    secField.UpdatePlayerLineRenderer(data);
    }

    /// <summary>
    /// Change player visibility.
    /// </summary>
    public void OnPlayStart()
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



}
