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
    private static int curFieldID;
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
            if (player.actionState == Player.ActionState.StickToEdge)
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
        #region calc
        //Vector2 intersection = Vector2.zero;
        //Vector3 mousePos_extended = midPoint + (player.mousePos - midPoint).normalized * 10f;
        //int curEdgeIndex = 0;
        //for (int i = 0; i < player.outerVertices.Length; i++)
        //{
        //    Vector3 playerMainVertex_extended = midPoint + ((player.outerVertices[0] - midPoint).normalized * 10f);
        //    if (ExtensionMethods.LineSegmentsIntersection(out intersection, playerMainVertex_extended, midPoint, TunnelData.vertices[i], TunnelData.vertices[(i + 1) % 3]))
        //    {
        //        // Current edge (main & sec)
        //        player.curEdge.start = TunnelData.vertices[i];          // i beginnt immer beim TunnelTriangle UNTEN LINKS!
        //        player.curEdge.end = TunnelData.vertices[(i + 1) % 3];
        //        for (int j = 0; j < player.curSecEdges.Length; j++)
        //        {
        //            player.curSecEdges[j].start = TunnelData.vertices[(i + 1 + j) % 3];
        //            player.curSecEdges[j].end = TunnelData.vertices[(i + 2 + j) % 3];
        //        }
        //        curEdgeIndex = i;
        //    }
        //}

        //// Current field (& edge percentage)
        //RaycastHit hit;
        //Physics.Raycast(midPoint, player.outerVertices[0] - midPoint, out hit);
        //Vector3 playerPointOnTunnel = new Vector3(hit.point.x, hit.point.y, player.outerVertices[0].z);
        //player.curEdge.percentage = Mathf.Clamp01((playerPointOnTunnel - player.curEdge.start).magnitude / (player.curEdge.end - player.curEdge.start).magnitude);
        //curFieldID = ((int)(player.curEdge.percentage.
        //    Remap(0, 1f, 0, VisualController.inst.fieldsPerEdge)
        //    + curEdgeIndex * VisualController.inst.fieldsPerEdge)) % (TunnelData.vertices.Length * VisualController.inst.fieldsPerEdge); // gar kein bock mehr
        //curFieldID = player.curField.ID;        // hack
        //Vector3 curEdgePart_start = TunnelData.fields[curFieldID].start;
        //Vector3 curEdgePart_end = TunnelData.fields[curFieldID].end;
        //var curFieldPositions = new List<Vector3> { curEdgePart_start, curEdgePart_end };

        // -------- nicht mehr
        //player.curField.ID = curFieldID;

        //#region secondary, id
        //// Seondary fields, ID
        //for (int i = 0; i < player.curSecondaryFields.Length; i++)
        //{
        //    int secID = (curFieldID + (i+1) * VisualController.inst.fieldsPerEdge) % VisualController.inst.FieldsCount;
        //    player.curSecondaryFields[i].ID = secID;
        //    Vector3 start = TunnelData.fields[secID].start;
        //    Vector3 end = TunnelData.fields[secID].end;
        //    var curSecFieldsPositions = new Vector3[] { start, end };
        //    player.curSecondaryFields[i].positions = curSecFieldsPositions;
        //}
        //#endregion
        #endregion

        

        // Edge change?
        if (player.curEdge.start == lastEdge_start && player.curEdge.end == lastEdge_end)
            player.curEdge.changed = false;
        else
            player.curEdge.changed = true;


        

        //foreach (MusicField secField in player.curSecondaryFields)
        //    secField.isCorner = isCorner;


        // Events (etwas unschön...)
        if (player.curField.changed)
        {
            GameEvents.inst.FieldChange();
        }


        // ASSIGN
        //player.curField.Set(curFieldID, curFieldPositions.ToArray(), isCorner);




        // First edge touch
        if (player.actionState == Player.ActionState.StickToEdge && player.lastActionState == Player.ActionState.none)
        {
            player.curEdge.firstTouch = true;
            //Debug.Log("first edge touch");
        }
        else
            player.curEdge.firstTouch = false;

        // Leave edge // to rework
        if (player.actionState == Player.ActionState.none && player.lastActionState == Player.ActionState.StickToEdge)
        {
            player.curEdge.leave = true;
            //Debug.Log("leave");
        }
        else
            player.curEdge.leave = false;


        //// last variables
        //player.lastActionState = player.actionState;
        //lastEdge_start = player.curEdge.start;
        //lastEdge_end = player.curEdge.end;
        //lastFieldID = curFieldID;
    }


    /// <summary>
    /// For mouse and gamepad-stick selection. Start in playerMid. [to do: dont set player.curSecEdges here]
    /// </summary>
    /// <param name="direction">Direction from mouse position to midPoint or value from gamepad stick input.</param>
    /// <returns></returns>
    public static int GetIDfromRaycast(Vector2 direction)
    {
        // 1. Cur edge
        Vector2 intersection = Vector2.zero;
        Vector3 curEdgeStart = Vector3.zero;
        Vector3 curEdgeEnd = Vector3.zero;
        Vector3 mousePos_extended = midPoint + (Vector3) direction.normalized * 10f;
        int curEdgeIndex = 0;
        for (int i = 0; i < player.outerVertices.Length; i++)
        {
            //Vector3 playerMainVertex_extended = midPoint + ((player.outerVertices[0] - midPoint).normalized * 10f);
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, mousePos_extended, midPoint, TunnelData.vertices[i], TunnelData.vertices[(i + 1) % TunnelData.vertices.Length]))
            {
                curEdgeStart = TunnelData.vertices[i];
                curEdgeEnd = TunnelData.vertices[(i + 1) % TunnelData.vertices.Length];
                #region secondary edges: positions
                for (int j = 0; j < player.curSecEdges.Length; j++)
                {
                    player.curSecEdges[j].start = TunnelData.vertices[(i + 1 + j) % TunnelData.vertices.Length];    // TO DO: player.curSecEdges sollten hier eig nicht gesetzt werden
                    player.curSecEdges[j].end = TunnelData.vertices[(i + 2 + j) % TunnelData.vertices.Length];
                }
                curEdgeIndex = i;
                #endregion
            }
        }

        // 2. Current field
        RaycastHit hit;
        Physics.Raycast(midPoint, direction, out hit);

        Vector3 pointerPosOnEdge = new Vector3(hit.point.x, hit.point.y, playerZpos);
        Debug.DrawLine(pointerPosOnEdge, midPoint, Color.red, 1f);

        player.curEdge.percentage = Mathf.Clamp01((pointerPosOnEdge - curEdgeStart).magnitude / (player.curEdge.end - curEdgeEnd).magnitude);

        curFieldID = ((int)(player.curEdge.percentage.                  // gar kein bock mehr
            Remap(0, 1f, 0, VisualController.inst.fieldsPerEdge)
            + curEdgeIndex * VisualController.inst.fieldsPerEdge)) 
            % (VisualController.inst.FieldsCount);

        return curFieldID;
    }



    /// <summary>
    /// Set data: ID, positions, sec IDs, sec positions (+curEdge, percentage, isCorner).
    /// </summary>
    /// <returns></returns>
    public static void SetDataByID(int ID)
    {
        // 0. last-variables                                // TO DO: nicht der beste ort hier (?)
        player.lastActionState = player.actionState;
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

        bool isCorner = MusicField.IsCorner(player.curField.ID);
        if (isCorner)
        {
            // Add third position (left or right)
            if (MusicField.IsCorner_RightPart(player.curField.ID))
            {
                int leftCornerID = ExtensionMethods.Modulo(player.curField.ID - 1, VisualController.inst.FieldsCount);
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
                int rightCornerID = ExtensionMethods.Modulo(player.curField.ID + 1, VisualController.inst.FieldsCount);
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

        // 2. Set (sec-field-variables get set before individually)
        player.curField.Set(ID, curFieldPositions.ToArray(), isCorner);
        
        // To do (irgendwann, wenn relevant): set cur edge
    }

    /// <summary>
    /// Check if an ID is a new field. Fire FieldChange-Events.
    /// </summary>
    public static bool FieldHasChanged(int ID)
    {
        // ID changed?
        if (player.curField.ID != lastFieldID)
        {
            // No corner?
            bool curIDisCorner = MusicField.IsCorner(player.curField.ID);
            if (!curIDisCorner)
            {
                player.curField.changed = true;     // TO DO: to remove
                //GameEvents.inst.FieldChange();
                return true;
            }
            else
            {
                // last ID corner & close?
                bool lastIDisCorner = MusicField.IsCorner(lastFieldID);
                bool lastIDisClose = Mathf.Abs(curFieldID - lastFieldID) == 1 || Mathf.Abs(curFieldID - lastFieldID) == VisualController.inst.FieldsCount - 1;
                if (lastIDisCorner && lastIDisClose)
                {
                    player.curField.changed = false;    // TO DO: to remove
                    return false;
                }
                else
                {
                    player.curField.changed = true;     // TO DO: to remove
                    //GameEvents.inst.FieldChange();
                    return true;
                }
            }
        }
        else
        {
            player.curField.changed = false;    // TO DO: to remove
            return false;
        }
    }



    public static void AddCornerPosition(int ID)
    {
        
    }



}
