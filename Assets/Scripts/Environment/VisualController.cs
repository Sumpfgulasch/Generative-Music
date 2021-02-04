using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VisualController : MonoBehaviour
{
    // public
    public static VisualController inst;

    [Header("Settings")]
    public int fieldsPerEdge = 6;
    public int tunnelVertices = 3;
    public bool showCursor = true;
    public bool showMilkSurface = false;
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
    [Range(0.1f, 1)]
    public float mouseColliderSize_play = 1;
    //[Range(0.1f, 1)]
    //public float mouseColliderSize_move = 0.3f;

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
    public void OnPlay(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Player.inst.curField.SetToPlay();
            foreach (PlayerField secField in Player.inst.curSecondaryFields)
                secField.SetVisible(true);
        }

        else if (context.canceled)
        {
            Player.inst.curField.SetToFocus();
            foreach (PlayerField secField in Player.inst.curSecondaryFields)
                secField.SetVisible(false);
        }
    }


    public void OnMove(InputAction.CallbackContext context)
    {

    }


    /// <summary>
    /// Change the played outer field.
    /// </summary>
    private void OnMouseInside()
    {
        if (Player.actionState == Player.ActionState.Play)
        {
            Player.curField.SetOpacity(0.6f);
            print("0.7");
        }
        else
        {
            Player.curField.SetOpacity(0.02f);
            print("0.2");
        }
    }

    private void OnMouseOutside()
    {
        if (Player.actionState == Player.ActionState.Play)
        {
            Player.curField.SetOpacity(1);
            print("1");
        }
        else
        {
            Player.curField.SetOpacity(0.3f);
            print("0.5");
        }
    }



}
