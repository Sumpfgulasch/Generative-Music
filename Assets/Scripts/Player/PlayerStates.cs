using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStates
{
    // Private variables
    private static Player player;
    private static Vector3 playerMid;


    // Constructor
    static PlayerStates()
    {
        player = Player.inst;
        playerMid = Player.inst.transform.position;
    }



    // ----------------------------- Public methods ----------------------------

    public static void SetActionStates()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            player.actionState = Player.ActionState.stickToEdge;
        else
            player.actionState = Player.ActionState.none;
    }


    public static void SetPositionalStates()
    {
        player.lastPosState = player.positionState;
        RaycastHit hit;
        if (Physics.Raycast(playerMid, player.outerVertices[0] - playerMid, out hit))
        {
            // Calculations
            float playerRadius = (player.outerVertices[0] - playerMid).magnitude;
            float envDistance = (hit.point - playerMid).magnitude;
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

            if (player.positionState == Player.PositionState.innerEdge && player.lastPosState != Player.PositionState.innerEdge ||
                player.positionState == Player.PositionState.outerEdge && player.lastPosState != Player.PositionState.outerEdge)
                player.firstEdgeTouch = true;
            else
                player.firstEdgeTouch = false;
        }
        else
            player.positionState = Player.PositionState.noTunnel;
    }


    public static void CalcEdgeData()
    {
        // letzte 4 zeilen
        player.lastPosState = player.positionState;
        RaycastHit hit;
        if (Physics.Raycast(playerMid, player.outerVertices[0] - playerMid, out hit))
        {

        }
    }
}
