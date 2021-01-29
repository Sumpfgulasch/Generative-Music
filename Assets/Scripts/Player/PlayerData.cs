using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public static class PlayerData
{
    // Private variables
    private static Player player;
    private static Vector3 midPoint;
    private static int lastFieldID = 4;
    private static Vector3 lastEdge_start, lastEdge_end;
    private static Vector3 lastEdge;
    private static float playerZpos;


    //Constructor
    static PlayerData()
    {
        player = Player.inst;
        midPoint = Player.inst.transform.position;
        player.curField.ID = 4;
        playerZpos = midPoint.z;
    }



    // ----------------------------- Public methods ----------------------------


    #region SetPositionStates
    //public static void SetPositionStates()
    //{
    //    player.lastPosState = player.positionState;

    //    RaycastHit hit;
    //    if (Physics.Raycast(midPoint, player.outerVertices[0] - midPoint, out hit))
    //    {
    //        // Calculations
    //        float playerRadius = (player.outerVertices[0] - midPoint).magnitude;
    //        float envDistance = (hit.point - midPoint).magnitude;
    //        float playerToEnvDistance = Mathf.Abs(playerRadius - envDistance);
    //        float innerVertexToEnvDistance = (player.innerVertices[0] - (hit.point + (player.innerVertices[0] - hit.point).normalized * player.stickToOuterEdge_holeSize)).magnitude;

    //        float stickToEdgeTolerance = player.stickToEdgeTolerance;
    //        if (player.actionState == Player.ActionState.Play)
    //            stickToEdgeTolerance *= 3f;

    //        // States
    //        if (playerToEnvDistance < stickToEdgeTolerance)
    //            player.positionState = Player.PositionState.InnerEdge;
    //        else if (innerVertexToEnvDistance < stickToEdgeTolerance)
    //            player.positionState = Player.PositionState.OuterEdge;
    //        else if (playerRadius < envDistance)
    //            player.positionState = Player.PositionState.Inside;
    //        else
    //            player.positionState = Player.PositionState.Outside;
    //    }
    //    else
    //        player.positionState = Player.PositionState.NoTunnel;

    //    // Tunnel enter?
    //    if (player.positionState != Player.PositionState.NoTunnel && player.lastPosState == Player.PositionState.NoTunnel)
    //    {
    //        player.tunnelEnter = true;
    //        GameEvents.inst.TunnelStart();
    //    }
    //    else
    //    {
    //        player.tunnelEnter = false;
    //    }
    //}
    #endregion

        

    /// <summary>
    /// Return the current ID. For mouse and gamepad-stick selection. Start in playerMid and send ray. Set curEdges (!).
    /// </summary>
    /// <param name="direction">Direction from mouse position to midPoint or value from gamepad stick input.</param>
    public static int GetIDfromRaycast(Vector2 direction)
    {
        // 1. Cur edge
        Vector2 intersection;
        Vector3 mousePos_extended = midPoint + (Vector3) direction.normalized * 10f;
        int curEdgeIndex = 0;
        for (int i = 0; i < player.outerVertices.Length; i++)
        {
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, mousePos_extended, midPoint, TunnelData.vertices[i], TunnelData.vertices[(i + 1) % TunnelData.vertices.Length]))
            {
                player.curEdge.start = TunnelData.vertices[i];
                player.curEdge.end = TunnelData.vertices[(i + 1) % TunnelData.vertices.Length];
                #region secondary edges: positions
                for (int j = 0; j < player.curSecEdges.Length; j++)
                {
                    player.curSecEdges[j].start = TunnelData.vertices[(i + 1 + j) % TunnelData.vertices.Length];    // TO DO: player.curSecEdges sollten hier eig nicht gesetzt werden
                    player.curSecEdges[j].end = TunnelData.vertices[(i + 2 + j) % TunnelData.vertices.Length];
                }
                #endregion
                curEdgeIndex = i;
                // To do (irgendwann, wenn relevant): set cur edge ID
            }
        }

        // 2. Current field
        RaycastHit hit;
        Physics.Raycast(midPoint, direction, out hit);

        Vector3 pointerPosOnEdge = new Vector3(hit.point.x, hit.point.y, playerZpos);

        player.curEdge.percentage = Mathf.Clamp01((pointerPosOnEdge - player.curEdge.start).magnitude / (player.curEdge.end - player.curEdge.start).magnitude);
        
        int curFieldID = ((int)(player.curEdge.percentage.                  // gar kein bock mehr
            Remap(0, 1f, 0, VisualController.inst.fieldsPerEdge)
            + curEdgeIndex * VisualController.inst.fieldsPerEdge)) 
            % (VisualController.inst.FieldsCount);

        return curFieldID;
    }



    /// <summary>
    /// Set data: ID, positions, sec IDs, sec positions (+curEdge, percentage, isCorner).
    /// </summary>
    /// <returns></returns>
    public static PlayerField GetDataByID(int ID)
    {
        // 0. last-variables
        lastEdge_start = player.curEdge.start;
        lastEdge_end = player.curEdge.end;
        lastFieldID = player.curField.ID;


        // 1. Calc positions & isCorner
        Vector3 curFieldStart = TunnelData.fields[ID].start;
        Vector3 curFieldEnd = TunnelData.fields[ID].end;
        var curFieldPositions = new List<Vector3> { curFieldStart, curFieldEnd };
        #region secondary: set ID & positions
        // Seondary fields, ID
        for (int i = 0; i < player.curSecondaryFields.Length; i++)
        {
            int secID = (ID + (i + 1) * VisualController.inst.fieldsPerEdge) % VisualController.inst.FieldsCount;
            player.curSecondaryFields[i].ID = secID;
            Vector3 start = TunnelData.fields[secID].start;
            Vector3 end = TunnelData.fields[secID].end;
            var curSecFieldsPositions = new Vector3[] { start, end };
            player.curSecondaryFields[i].positions = curSecFieldsPositions;
        }
        #endregion

        bool isCorner = MusicField.IsCorner(ID);
        if (isCorner)
        {
            // Add third position (left or right)
            if (MusicField.IsCorner_RightPart(ID))
            {
                int leftCornerID = ExtensionMethods.Modulo(ID - 1, VisualController.inst.FieldsCount);
                Vector3 leftCornerPos = TunnelData.fields[leftCornerID].start;
                curFieldPositions.Insert(0, leftCornerPos);

                #region secondary: add pos
                // secondary
                for (int i = 0; i < player.curSecondaryFields.Length; i++)
                {
                    int curID = player.curSecondaryFields[i].ID;
                    leftCornerID = ExtensionMethods.Modulo(curID - 1, VisualController.inst.FieldsCount);
                    leftCornerPos = TunnelData.fields[leftCornerID].start;
                    var temp = player.curSecondaryFields[i].positions.ToList();
                    temp.Insert(0, leftCornerPos);
                    player.curSecondaryFields[i].positions = temp.ToArray();
                }
                #endregion
            }
            else
            {
                int rightCornerID = ExtensionMethods.Modulo(ID + 1, VisualController.inst.FieldsCount);
                Vector3 rightCornerPos = TunnelData.fields[rightCornerID].end;
                curFieldPositions.Add(rightCornerPos);

                #region secondary: add pos
                // secondary
                for (int i = 0; i < player.curSecondaryFields.Length; i++)
                {
                    int curID = player.curSecondaryFields[i].ID;
                    rightCornerID = ExtensionMethods.Modulo(curID + 1, VisualController.inst.FieldsCount);
                    rightCornerPos = TunnelData.fields[rightCornerID].end;
                    var temp = player.curSecondaryFields[i].positions.ToList();
                    temp.Add(rightCornerPos);
                    player.curSecondaryFields[i].positions = temp.ToArray();
                }
                #endregion
            }
        }
        foreach (MusicField secField in player.curSecondaryFields)
            secField.isCorner = isCorner;

        // 2. Set                                               // TO DO: to remove?
        player.curField.Set(ID, curFieldPositions.ToArray(), isCorner);
        // (sec - field - variables get set before individually)

        PlayerField data = new PlayerField(ID, curFieldPositions.ToArray(), isCorner);

        return data;

    }

    /// <summary>
    /// Check if an ID is a new field. Fire FieldChange-Events.
    /// </summary>
    public static bool FieldHasChanged()
    {
        int curID = player.curField.ID;

        // ID changed?
        if (curID != lastFieldID)
        {
            // No corner?
            bool curIDisCorner = MusicField.IsCorner(player.curField.ID);
            if (!curIDisCorner)
            {
                return true;
            }
            else
            {
                // last ID corner & close?
                bool lastIDisCorner = MusicField.IsCorner(lastFieldID);
                bool lastIDisClose = Mathf.Abs(curID - lastFieldID) == 1 || Mathf.Abs(curID - lastFieldID) == VisualController.inst.FieldsCount - 1;
                if (lastIDisCorner && lastIDisClose)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            return false;
        }
    }



}
