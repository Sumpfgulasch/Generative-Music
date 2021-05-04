using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public static class PlayerData
{
    // Private variables
    //private static Player player;
    private static Vector3 midPoint;
    private static int lastFieldID = 4;
    private static float playerZpos;

    // Properties
    private static Player Player { get { return Player.inst; } }


    //Constructor
    static PlayerData()
    {
        midPoint = Player.inst.transform.position;
        playerZpos = midPoint.z;
    }



    // ----------------------------- Public methods ----------------------------
    



    /// <summary>
    /// Return the current ID. Start in playerMid and send ray. For mouse and gamepad-stick selection. Set curEdge.percentage (!).
    /// </summary>
    /// <param name="direction">Direction from mouse position to midPoint or value from gamepad stick input.</param>
    public static int GetIDfromRaycast(Vector2 direction)
    {
        // 1. Cur edge
        Vector2 intersection;
        Vector3 mousePos_extended = midPoint + (Vector3) direction.normalized * 10f;
        Vector3 curEdgeStart = Vector3.zero;
        Vector3 curEdgeEnd = Vector3.zero;
        int curEdgeIndex = 0;
        for (int i = 0; i < Player.OuterVertices.Length; i++)
        {
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, mousePos_extended, midPoint, TunnelData.vertices[i], TunnelData.vertices[(i + 1) % TunnelData.vertices.Length]))
            {
                curEdgeStart = TunnelData.vertices[i];
                curEdgeEnd = TunnelData.vertices[(i + 1) % TunnelData.vertices.Length];
                curEdgeIndex = i;
            }
        }

        // 2. Current field
        Physics.Raycast(midPoint, direction, out RaycastHit hit);

        Vector3 pointerPosOnEdge = new Vector3(hit.point.x, hit.point.y, playerZpos);
        
        Player.curEdge.percentage = Mathf.Clamp01((pointerPosOnEdge - curEdgeStart).magnitude / (curEdgeEnd - curEdgeStart).magnitude);
        int curEdgePercentage_quantized = (int)(Player.curEdge.percentage.Remap(0, 1f, 0, VisualController.inst.fieldsPerEdge));

        int fieldsPerEdge = VisualController.inst.fieldsPerEdge;

        int curFieldID = (curEdgeIndex * (fieldsPerEdge - 1) + curEdgePercentage_quantized) % TunnelData.FieldsCount;

        return curFieldID;
    }



    /// <summary>
    /// Change curField- and lastField-references.
    /// </summary>
    /// <returns></returns>
    public static MusicField SetDataByID(int ID)
    {
        // 0. last-variables
        lastFieldID = Player.curField.ID;
        Player.inst.lastField = Player.inst.curField;

        // 1. SET
        Player.inst.curField = Player.inst.curFieldSet[ID];

        //Debug.Log("curFieldID: " + Player.inst.curField.ID + ", lastFieldID: " + Player.inst.lastField.ID);

        return Player.curField;

    }

    /// <summary>
    /// Check if an ID is a new field.
    /// </summary>
    public static bool FieldHasChanged()
    {
        int curID = Player.curField.ID;

        if (curID != lastFieldID)
            return true;
        else
            return false;
    }



}
