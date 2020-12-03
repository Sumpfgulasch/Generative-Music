using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentData : MonoBehaviour
{
    // == calc stuff and store data in MeshRef and Player


    public static EnvironmentData inst;

    [HideInInspector] public Vector3[] envVertices = new Vector3[3];

    // Private variables
    Vector3 playerMid;

    void Start()
    {
        inst = this;
        playerMid = Player.instance.transform.position;
    }

    

    public void HandleData()
    {
        GetEnvironmentTriangle();
        SetPositionalStates();
    }




    // ----------------------------- private methods ----------------------------

    void GetEnvironmentTriangle()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        envVertices = new Vector3[3];
        Vector3 intersection = Vector3.zero;

        // 2) Prepare raycast
        for (int i = 0; i < Player.instance.verticesCount; i++)
        {
            Vector3 playerEdgeMid = Player.instance.outerVertices[i] + ((Player.instance.outerVertices[(i + 1) % 3] - Player.instance.outerVertices[i]) / 2f);
            playerEdgeMid.z = MeshRef.instance.envEdges_lr.transform.position.z;
            Vector3 directionOut = (playerEdgeMid - playerMid).normalized;
            RaycastHit hit;

            // 3) Raycasts from player to environment
            if (Physics.Raycast(playerMid, directionOut, out hit))
            {
                edgeHits[i] = hit;
            }
        }
        // 4) Final: Construct environment triangle by line intersections
        for (int i = 0; i < edgeHits.Length; i++)
        {
            Vector3 point1, point2;
            Vector3 direction1, direction2;
            point1 = edgeHits[i].point;
            point1.z = edgeHits[0].point.z;
            point2 = edgeHits[(i + 1) % 3].point;
            point2.z = edgeHits[0].point.z;
            direction1 = Vector3.Cross(Vector3.forward, edgeHits[i].normal);
            direction1.z = 0;
            direction2 = Vector3.Cross(Vector3.forward, edgeHits[(i + 1) % 3].normal);
            direction2.z = 0;

            if (ExtensionMethods.LineLineIntersection(out intersection, point1, direction1, point2, direction2))
            {
                envVertices[i] = intersection;
            }
        }
    }



    // STATES
    void SetPositionalStates()
    {
        Player player = Player.instance;
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
}
