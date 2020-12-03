using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerData
{
    //public static PlayerData inst;

    // Private variables
    private static Player player;
    private static Vector3 midPoint;


    //Constructor
    static PlayerData()
    {
        player = Player.inst;
        midPoint = Player.inst.transform.position;
    }
    


    // ----------------------------- Public methods ----------------------------


    public static void SetActionStates()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            player.actionState = Player.ActionState.stickToEdge;
        else
            player.actionState = Player.ActionState.none;
    }



    public static void SetPositionStates()
    {
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
            if (playerToEnvDistance < stickToEdgeTolerance && !player.startedBounce)
                player.positionState = Player.PositionState.innerEdge;
            else if (innerVertexToEnvDistance < stickToEdgeTolerance && !player.startedBounce)
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
        // Last-variables
        player.lastEnvEdge = player.curEdge;

        // First edge touch?
        player.lastPosState = player.positionState;
        RaycastHit hit;
        if (Physics.Raycast(midPoint, player.outerVertices[0] - midPoint, out hit)) // gleicher raycast wie in SetPositionStates()
        {
            if (player.positionState == Player.PositionState.innerEdge && player.lastPosState != Player.PositionState.innerEdge ||
                player.positionState == Player.PositionState.outerEdge && player.lastPosState != Player.PositionState.outerEdge)
                player.firstEdgeTouch = true;
            else
                player.firstEdgeTouch = false;
        }

        
        Vector2 intersection = Vector2.zero;
        Vector3 mousePos_extended = midPoint + (player.mousePos - midPoint).normalized * 10f;
        int curEdgeIndex = 0;
        Vector3 curEdgeStart, curEdgeEnd;
        for (int i = 0; i < player.outerVertices.Length; i++)
        {
            Vector3 playerMainVertex_extended = midPoint + ((player.outerVertices[0] - midPoint).normalized * 10f);
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, playerMainVertex_extended, midPoint, EnvironmentData.vertices[i], EnvironmentData.vertices[(i + 1) % 3]))
            {
                // Current edge
                player.curEdge.Item1 = EnvironmentData.vertices[(i + 1) % 3]; // im Uhrzeigersinn (anders als alle anderen Vertex-Arrays)
                player.curEdge.Item2 = EnvironmentData.vertices[i];           // i beginnt immer beim environmentTriangle UNTEN LINKS!
                player.curEdge_2nd.Item1 = EnvironmentData.vertices[(i + 2) % 3];
                player.curEdge_2nd.Item2 = EnvironmentData.vertices[(i + 1) % 3];
                player.curEdge_3rd.Item1 = EnvironmentData.vertices[(i + 3) % 3];
                player.curEdge_3rd.Item2 = EnvironmentData.vertices[(i + 2) % 3];

                curEdgeIndex = (EnvironmentData.vertices.Length - i) % EnvironmentData.vertices.Length; // for later edgePart-index; kompliziert weil i eigentlich gegen Uhrzeigersinn; will i IM Uhrzeigersinn
            }
        }
        
        // Edge change?
        if (player.curEdge.Item1 == player.lastEnvEdge.Item1 && player.curEdge.Item2 == player.lastEnvEdge.Item2)
            player.edgeChange = false;
        else
            player.edgeChange = true;

        // Edge part change?
        if (player.curEnvEdgePart != player.lastEnvEdgePart)
            player.edgePartChange = true;
        else
            player.edgePartChange = false;
        player.lastEnvEdgePart = player.curEnvEdgePart;

        // Current edgePart & edgePartPercentage
        player.curEnvEdgePercentage = (player.outerVertices[0] - player.curEdge.Item1).magnitude / (player.curEdge.Item2 - player.curEdge.Item1).magnitude;
        player.curEnvEdgePart = (int)player.curEnvEdgePercentage.Remap(0, 1f, 0, VisualController.inst.envGridLoops);
        int edgePartID = (int) player.curEnvEdgePercentage.Remap(0, 1f, 0, VisualController.inst.envGridLoops) + curEdgeIndex * VisualController.inst.envGridLoops;
        //Vector3 curEdgePart_start = 

        // Is corner?
        bool isCorner = false;
        if (edgePartID % VisualController.inst.envGridLoops == 0 || edgePartID % (VisualController.inst.envGridLoops - 1) == 0) {
            isCorner = true;
            player.edgePartChange = false;
        }
        if (player.edgePartChange)
            Debug.Log("isCorner: " + isCorner);

        // Assign
        //player.curEdgePart.Set(edgePartID, )
    }

    public static void SetupEdgeParts()
    {
        player.curEdgePart = new EdgePart(EdgePart.Type.PlayerMain);
        player.curSecEdgeParts = new EdgePart[player.verticesCount - 1];
        for (int i=0; i<player.curSecEdgeParts.Length; i++)
            player.curSecEdgeParts[i] = new EdgePart(EdgePart.Type.PlayerSec);
    }
}
