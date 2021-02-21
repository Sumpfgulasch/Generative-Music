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
                // TO DO: verschieben
                #region secondary edges: positions
                for (int j = 0; j < Player.curSecEdges.Length; j++)
                {
                    Player.curSecEdges[j].start = TunnelData.vertices[(i + 1 + j) % TunnelData.vertices.Length];    // TO DO: player.curSecEdges sollten hier eig nicht gesetzt werden
                    Player.curSecEdges[j].end = TunnelData.vertices[(i + 2 + j) % TunnelData.vertices.Length];
                }
                #endregion
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
    /// Set player field data: ID, positions, mid, sec IDs, sec positions (+curEdge, percentage, isCorner).
    /// </summary>
    /// <returns></returns>
    public static PlayerField SetDataByID(int ID)
    {
        // 0. last-variables
        lastFieldID = Player.curField.ID;

        MusicField[] fields = TunnelData.fields;

        // 1. SET
        // Primary
        Player.curField.Set(ID, fields[ID].positions, fields[ID].mid, fields[ID].isCorner);

        // Secondary
        for (int i = 0; i < Player.curSecondaryFields.Length; i++)
        {
            int secID = (ID + ((i + 1) * (VisualController.inst.fieldsPerEdge - 1))) % TunnelData.FieldsCount;
            Player.curSecondaryFields[i].Set(secID, fields[secID].positions, fields[secID].mid, fields[secID].isCorner);
        }
        // TO DO: set current edge (start, end, ID; percentage already set)

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
