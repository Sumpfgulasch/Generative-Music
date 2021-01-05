using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerData
{
    //public static PlayerData inst;

    // Private variables
    private static Player player;
    private static Vector3 midPoint;
    private static int lastEdgePartID;
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


    public static void SetActionStates()
    {
        player.lastActionState = player.actionState;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            player.actionState = Player.ActionState.stickToEdge;
        else
            player.actionState = Player.ActionState.move;
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
            if (playerToEnvDistance < stickToEdgeTolerance && !player.startedBounce) // player.actionState == Player.ActionState.stickToEdge && (player.positionState == Player.PositionState.inside || player.positionState == Player.PositionState.innerEdge)
                player.positionState = Player.PositionState.innerEdge;
            else if (innerVertexToEnvDistance < stickToEdgeTolerance && !player.startedBounce) // player.actionState == Player.ActionState.stickToEdge && (player.positionState == Player.PositionState.outside || player.positionState == Player.PositionState.outerEdge)
                player.positionState = Player.PositionState.outerEdge;
            else if (playerRadius < envDistance)
                player.positionState = Player.PositionState.inside;
            else
                player.positionState = Player.PositionState.outside;
        }
        else
            player.positionState = Player.PositionState.noTunnel;
    }



    public static void CalcEdgeData()
    {
        // Berechne nur auf Edges
        if (player.actionState == Player.ActionState.stickToEdge)
        {
            // Last-variables
            lastEdge_start = player.curEdge.start;
            lastEdge_end = player.curEdge.end;
            lastEdgePartID = curEdgePartID;


            Vector2 intersection = Vector2.zero;
            Vector3 mousePos_extended = midPoint + (player.mousePos - midPoint).normalized * 10f;
            int curEdgeIndex = 0;
            for (int i = 0; i < player.outerVertices.Length; i++)
            {
                Vector3 playerMainVertex_extended = midPoint + ((player.outerVertices[0] - midPoint).normalized * 10f);
                if (ExtensionMethods.LineSegmentsIntersection(out intersection, playerMainVertex_extended, midPoint, EnvironmentData.vertices[i], EnvironmentData.vertices[(i + 1) % 3]))
                {
                    // Current edge (main & sec)
                    player.curEdge.start = EnvironmentData.vertices[i];           // i beginnt immer beim environmentTriangle UNTEN LINKS!
                    player.curEdge.end = EnvironmentData.vertices[(i + 1) % 3]; // im Uhrzeigersinn (wie alle anderen Vertex-Arrays)
                    for (int j = 0; j < player.curSecEdges.Length; j++)
                    {
                        player.curSecEdges[j].start = EnvironmentData.vertices[(i + 1 + j) % 3];
                        player.curSecEdges[j].end = EnvironmentData.vertices[(i + 2 + j) % 3];
                    }
                    curEdgeIndex = i; // for later edgePart-index
                }
            }

            // Current edgePart & edgePartPercentage
            RaycastHit hit;
            Physics.Raycast(midPoint, player.outerVertices[0] - midPoint, out hit);
            Vector3 playerPointOnEnv = new Vector3(hit.point.x, hit.point.y, player.outerVertices[0].z);
            player.curEdge.percentage = Mathf.Clamp01((playerPointOnEnv - player.curEdge.start).magnitude / (player.curEdge.end - player.curEdge.start).magnitude);
            curEdgePartID = ((int)(player.curEdge.percentage.
                Remap(0, 1f, 0, VisualController.inst.envGridLoops)
                + curEdgeIndex * VisualController.inst.envGridLoops)) % (EnvironmentData.vertices.Length * VisualController.inst.envGridLoops); // gar kein bock mehr
            Vector3 curEdgePart_start = EnvironmentData.edgeParts[curEdgePartID].start;
            Vector3 curEdgePart_end = EnvironmentData.edgeParts[curEdgePartID].end;
            var curEdgePart_positions = new List<Vector3> { curEdgePart_start, curEdgePart_end };

            // Edge part change?
            //Debug.Log("curEdgePartID: " + curEdgePartID + ", lastEdgePartID: " + lastEdgePartID + ", percentage: " + player.curEdge.percentage + ", curEdgeIndex: " + curEdgeIndex);
            if (curEdgePartID != lastEdgePartID)
                player.curEdgePart.changed = true;
            else
                player.curEdgePart.changed = false;

            // Edge change?
            if (player.curEdge.start == lastEdge_start && player.curEdge.end == lastEdge_end)
                player.curEdge.changed = false;
            else
                player.curEdge.changed = true;

            // Is corner?
            bool isCorner = (curEdgePartID + 1) % VisualController.inst.envGridLoops == 0 || curEdgePartID % VisualController.inst.envGridLoops == 0;
            //bool isCorner = EdgePart.IsCorner(curEdgePartID);
            if (isCorner)
            {
                // Add third position
                if (EdgePart.IsCorner_RightPart(curEdgePartID))
                {
                    int leftCornerID = ExtensionMethods.Modulo(curEdgePartID - 1, VisualController.inst.EdgePartCount);
                    Vector3 leftCornerPos = EnvironmentData.edgeParts[leftCornerID].start;
                    curEdgePart_positions.Insert(0, leftCornerPos);
                }
                else
                {
                    int rightCornerID = ExtensionMethods.Modulo(curEdgePartID + 1, VisualController.inst.EdgePartCount);
                    Vector3 rightCornerPos = EnvironmentData.edgeParts[rightCornerID].end;
                    curEdgePart_positions.Add(rightCornerPos);
                }
                // No edgePartChange in corners
                bool lastIDisCorner = (lastEdgePartID + 1) % VisualController.inst.envGridLoops == 0 || lastEdgePartID % VisualController.inst.envGridLoops == 0;
                //bool lastIDisCorner = EdgePart.IsCorner(lastEdgePartID);
                if (lastIDisCorner)
                    player.curEdgePart.changed = false;
            }

            // ASSIGN
            player.curEdgePart.Set(curEdgePartID, curEdgePart_start, curEdgePart_end, isCorner);
        }
        
        
        // First edge touch
        if (player.actionState == Player.ActionState.stickToEdge && player.lastActionState == Player.ActionState.move)
        {
            player.curEdge.firstTouch = true;
        }
        else
            player.curEdge.firstTouch = false;

        // Leave edge // to rework
        if (player.actionState == Player.ActionState.move && player.lastActionState == Player.ActionState.stickToEdge)
        {
            player.curEdge.leave = true;
        }
        else
            player.curEdge.leave = false;
        
        
    }
}
