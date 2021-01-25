﻿using System;
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
    private static int lastEdgePartID = 4;
    private static int curEdgePartID;
    private static Vector3 lastEdge_start, lastEdge_end;
    private static Vector3 lastEdge;


    //Constructor
    static PlayerData()
    {
        player = Player.inst;
        midPoint = Player.inst.transform.position;
    }



    // ----------------------------- Public methods ----------------------------



    public static void OnMovementChanged()
    {

    }


    public static void SetPositionStates()
    {
        player.lastPosState = player.positionState;

        RaycastHit hit;
        if (Physics.Raycast(midPoint, player.outerVertices[0] - midPoint, out hit))
        {
            // Calculations
            float playerRadius = (player.outerVertices[0] - midPoint).magnitude;
            float envDistance = (hit.point - midPoint).magnitude;
            float playerToEnvDistance = Mathf.Abs(playerRadius - envDistance);
            float innerVertexToEnvDistance = (player.innerVertices[0] - (hit.point + (player.innerVertices[0] - hit.point).normalized * player.stickToOuterEdge_holeSize)).magnitude;

            float stickToEdgeTolerance = player.stickToEdgeTolerance;
            if (player.actionState == Player.ActionState.stickToEdge)
                stickToEdgeTolerance *= 3f;

            // States
            if (playerToEnvDistance < stickToEdgeTolerance)
                player.positionState = Player.PositionState.innerEdge;
            else if (innerVertexToEnvDistance < stickToEdgeTolerance)
                player.positionState = Player.PositionState.outerEdge;
            else if (playerRadius < envDistance)
                player.positionState = Player.PositionState.inside;
            else
                player.positionState = Player.PositionState.outside;
        }
        else
            player.positionState = Player.PositionState.noTunnel;

        // Tunnel enter?
        if (player.positionState != Player.PositionState.noTunnel && player.lastPosState == Player.PositionState.noTunnel)
        {
            player.tunnelEnter = true;
            GameEvents.inst.TunnelStart();
        }
        else
        {
            player.tunnelEnter = false;
        }
    }



    public static void CalcEdgeData()
    {
        Vector2 intersection = Vector2.zero;
        Vector3 mousePos_extended = midPoint + (player.mousePos - midPoint).normalized * 10f;
        int curEdgeIndex = 0;
        for (int i = 0; i < player.outerVertices.Length; i++)
        {
            Vector3 playerMainVertex_extended = midPoint + ((player.outerVertices[0] - midPoint).normalized * 10f);
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, playerMainVertex_extended, midPoint, TunnelData.vertices[i], TunnelData.vertices[(i + 1) % 3]))
            {
                // Current edge (main & sec)
                player.curEdge.start = TunnelData.vertices[i];           // i beginnt immer beim environmentTriangle UNTEN LINKS!
                player.curEdge.end = TunnelData.vertices[(i + 1) % 3]; // im Uhrzeigersinn (wie alle anderen Vertex-Arrays)
                for (int j = 0; j < player.curSecEdges.Length; j++)
                {
                    player.curSecEdges[j].start = TunnelData.vertices[(i + 1 + j) % 3];
                    player.curSecEdges[j].end = TunnelData.vertices[(i + 2 + j) % 3];
                }
                curEdgeIndex = i; // for later edgePart-index
            }
        }

        // Current field & edge percentage
        RaycastHit hit;
        Physics.Raycast(midPoint, player.outerVertices[0] - midPoint, out hit);
        Vector3 playerPointOnEnv = new Vector3(hit.point.x, hit.point.y, player.outerVertices[0].z);
        player.curEdge.percentage = Mathf.Clamp01((playerPointOnEnv - player.curEdge.start).magnitude / (player.curEdge.end - player.curEdge.start).magnitude);
        curEdgePartID = ((int)(player.curEdge.percentage.
            Remap(0, 1f, 0, VisualController.inst.fieldsPerEdge)
            + curEdgeIndex * VisualController.inst.fieldsPerEdge)) % (TunnelData.vertices.Length * VisualController.inst.fieldsPerEdge); // gar kein bock mehr
        Vector3 curEdgePart_start = TunnelData.fields[curEdgePartID].start;
        Vector3 curEdgePart_end = TunnelData.fields[curEdgePartID].end;
        var curEdgePart_positions = new List<Vector3> { curEdgePart_start, curEdgePart_end };

        player.curField.ID = curEdgePartID;

        #region secondary, id
        // Seondary fields, ID
        for (int i = 0; i < player.curSecondaryFields.Length; i++)
        {
            int secID = (curEdgePartID + (i+1) * VisualController.inst.fieldsPerEdge) % VisualController.inst.FieldsCount;
            player.curSecondaryFields[i].ID = secID;
            Vector3 start = TunnelData.fields[secID].start;
            Vector3 end = TunnelData.fields[secID].end;
            var curSecFieldsPositions = new Vector3[] { start, end };
            player.curSecondaryFields[i].positions = curSecFieldsPositions;
        }
        #endregion

        // Edge part change?
        if (curEdgePartID != lastEdgePartID)
        {
            player.curField.changed = true;
        }
        else
        {
            player.curField.changed = false;
        }

        // Edge change?
        if (player.curEdge.start == lastEdge_start && player.curEdge.end == lastEdge_end)
            player.curEdge.changed = false;
        else
            player.curEdge.changed = true;

        // Is corner?
        bool isCorner = MusicField.IsCorner(curEdgePartID);
        if (isCorner)
        {
            // Add third position (left or right)
            if (MusicField.IsCorner_RightPart(curEdgePartID))
            {
                int leftCornerID = ExtensionMethods.Modulo(curEdgePartID - 1, VisualController.inst.FieldsCount);
                Vector3 leftCornerPos = TunnelData.fields[leftCornerID].start;
                curEdgePart_positions.Insert(0, leftCornerPos);

                #region secondary
                // secondary
                for (int i=0; i<player.curSecondaryFields.Length; i++)
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
                int rightCornerID = ExtensionMethods.Modulo(curEdgePartID + 1, VisualController.inst.FieldsCount);
                Vector3 rightCornerPos = TunnelData.fields[rightCornerID].end;
                curEdgePart_positions.Add(rightCornerPos);

                #region secondary
                // secondary
                for (int i = 0; i < player.curSecondaryFields.Length; i++)
                {
                    int curID = player.curSecondaryFields[i].ID;
                    rightCornerID = ExtensionMethods.Modulo(curID + 1, VisualController.inst.FieldsCount);
                    rightCornerPos = TunnelData.fields[rightCornerID].end;
                    var temp = player.curSecondaryFields[i].positions.ToList();
                    temp.Insert(0, rightCornerPos);
                    player.curSecondaryFields[i].positions = temp.ToArray();
                }
                #endregion
            }
            // No edgePartChange in corners
            bool lastIDisCorner = MusicField.IsCorner(lastEdgePartID);
            bool lastIDisClose = Mathf.Abs(curEdgePartID - lastEdgePartID) == 1 || Mathf.Abs(curEdgePartID - lastEdgePartID) == VisualController.inst.FieldsCount - 1;
            if (lastIDisCorner && lastIDisClose)
                player.curField.changed = false;
        }


        // Events (etwas unschön...)
        if (player.curField.changed)
        {
            GameEvents.inst.EdgePartChange();
        }


        // ASSIGN
        player.curField.Set(curEdgePartID, curEdgePart_positions.ToArray(), isCorner);




        // First edge touch                                                             to do: entfernen, nicht mehr nötig
        if (player.actionState == Player.ActionState.stickToEdge && player.lastActionState == Player.ActionState.none)
        {
            player.curEdge.firstTouch = true;
        }
        else
            player.curEdge.firstTouch = false;

        // Leave edge // to rework                                                      to do: entfernen, nicht mehr nötig
        if (player.actionState == Player.ActionState.none && player.lastActionState == Player.ActionState.stickToEdge)
        {
            player.curEdge.leave = true;
        }
        else
            player.curEdge.leave = false;


        // last variables
        player.lastActionState = player.actionState;
        lastEdge_start = player.curEdge.start;
        lastEdge_end = player.curEdge.end;
        lastEdgePartID = curEdgePartID;
    }



}
